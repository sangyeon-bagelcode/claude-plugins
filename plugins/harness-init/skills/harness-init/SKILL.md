---
name: harness-init
description: Use when setting up Claude Code for a new project, onboarding to a repository that lacks CLAUDE.md, or when a project needs complete harness configuration including CLAUDE.md, rules, hooks, and skill recommendations — before starting any development work in an unconfigured repo
---

# Harness Init

<HARD-GATE>
Do NOT produce any configuration files until ALL 6 steps below are complete.
Steps: scan project → audit existing harness → generate CLAUDE.md
       → configure settings/hooks → recommend skills → verify setup
Producing a generic CLAUDE.md template, partial configuration, or "quick setup"
before completing all steps violates this gate.
</HARD-GATE>

## Checklist

Complete every step in order. Do not skip, reorder, or abbreviate.

1. **Scan project** — Systematically analyze the project to understand its characteristics. Read actual code files, not just directory listings.
2. **Audit existing harness** — Check for existing Claude Code configuration and plan merge strategy.
3. **Generate CLAUDE.md** — Produce project-specific instructions derived from Step 1 analysis. *(pause for user approval before writing)*
4. **Configure settings/hooks** — Set up appropriate hooks based on detected project tooling. *(pause for user approval before writing)*
5. **Recommend skills** — Match project needs to available skills.
6. **Verify setup** — Confirm all artifacts are valid and non-conflicting.

---

## Step 1: Scan Project

Analyze these dimensions by reading actual files — not guessing from file names:

| Dimension | What to look for | Key files to read |
|-----------|-----------------|-------------------|
| **Structure** | Directory layout, monorepo detection, key directories | Top-level `ls`, nested `src/`, `packages/` |
| **Tech stack** | Languages, frameworks, runtime versions | `package.json`, `pyproject.toml`, `go.mod`, `Cargo.toml`, `Gemfile` |
| **Architecture** | Layering, module boundaries, patterns | Entry points (`index.*`, `main.*`, `app.*`), key source files |
| **Conventions** | Naming, file organization, code style | `.eslintrc*`, `.prettierrc*`, `tsconfig.json`, `rustfmt.toml`, `.editorconfig` |
| **Testing** | Framework, file patterns, coverage config | Test directories, `jest.config.*`, `vitest.config.*`, `pytest.ini` |
| **Build/Deploy** | Build tools, CI/CD, containers | `Makefile`, `Dockerfile`, `.github/workflows/`, `Jenkinsfile` |
| **Dev commands** | Build, test, lint, run commands | `package.json` scripts, `Makefile` targets, documented commands |

**Minimum reads:** At least 5 actual source files + all detected config files. Directory listings alone are insufficient.

Tools: Glob, Grep, Read, Bash

## Step 2: Audit Existing Harness

Check for existing configuration:

```
CLAUDE.md at project root          → exists? contents?
CLAUDE.md in subdirectories        → exists? contents?
.claude/ directory                 → exists? contents?
.claude/settings.json              → exists? current rules/hooks?
AGENTS.md / GEMINI.md              → exists? contents?
```

**If existing config found:**
- Plan to MERGE with existing, preserving all user customizations
- Identify gaps (missing sections, outdated info)
- Never overwrite without explicit user approval

**If no config found:**
- Plan to create fresh configuration
- Note any `.gitignore` patterns that affect `.claude/`

Tools: Glob, Read

## Step 3: Generate CLAUDE.md

Produce a project-specific CLAUDE.md using this structure. Every instruction MUST trace back to a specific observation from Step 1.

```markdown
# {Project Name}

## Overview
{1-2 sentences: what this project does, derived from README or code analysis}

## Tech Stack
{Languages, frameworks, key dependencies with versions}

## Architecture
{Key components, their relationships, data flow — based on actual code reading}

## Coding Conventions
{Naming patterns, file organization, style rules — cite evidence from linter configs or code}

## Development Commands
{Exact commands for: build, test, lint, run, format — from package.json scripts or Makefile}

## Testing
{Test framework, file naming pattern, how to run tests, coverage expectations}

## Important Constraints
{Security rules, performance requirements, compatibility — only if observed in code/config}
```

