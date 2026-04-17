# Changelog

## 1.0.0 — 2026-04-17

Initial release.

### Added
- `harness-cost-audit` skill — 13-step data-driven workflow to audit Claude Code harness configuration (CLAUDE.md cascade, memory files, MCP servers, permissions) and reduce per-session token cost.
- Two HARD-GATEs:
  - Measurement before proposal — 5 measurement scripts must run before any recommendation.
  - Destructive-action gate — timestamped backup + per-change approval + no batching.
- Four measurement scripts (Python 3, stdlib only):
  - `analyze_transcripts.py` — per-turn cost & tool distribution with model-family pricing
  - `measure_baseline.py` — first-turn `cache_creation_input_tokens` averaged across sessions
  - `file_breakdown.py` — CLAUDE.md cascade with `@-import` recursion, memory, and rules token estimation
  - `cross_project_usage.py` — MCP tool-call aggregation across all `~/.claude/projects/`
- Pricing table versioned at 2026-04-17 (Opus / Sonnet / Haiku 4.x), with update procedure documented in `reference/pricing.md`.
- Fallback mode for Codex / Gemini / API-only platforms where transcripts are unavailable.

### Test report
- Composite score 87.7 (PASS); axes: compliance 91, outcome 88, portability 82.
- Four pressure scenarios (simplicity+authority, sunk-cost+batch-approval, small-change-skip-backup, tool-gap+foreign-platform) all defended.
