#!/usr/bin/env python3
"""
Analyze Claude Code session transcripts for token/cost patterns.

Usage:
  python3 analyze_transcripts.py <project_transcript_dir> [n_sessions]

Parses JSONL session logs, computes per-turn cost using the 2026-04-17 price
table (see reference/pricing.md), and emits an aggregate report.

Outputs to stdout as text + also writes JSON summary to:
  /tmp/harness-audit-transcripts.json

Schema dependencies (verified at step 0 of the skill):
  - line.type == "assistant"
  - line.message.usage.input_tokens
  - line.message.usage.output_tokens
  - line.message.usage.cache_creation_input_tokens
  - line.message.usage.cache_creation.{ephemeral_5m,ephemeral_1h}_input_tokens
  - line.message.usage.cache_read_input_tokens
  - line.message.model (opus|sonnet|haiku substring match)
"""
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path

# Prices per 1M tokens, USD. Version marker: 2026-04-17
PRICES = {
    "opus":   {"in": 15, "out": 75, "c5": 18.75, "c1": 30, "cr": 1.5},
    "sonnet": {"in": 3,  "out": 15, "c5": 3.75,  "c1": 6,  "cr": 0.3},
    "haiku":  {"in": 1,  "out": 5,  "c5": 1.25,  "c1": 2,  "cr": 0.1},
}
PRICE_VERSION = "2026-04-17"


def model_family(model_id: str) -> str:
    for key in ("opus", "sonnet", "haiku"):
        if key in (model_id or "").lower():
            return key
    return "opus"  # conservative default


def turn_cost(usage: dict, family: str) -> dict:
    p = PRICES[family]
    cc = usage.get("cache_creation", {}) or {}
    c5 = cc.get("ephemeral_5m_input_tokens", 0)
    c1 = cc.get("ephemeral_1h_input_tokens", 0)
    if c5 + c1 == 0:
        c5 = usage.get("cache_creation_input_tokens", 0)
    inp = usage.get("input_tokens", 0)
    out = usage.get("output_tokens", 0)
    cr = usage.get("cache_read_input_tokens", 0)
    return {
        "cost": (inp * p["in"] + out * p["out"] + c5 * p["c5"] + c1 * p["c1"] + cr * p["cr"]) / 1_000_000,
        "tokens_in": inp,
        "tokens_out": out,
        "tokens_c5": c5,
        "tokens_c1": c1,
        "tokens_cr": cr,
    }


def analyze_session(path: Path) -> dict:
    tool_calls = Counter()
    tool_result_approx = defaultdict(int)
    id_to_name = {}
    turns = 0
    costs = []
    models = Counter()
    with open(path) as f:
        for line in f:
            try:
                o = json.loads(line)
            except Exception:
                continue
            t = o.get("type")
            if t == "assistant":
                m = o.get("message", {}) or {}
                u = m.get("usage", {}) or {}
                fam = model_family(m.get("model", ""))
                models[fam] += 1
                tc = turn_cost(u, fam)
                costs.append(tc["cost"])
                turns += 1
                for c in (m.get("content") or []):
                    if c.get("type") == "tool_use":
                        tool_calls[c.get("name", "?")] += 1
                        id_to_name[c.get("id")] = c.get("name", "?")
            elif t == "user":
                m = o.get("message", {}) or {}
                content = m.get("content")
                if isinstance(content, list):
                    for c in content:
                        if c.get("type") == "tool_result":
                            name = id_to_name.get(c.get("tool_use_id"), "?")
                            inner = c.get("content")
                            if isinstance(inner, list):
                                sz = sum(len(str(x.get("text", ""))) for x in inner if isinstance(x, dict))
                            else:
                                sz = len(str(inner or ""))
                            tool_result_approx[name] += sz // 4
    costs.sort()
    return {
        "path": str(path),
        "turns": turns,
        "total_cost": sum(costs),
        "max_turn_cost": max(costs) if costs else 0,
        "p50_cost": costs[len(costs) // 2] if costs else 0,
        "p95_cost": costs[int(len(costs) * 0.95)] if costs else 0,
        "tool_calls": dict(tool_calls.most_common()),
        "tool_result_tokens": dict(sorted(tool_result_approx.items(), key=lambda x: -x[1])),
        "models": dict(models),
    }


def main():
    if len(sys.argv) < 2:
        print("Usage: analyze_transcripts.py <project_transcript_dir> [n_sessions]", file=sys.stderr)
        sys.exit(1)
    target = Path(sys.argv[1]).expanduser()
    n = int(sys.argv[2]) if len(sys.argv) > 2 else 15
    if not target.is_dir():
        print(f"[ERROR] Not a directory: {target}", file=sys.stderr)
        sys.exit(2)
    files = sorted(target.glob("*.jsonl"), key=lambda p: p.stat().st_mtime, reverse=True)[:n]
    if len(files) < 10:
        print(f"[WARN] Only {len(files)} session(s) available — need ≥10 for stable analysis", file=sys.stderr)

    sessions = [analyze_session(p) for p in files]
    agg_cost = 0.0
    agg_tool_calls = Counter()
    agg_tool_result = Counter()
    all_turn_costs = []
    for s in sessions:
        agg_cost += s["total_cost"]
        for k, v in s["tool_calls"].items():
            agg_tool_calls[k] += v
        for k, v in s["tool_result_tokens"].items():
            agg_tool_result[k] += v

    summary = {
        "price_version": PRICE_VERSION,
        "session_count": len(sessions),
        "total_cost_usd": round(agg_cost, 2),
        "tool_calls_top": dict(agg_tool_calls.most_common(15)),
        "tool_result_tokens_top": dict(sorted(agg_tool_result.items(), key=lambda x: -x[1])[:10]),
        "sessions": sessions,
    }

    out = Path("/tmp/harness-audit-transcripts.json")
    out.write_text(json.dumps(summary, indent=2))

    # Human-readable stdout
    print(f"=== Transcripts analyzed: {len(sessions)} sessions ===")
    print(f"Price version: {PRICE_VERSION}")
    print(f"Total estimated cost: ${agg_cost:.2f}\n")
    print(f"{'file':<42s}  {'turns':>5}  {'cost$':>7}  {'max_turn$':>10}")
    for s in sessions:
        name = Path(s["path"]).name[:40]
        print(f"{name:<42s}  {s['turns']:>5d}  {s['total_cost']:>7.2f}  {s['max_turn_cost']:>10.2f}")
    print("\nTop tools by call count:")
    for name, n in agg_tool_calls.most_common(10):
        rt = agg_tool_result.get(name, 0)
        print(f"  {name:<28s} {n:>5d} calls, ~{rt:>10,d} result tokens")
    print(f"\nJSON summary: {out}")


if __name__ == "__main__":
    main()