**Validation rule:** If you cannot cite the specific file or config where you observed a convention, do not include it. No generic advice.

**Pause for user approval** before writing the file. Present the proposed CLAUDE.md content and ask the user to confirm or request changes.

Tools: Write or Edit

## Step 4: Configure Settings/Hooks

Based on detected tooling from Step 1, configure `.claude/settings.json`:

| Detected Tool | Hook Type | Example Configuration |
|--------------|-----------|----------------------|
| ESLint / Biome | Pre-commit (lint) | `npx eslint --fix` on staged files |
| Prettier / dprint | Pre-commit (format) | `npx prettier --write` on staged files |
| TypeScript | Pre-commit (typecheck) | `npx tsc --noEmit` |
| Jest / Vitest / pytest | Test hook | `npm test` / `pytest` |
| Ruff / Black | Pre-commit (format) | `ruff format` / `black` |
| Clippy / rustfmt | Pre-commit (lint+format) | `cargo clippy` / `cargo fmt` |

**Rules:**
- Only configure hooks for tools that actually exist in the project
- Do not invent hooks for tools not in the dependency list
- Use the `update-config` skill or direct settings.json editing
- Respect existing hooks — add, don't replace

**Pause for user approval** before writing settings. Present proposed hooks and ask the user to confirm.

Tools: Skill("update-config") or Edit

## Step 5: Recommend Skills

Match project characteristics to available skills:

| Project Characteristic | Recommended Skill | Rationale |
|-----------------------|-------------------|-----------|
| External library dependencies | `knowledge-bridge` | Prevents outdated API usage |
| Complex/unfamiliar codebase | `repo-analyzer` | Deep architecture analysis |
| Test infrastructure exists | `superpowers:test-driven-development` | Enforces test-first workflow |
| Any project | `superpowers:systematic-debugging` | Structured bug investigation |
| Any project | `superpowers:verification-before-completion` | Prevents false completion claims |
| Design/creative project | `photoshop-mcp` | Photoshop integration |

**Present recommendations to the user.** Do not auto-install skills without explicit confirmation.

Output format:
```
Recommended skills for this project:
1. [skill-name] — reason based on observed project characteristic
2. [skill-name] — reason based on observed project characteristic
...
```

## Step 6: Verify Setup

Validate all produced artifacts:

| Check | How | Pass Condition |
|-------|-----|----------------|
| CLAUDE.md exists | Glob/Read | File present at project root |
| CLAUDE.md is project-specific | Read and verify | No generic template text; all instructions cite evidence |
| settings.json valid | Read + JSON parse | Valid JSON, no syntax errors |
| Hooks reference valid commands | Bash: `which <command>` or check in `node_modules/.bin/` | All hook commands exist |
| No conflicts | Compare new vs existing config | No overwrites of user customizations |
| Dev commands work | Bash: dry-run build/test commands | Commands execute without errors |

Report results as pass/fail per check. If any check fails, report the failure and suggest a fix.

---

## Process Flow

