#!/usr/bin/env python3
"""
Measure the per-session baseline context size.

The first assistant turn's `cache_creation_input_tokens` is the fixed cost
that enters every session: CLAUDE.md cascade + @-imports + memory files +
MCP instructions + system prompt + skills list + tool schemas.

Usage:
  python3 measure_baseline.py <project_transcript_dir> [n_sessions]

Output:
  - stdout: per-session first-turn tokens + average
  - JSON: /tmp/harness-audit-baseline.json
"""
import json
import sys
from pathlib import Path


def first_turn_tokens(path: Path):
    with open(path) as f:
        for line in f:
            try:
                o = json.loads(line)
            except Exception:
                continue
            if o.get("type") != "assistant":
                continue
            u = o.get("message", {}).get("usage", {}) or {}
            cc_total = u.get("cache_creation_input_tokens", 0)
            cache_read = u.get("cache_read_input_tokens", 0)
            inp = u.get("input_tokens", 0)
            return {
                "cache_creation": cc_total,
                "cache_read": cache_read,
                "input": inp,
                "baseline_total": cc_total + cache_read,
                "model": o.get("message", {}).get("model", ""),
            }
    return None


def main():
    if len(sys.argv) < 2:
        print("Usage: measure_baseline.py <project_transcript_dir> [n_sessions]", file=sys.stderr)
        sys.exit(1)
    target = Path(sys.argv[1]).expanduser()
    n = int(sys.argv[2]) if len(sys.argv) > 2 else 10
    files = sorted(target.glob("*.jsonl"), key=lambda p: p.stat().st_mtime, reverse=True)[:n]

    results = []
    cc_values = []
    for p in files:
        r = first_turn_tokens(p)
        if r and r["cache_creation"] > 0:
            r["session"] = p.name
            results.append(r)
            cc_values.append(r["cache_creation"])

    if not cc_values:
        print("[ERROR] No sessions with measurable baseline found.", file=sys.stderr)
        sys.exit(3)

    avg = sum(cc_values) / len(cc_values)
    summary = {
        "session_count": len(results),
        "baseline_avg_tokens": round(avg),
        "baseline_min_tokens": min(cc_values),
        "baseline_max_tokens": max(cc_values),
        "sessions": results,
    }
    out = Path("/tmp/harness-audit-baseline.json")
    out.write_text(json.dumps(summary, indent=2))

    print(f"=== Baseline (first-turn cache_creation) over {len(results)} sessions ===")
    print(f"{'session':<44s}  {'tokens':>8}  {'model':<20}")
    for r in results:
        print(f"{r['session'][:40]:<44s}  {r['cache_creation']:>8,d}  {r['model']:<20}")
    print(f"\nAverage baseline: {avg:,.0f} tokens")
    print(f"Min: {min(cc_values):,}  Max: {max(cc_values):,}")
    print(f"\nJSON summary: {out}")


if __name__ == "__main__":
    main()
