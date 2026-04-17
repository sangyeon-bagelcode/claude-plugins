#!/usr/bin/env python3
"""
Aggregate MCP / tool usage across ALL projects in ~/.claude/projects/.

Purpose: judge whether a globally-configured MCP server is actually used
somewhere (keep it) or nowhere (safe to remove globally).

Usage:
  python3 cross_project_usage.py

Output:
  - stdout: server → project → call count, sorted by total
  - JSON: /tmp/harness-audit-cross-usage.json
"""
import json
import sys
from collections import defaultdict
from pathlib import Path


def main():
    root = Path.home() / ".claude" / "projects"
    if not root.is_dir():
        print(f"[ERROR] Not found: {root}", file=sys.stderr)
        sys.exit(2)

    usage = defaultdict(lambda: defaultdict(int))
    total_sessions = 0

    for proj_dir in root.iterdir():
        if not proj_dir.is_dir():
            continue
        for p in proj_dir.glob("*.jsonl"):
            total_sessions += 1
            try:
                with open(p) as f:
                    for line in f:
                        try:
                            o = json.loads(line)
                        except Exception:
                            continue
                        if o.get("type") != "assistant":
                            continue
                        content = o.get("message", {}).get("content") or []
                        if not isinstance(content, list):
                            continue
                        for c in content:
                            if c.get("type") == "tool_use":
                                name = c.get("name", "")
                                if name.startswith("mcp__"):
                                    parts = name.split("__")
                                    server = parts[1] if len(parts) > 1 else name
                                    usage[server][proj_dir.name] += 1
            except Exception:
                continue

    summary = {
        "total_sessions_scanned": total_sessions,
        "servers": {},
    }
    for srv, projs in usage.items():
        total = sum(projs.values())
        summary["servers"][srv] = {
            "total_calls": total,
            "by_project": dict(sorted(projs.items(), key=lambda x: -x[1])),
        }

    out = Path("/tmp/harness-audit-cross-usage.json")
    out.write_text(json.dumps(summary, indent=2))

    print(f"=== MCP usage across all projects ({total_sessions} sessions) ===\n")
    srv_sorted = sorted(summary["servers"].items(), key=lambda x: -x[1]["total_calls"])
    for srv, data in srv_sorted:
        print(f"{srv} — {data['total_calls']} total calls")
        for proj, n in list(data["by_project"].items())[:5]:
            print(f"  {n:>6} | {proj[:60]}")
        print()
    if not summary["servers"]:
        print("(no MCP tool calls found in any project)")
    print(f"JSON summary: {out}")


if __name__ == "__main__":
    main()
