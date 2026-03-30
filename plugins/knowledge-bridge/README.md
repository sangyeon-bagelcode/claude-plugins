# Knowledge Bridge

Closes the knowledge gap between AI training data and current library/SDK documentation.

Inspired by [Google's Agent Skills research](https://developers.googleblog.com/closing-the-knowledge-gap-with-agent-skills/), which showed success rates jumping from 28% to 97% when agents fetch live documentation before coding.

## Installation

```bash
claude plugins add /path/to/knowledge-bridge
```

Or add to your Claude Code plugins directory:
```
~/.claude/plugins/knowledge-bridge/
```

## Usage

The skill triggers automatically when you write or modify code that uses external libraries, frameworks, SDKs, or APIs.

**Trigger:** Use when writing or modifying code that imports or depends on any external library, framework, SDK, or API.

### What it does

1. Detects external library usage in the code you're about to write
2. Fetches current documentation via Context7, WebFetch, WebSearch, or asks you
3. Verifies the docs cover the needed APIs
4. Writes code based on fetched docs (not training memory)
5. Cross-checks the output against fetched documentation

### Tool Fallback Chain

The skill uses the first available tool:
1. Context7 MCP (`resolve-library-id` + `query-docs`)
2. WebFetch (official docs URL)
3. WebSearch (search for docs)
4. Ask user (request docs URL)

## Cross-Platform Support

| Platform | Status |
|----------|--------|
| Claude Code | Full support |
| Codex CLI | Supported via `curl` fallback |
| Gemini CLI | Supported via `activate_skill` + Google Search |
