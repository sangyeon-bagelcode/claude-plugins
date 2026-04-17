# Platform-specific paths

Reference for where to look on each platform. Used during step 0 (schema /
path detection) to decide whether the skill can run in full mode.

## Claude Code (primary target)

| What | Path |
|------|------|
| Session transcripts | `~/.claude/projects/<encoded_cwd>/*.jsonl` |
| Project memory | `~/.claude/projects/<encoded_cwd>/memory/` |
| User settings | `~/.claude/settings.json` |
| User local settings | `~/.claude/settings.local.json` |
| Global config (MCP, projects, plugins) | `~/.claude.json` |
| Statusline | `~/.claude/statusline.sh` |
| Project `.mcp.json` | `<cwd>/.mcp.json` |
| Project rules | `<cwd>/.claude/rules/*.md` |
| Project CLAUDE.md cascade | `<cwd>/CLAUDE.md`, `<cwd>/**/CLAUDE.md`, + `@imports` |

**encoded_cwd rule**: `"-" + cwd.lstrip("/").replace("/", "-")`
Example: `/Users/wonsang-yeon/total-workspace` → `-Users-wonsang-yeon-total-workspace`

## Codex CLI

- No session-level transcript log in the same JSONL shape.
- MCP config lives in Codex's own config (path varies by version).
- Fallback: skip transcript-dependent steps; run only file-breakdown
  (plus user-reported tool usage, if the user provides it).

## Gemini CLI

- Same limitation as Codex — no cache-tier token accounting exposed.
- Fallback: file-breakdown mode only.

## Fallback mode triggers

Enter fallback mode (static-only analysis, no usage data) when ANY of:

- `~/.claude/projects/` does not exist
- The target project's transcript dir contains 0 `.jsonl` files
- The required fields are missing from the first parsed assistant turn:
  - `message.usage.cache_creation_input_tokens`
  - `message.usage.cache_read_input_tokens`

Fallback mode limitations (MUST be stated in the report):

- Cannot measure per-turn cost distribution.
- Cannot judge MCP/tool usage — all MCP removal recommendations omitted.
- Baseline measurement becomes a *file-size-based estimate* only
  (conservative, typically 20–40% below true baseline because system
  prompt and tool schemas are invisible from outside).
