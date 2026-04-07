# Harness Init

Analyzes a project and sets up complete Claude Code harness configuration based on actual code analysis.

## Installation

Add to your Claude Code plugins via the gs-plugins marketplace:
```bash
claude plugins add gs-plugins/harness-init
```

## Usage

**Trigger:** Use when setting up Claude Code for a new project, onboarding to a repository that lacks CLAUDE.md, or when a project needs complete harness configuration.

### What it does

1. Scans the project (structure, tech stack, architecture, conventions, testing, build)
2. Audits existing harness configuration (CLAUDE.md, .claude/, settings.json)
3. Generates project-specific CLAUDE.md (with user approval)
4. Configures settings/hooks based on detected tooling (with user approval)
5. Recommends relevant skills based on project characteristics
6. Verifies all artifacts are valid and non-conflicting

### Key Principles

- **Evidence-based:** Every CLAUDE.md instruction traces back to an observed project characteristic
- **Non-destructive:** Existing user configuration is preserved and merged, never overwritten
- **Approval gates:** CLAUDE.md and hooks are presented for user approval before writing
- **No templates:** Produces project-specific configuration, not generic templates

## Cross-Platform Support

| Platform | Status |
|----------|--------|
| Claude Code | Full support (CLAUDE.md + hooks + skills) |
| Codex CLI | Degraded (enhanced CLAUDE.md with embedded rules) |
| Gemini CLI | Degraded (enhanced CLAUDE.md with embedded rules) |
