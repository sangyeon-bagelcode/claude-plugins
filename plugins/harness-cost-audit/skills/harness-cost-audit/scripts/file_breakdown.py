#!/usr/bin/env python3
"""
Break down the user-editable portion of baseline context by file.

Counts characters in:
  - The project's CLAUDE.md cascade (recursively follows @path imports)
  - The project's .claude/rules/*.md files
  - The user's memory directory for this project (MEMORY.md + linked files)

Character count / 4 ≈ token count (standard estimate for English/Korean mix).

Usage:
  python3 file_breakdown.py <project_cwd> [memory_dir]

memory_dir defaults to:
  ~/.claude/projects/<encoded_cwd>/memory/

Output:
  - stdout: per-file table + totals
  - JSON: /tmp/harness-audit-files.json
"""
import json
import re
import sys
from pathlib import Path


def encode_cwd(cwd: Path) -> str:
    # Claude Code encodes cwd as -Users-wonsang-yeon-total-workspace style
    return "-" + str(cwd.resolve()).lstrip("/").replace("/", "-")


def resolve_at_import(base: Path, ref: str) -> Path | None:
    ref = ref.strip()
    if not ref:
        return None
    # @path may be absolute or relative
    p = Path(ref)
    if not p.is_absolute():
        p = (base / p).resolve()
    return p if p.is_file() else None


def scan_claude_md_cascade(root: Path) -> list[Path]:
    """Follow CLAUDE.md starting at root, recursively expand @-imports."""
    seen = set()
    queue = []
    for name in ("CLAUDE.md",):
        start = root / name
        if start.is_file():
            queue.append(start)
    # also gather nested CLAUDE.md files one level deep (common pattern)
    for sub in root.iterdir() if root.is_dir() else []:
        if sub.is_dir():
            nested = sub / "CLAUDE.md"
            if nested.is_file():
                queue.append(nested)
    collected = []
    while queue:
        p = queue.pop(0)
        if p in seen or not p.is_file():
            continue
        seen.add(p)
        collected.append(p)
        # parse @-imports
        try:
            text = p.read_text(errors="ignore")
        except Exception:
            continue
        for m in re.finditer(r"@([./\w\-]+\.md)\b", text):
            target = resolve_at_import(p.parent, m.group(1))
            if target and target not in seen:
                queue.append(target)
    return collected


def scan_rules(root: Path) -> list[Path]:
    rules_dir = root / ".claude" / "rules"
    return sorted(rules_dir.glob("*.md")) if rules_dir.is_dir() else []


def scan_memory(mem_dir: Path) -> list[Path]:
    if not mem_dir.is_dir():
        return []
    return sorted(mem_dir.glob("*.md"))


def measure(files: list[Path]) -> list[dict]:
    rows = []
    for f in files:
        try:
            sz = f.stat().st_size
            rows.append({
                "path": str(f),
                "chars": sz,
                "tokens_est": sz // 4,
            })
        except Exception:
            pass
    return rows


def main():
    if len(sys.argv) < 2:
        print("Usage: file_breakdown.py <project_cwd> [memory_dir]", file=sys.stderr)
        sys.exit(1)
    cwd = Path(sys.argv[1]).expanduser().resolve()
    if not cwd.is_dir():
        print(f"[ERROR] Not a directory: {cwd}", file=sys.stderr)
        sys.exit(2)

    if len(sys.argv) > 2:
        mem_dir = Path(sys.argv[2]).expanduser()
    else:
        mem_dir = Path.home() / ".claude" / "projects" / encode_cwd(cwd) / "memory"

    claude_md = scan_claude_md_cascade(cwd)
    rules = scan_rules(cwd)
    memory = scan_memory(mem_dir)

    sections = {
        "claude_md_cascade": measure(claude_md),
        "project_rules": measure(rules),
        "user_memory": measure(memory),
    }
    totals = {k: sum(r["tokens_est"] for r in v) for k, v in sections.items()}
    grand_total = sum(totals.values())

    summary = {
        "project_cwd": str(cwd),
        "memory_dir": str(mem_dir),
        "sections": sections,
        "section_totals_tokens": totals,
        "grand_total_tokens_est": grand_total,
    }
    out = Path("/tmp/harness-audit-files.json")
    out.write_text(json.dumps(summary, indent=2))

    print(f"=== File breakdown for {cwd.name} ===")
    for section, rows in sections.items():
        if not rows:
            continue
        print(f"\n[{section}] — {totals[section]:,} tokens")
        for r in sorted(rows, key=lambda x: -x["tokens_est"])[:15]:
            rel = Path(r["path"]).name
            print(f"  {rel:<50s} ~{r['tokens_est']:>5,} tokens  ({r['chars']:,} chars)")
    print(f"\nGrand total (user-editable): ~{grand_total:,} tokens")
    print(f"JSON summary: {out}")


if __name__ == "__main__":
    main()