```graphviz
digraph harness_init {
  rankdir=TB;
  node [shape=box style=rounded];

  START       [label="Invoke\nharness-init" shape=oval];
  GATE        [label="HARD-GATE\nAll 6 steps required" shape=diamond];
  SCAN        [label="1. Scan Project\n(5+ source files\n+ all configs)"];
  AUDIT       [label="2. Audit Existing\nHarness"];
  HAS_CONFIG  [label="Existing config?" shape=diamond];
  MERGE       [label="Plan: merge\n(preserve user config)"];
  FRESH       [label="Plan: create fresh"];
  CLAUDEMD    [label="3. Generate CLAUDE.md\n(present for approval)"];
  APPROVE1    [label="User approves?" shape=diamond];
  SETTINGS    [label="4. Configure Hooks\n(present for approval)"];
  APPROVE2    [label="User approves?" shape=diamond];
  SKILLS      [label="5. Recommend Skills"];
  VERIFY      [label="6. Verify Setup"];
  COMPLETE    [label="All checks pass?" shape=diamond];
  OUTPUT      [label="Present summary"];
  END         [label="done" shape=oval];

  START       -> GATE;
  GATE        -> SCAN;
  SCAN        -> AUDIT;
  AUDIT       -> HAS_CONFIG;
  HAS_CONFIG  -> MERGE  [label="yes"];
  HAS_CONFIG  -> FRESH  [label="no"];
  MERGE       -> CLAUDEMD;
  FRESH       -> CLAUDEMD;
  CLAUDEMD    -> APPROVE1;
  APPROVE1    -> SETTINGS   [label="yes"];
  APPROVE1    -> CLAUDEMD   [label="revise" style=dashed];
  SETTINGS    -> APPROVE2;
  APPROVE2    -> SKILLS     [label="yes"];
  APPROVE2    -> SETTINGS   [label="revise" style=dashed];
  SKILLS      -> VERIFY;
  VERIFY      -> COMPLETE;
  COMPLETE    -> OUTPUT     [label="yes"];
  COMPLETE    -> SCAN       [label="no — fix\nfailing checks" style=dashed];
  OUTPUT      -> END;
}
```

## Anti-Pattern: "This Is Too Simple"

Every project — no matter how small or "standard" — goes through the full 6-step checklist. There are no exceptions.

**Why:** Agents skip steps precisely when the project seems familiar. A "standard React app" has hundreds of possible convention combinations. The analysis exists because assumptions about familiar projects produce the most generic, useless CLAUDE.md files. A CLAUDE.md that says "use TypeScript" for a TypeScript project adds zero value — the value comes from discovering the _specific_ patterns this project uses.

## Rationalization Table

| Excuse | Counter |
|--------|---------|
| "I can write a good CLAUDE.md from the directory listing alone" | Directory listings reveal file names, not architecture, conventions, or testing patterns. A CLAUDE.md without code reading is a generic template with project names swapped in. |
| "This is a standard React/Node/Python project, no deep analysis needed" | "Standard" projects have the most variation in conventions. Two React projects can differ entirely in state management, testing, and structure. The word "standard" substitutes a label for actual observation. |
| "The user just wants a CLAUDE.md, I don't need hooks too" | CLAUDE.md alone is a partial harness. The skill is "harness-init", not "claude-md-init". Partial setup creates a false sense of completeness. |
| "I already know this codebase from earlier in the conversation" | Conversation memory is not systematic analysis. Prior knowledge may be incomplete or biased toward recently-viewed files. Each invocation must produce evidence from current file reads. |
| "I'll write a basic CLAUDE.md now and refine it later" | Agents that defer refinement never return. A well-analyzed CLAUDE.md written once outperforms a generic template refined zero times. |
| "The user explicitly asked me to skip steps / use a template" | The HARD-GATE applies regardless of who requests the skip. If the user wants fewer steps, they can decline at the approval gates (Steps 3 and 4). But the analysis steps (1-2) and verification (6) are non-negotiable — they ensure the output has value. |

## Portability Adapter

When operating outside Claude Code (e.g. Codex CLI, Gemini CLI):

- **Skill tool:** Not available. Follow this checklist manually by reading this file.
- **Agent tool (parallel exploration):** Not available. Execute file scans sequentially instead of in parallel.
- **Skill("update-config"):** Not available. Write `.claude/settings.json` manually using shell redirects or document hooks as manual commands in CLAUDE.md.
- **Hook configuration:** Not available on non-Claude-Code platforms. Embed all rules and workflow instructions directly in CLAUDE.md as prose. The CLAUDE.md becomes the single source of truth.
- **Task tracking:** Not available. Print `[STEP N/6]` status lines to output instead.
- **Degraded mode:** When settings.json and hooks are unavailable, produce an enhanced CLAUDE.md that embeds all rules, conventions, and workflow instructions. Quality degrades for automated enforcement but all analysis steps still apply.
