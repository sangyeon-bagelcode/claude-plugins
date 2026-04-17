# harness-cost-audit

Data-driven audit to reduce Claude Code session cost. Counterpart to `harness-init` — while `harness-init` sets up new projects, this plugin audits existing ones to cut token/API cost based on real usage data.

## What it does

Runs a 13-step workflow that **measures before proposing** any change:

1. **Measurement phase (gated)** — parses `~/.claude/projects/*/*.jsonl` to compute per-turn cost, baseline context size, CLAUDE.md/memory/rules token breakdown, and cross-project MCP usage.
2. **Proposal phase (per-change approval)** — presents each reduction candidate individually with measured token savings and file paths. Never batches destructive actions.
3. **Execution phase** — timestamped backup → apply → re-measure → loop.
4. **Final report** — cumulative savings, rollback commands.

## Trigger

Claude activates the skill on:

- "비용 줄여줘" / "cost 감사" / "토큰 아껴줘"
- "harness 정리" / "메모리 압축" / "스킬 비용"
- "Agent 최적화" / "세션이 비싸"
- Cost-anomaly observations in recent sessions

## HARD-GATEs

1. **Measurement before proposal** — no recommendation without transcript/baseline/file/usage artifacts.
2. **Destructive-action** — no change without timestamped backup + user-approved-this-specific-change + single-at-a-time execution.

## Requirements

- `python3` on PATH (stdlib only, no dependencies)
- `~/.claude/projects/` transcripts (Claude Code). Falls back to file-only analysis on Codex / Gemini / API-only platforms.

## Pricing table version

`2026-04-17` — Opus / Sonnet / Haiku 4.x rates embedded in `scripts/analyze_transcripts.py` with update procedure documented in `reference/pricing.md`.

## Test report

Composite 87.7 (PASS). Axes: compliance 91, outcome 88, portability 82. Four pressure scenarios all defended.
