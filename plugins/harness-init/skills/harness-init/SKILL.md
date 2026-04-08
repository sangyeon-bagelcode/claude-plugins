---
name: harness-init
description: Use when setting up Claude Code for a new project, onboarding to a repository that lacks CLAUDE.md, or when a project needs complete harness configuration including CLAUDE.md, rules, hooks, and skill recommendations — before starting any development work in an unconfigured repo
---

# Harness Init

<HARD-GATE>
Do NOT produce any configuration files until ALL 7 steps below are complete.
Steps: scan → audit → CLAUDE.md → rules → hooks → skills/subagents → verify
</HARD-GATE>

## Checklist

1. **Scan project** — Read actual code files (min 5 source + all configs). Not just directory listings.
2. **Audit existing harness** — Check CLAUDE.md, .claude/, AGENTS.md. Plan merge or fresh creation.
3. **Generate CLAUDE.md** — Behavioral guide, not README. *(pause for user approval)*
4. **Generate .claude/rules/** — 3-layer behavioral rules. *(pause for user approval)*
5. **Configure hooks** — Claude Code lifecycle hooks + settings.json. *(pause for user approval)*
6. **Scaffold skills & subagents** — Create project-specific skills, recommend plugins, scaffold subagents if needed.
7. **Verify setup** — Validate all artifacts.

---

## Step 1: Scan Project

| What to look for | Key files | Feeds into |
|-----------------|-----------|------------|
| Build/test/lint/run commands | `package.json` scripts, `Makefile` | CLAUDE.md Commands |
| Config non-defaults | `.eslintrc*`, `tsconfig.json`, `.prettierrc*` | rules/ Code Style |
| Repeated code patterns (5+ files) | Source files across modules | rules/ Code Style |
| Module boundaries & import graph | `src/` subdirectories, entry points | CLAUDE.md Architecture |
| Test conventions (mock vs fixture) | Test files, test config, fixtures dir | rules/testing.md |
| CI pipeline & branch rules | `.github/workflows/`, `.gitlab-ci.yml` | CLAUDE.md Workflow |
| Environment dependencies | `.env.example`, `docker-compose.yml` | CLAUDE.md Gotchas |
| Non-obvious behaviors | NOTE/HACK/WORKAROUND comments, git reverts | CLAUDE.md Gotchas |

Every observation must feed into a concrete rule. If it doesn't produce a rule, it was unnecessary.

## Step 2: Audit Existing Harness

Check: `CLAUDE.md`, `.claude/`, `.claude/settings.json`, `.claude/rules/`, `.claude/skills/`, `.claude/agents/`, `AGENTS.md`

- Existing config → merge, preserve user customizations
- No config → create fresh

## Step 3: Generate CLAUDE.md

**~100 lines ideal** (Boris Cherny, Claude Code creator). Max 150 for complex projects.

Golden Rule: **"Would removing this line cause Claude to make mistakes?"** NO → cut it.

### The Code-Readable Filter (BEFORE writing, not after)

Do NOT write a draft first and filter later. You will fail to delete your own writing.

**Process: filter first, then write.**

Step 1: From the scan results, build a candidate list of facts you might include.
Step 2: For each candidate, run this test:
```
Q: Does this information exist in ANY file in the repo?
   → YES → DISCARD. Do not write it.
Q: Could Claude discover this by reading that file?
   → YES → DISCARD. Do not write it.
Only candidates that fail BOTH questions enter the draft.
```
Step 3: Write CLAUDE.md using ONLY the surviving candidates. The draft must contain NO information that is not in the KEEP column of your filter table. If a fact is not in the table, it does not go in the draft.

**The test is binary. "Useful" is not a factor.** If the info exists in the repo, Claude will find it when it needs it. CLAUDE.md is not a summary of the codebase — it is a list of things that cannot be found by reading code.

**Do not add sections beyond the template.** If the template has no "External Dependencies" section, do not create one. Stick to: Commands, What This Is, How It Works, Things That Will Bite You, Guidelines, Code Conventions, Workflow.

### Template (Anthropic pattern)

```markdown
# {Project Name}

## Commands
{Exact build/test/lint/run commands — ONLY those Claude can't guess}

## What This Is
{1-3 sentences. Brief, factual.}

## How It Works
{3-5 lines MAX. One-sentence data flow + key entrypoint. That's it.}
{NO directory trees. NO file-by-file descriptions. Claude reads files itself.}
{BAD: listing src/agents/session.ts — "SDK query() 호출, 보안 훅, 프롬프트 빌드"}
{GOOD: "Slack event → Gateway(normalize) → Orchestrator(route) → Agent → SDK query()"}

## Things That Will Bite You
{Non-obvious behaviors, tribal knowledge. HIGHEST VALUE section.}

## Guidelines
- State assumptions explicitly. If uncertain, ask — don't guess silently.
- Minimum code that solves the problem. No speculative features or abstractions.
- Every changed line must trace to the request. Don't "improve" unrelated code.
- Transform tasks into verifiable goals: "Fix bug" → "Write reproducing test, then fix."
- Find root cause before patching. STOP and reason when anything fails.
- Run tests after every change. Do not claim done without output.

## Code Conventions
{ONLY if KEEP items exist for this section. If all conventions are enforced by
tooling or visible in code, OMIT THIS SECTION ENTIRELY. An empty section is
worse than no section — it invites filler.}

## Workflow
{ONLY if KEEP items exist. Omit if no non-obvious workflow rules.}
```

Use `@path/to/file` imports to reference detailed docs without bloating CLAUDE.md:
```markdown
See @docs/api-guide.md for API conventions.
```

For monorepos, use subdirectory CLAUDE.md files — they load on demand when Claude works in that directory.

### Validation

Before presenting the draft, verify:

1. **Show your filter work.** List each candidate fact and whether it passed or failed the 2-question test. Present this table to the user alongside the draft so they can verify.
2. **How It Works:** Max 1-2 sentences. Data flow only.
3. **Things That Will Bite You:** Each item survived the filter — cannot be found by reading any single file.
4. **Code Conventions / Workflow:** Only KEEP items. If no KEEP items exist for a section, omit the section entirely.
5. Total under 100 lines.
6. Commands section is first.

**Pause for user approval.**

## Step 4: Generate .claude/rules/

### Layer 1: Agent Behavioral Guide → lives in CLAUDE.md

The "## Guidelines" section in the CLAUDE.md template above IS Layer 1. It goes directly in CLAUDE.md, not in a separate rules file.

Based on Karpathy's validated guidelines (think before coding, simplicity first, surgical changes, goal-driven execution) + community-validated root cause debugging. Customize wording to match the project's tone, but keep all 6 bullet points.

### Layer 2 & 3: Rules files

**The same Code-Readable Filter applies to rules.** Rules are behavioral guidance, not code documentation. If a linter, compiler, or the code itself already enforces/shows something, it does not go in a rule.

Generate `testing.md` for every project that has tests — these are behavioral rules, not code descriptions:

```markdown
# .claude/rules/testing.md
---
paths: ["**/*.test.*", "**/*.spec.*"]
---
- Write a failing test BEFORE fixing any bug.
- Never modify existing tests to make them pass — fix the implementation.
- Test behavior, not implementation. Mock only at boundaries.
- Never use weak assertions (toBeDefined, toBeTruthy). Assert specific values.
```

For other rules files, only create them if KEEP items survive the filter. Apply `paths:` frontmatter to scope them. Do NOT create rules files that merely describe existing code patterns — Claude reads code.

**Pause for user approval.**

## Step 5: Configure Hooks

Claude Code hooks are **deterministic** (100% enforcement) unlike CLAUDE.md (~70-80%). Use hooks for anything that MUST always happen.

### Hook Event Types

| Event | Use for | Example |
|-------|---------|---------|
| `PreToolUse` | Block dangerous actions before they happen | Block writes to `migrations/` without approval |
| `PostToolUse` | Validate after tool actions | Run linter after file edits |
| `Notification` | Alert on specific patterns | Warn when editing security-sensitive files |
| `Stop` | Enforce checks before session ends | Require test pass before claiming done |

Configure hooks based on what Step 1 found. Only for tools that exist in the project. Prioritize hooks that prevent real damage over convenience automation.

**Never hardcode absolute paths in hook commands.** Use relative paths, `$PWD`, or project-relative resolution. Hooks with absolute paths break when the project moves or another developer clones it.

**Pause for user approval.**

## Step 6: Scaffold Skills & Subagents

Only create skills/subagents if the project has clear, repeatable workflows that would benefit from them. Ask the user before scaffolding — don't assume.

**Skills (.claude/skills/):** For repeatable multi-step workflows the user invokes by name.
**Subagents (.claude/agents/):** For tasks that need isolated context with specific tool permissions.
**Plugin recommendations:** Match project needs to available plugins. Present to user, don't auto-install.

If the project is simple or early-stage, skip this step entirely. Not every project needs custom skills or subagents.

## Step 7: Verify Setup

| Check | Pass Condition |
|-------|----------------|
| CLAUDE.md exists, Commands first | Commands is the first content section |
| CLAUDE.md ~100 lines | Under 150 lines |
| Gotchas section has 2+ items | Non-obvious behaviors documented |
| .claude/rules/ if created | Every rule passed Code-Readable Filter |
| settings.json valid JSON | Parses without errors |
| Hook commands exist | `which <cmd>` or `node_modules/.bin/` check |
| No overwrites of user config | Existing customizations preserved |

---

## Anti-Pattern: "README-as-CLAUDE.md"

| Descriptive (cut it) | Actionable (keep it) |
|----------------------|---------------------|
| "This project uses TypeScript and React" | "Runtime is Bun, not Node. Use bun test, not jest." |
| "The API is in src/api/" | "API handlers must validate input with zod — never trust req.body" |
| "We use Jest for testing" | "Run `npm test -- --testPathPattern=<file>`, never the full suite" |
| Directory tree with file descriptions | One-sentence data flow: "Event → Gateway → Agent → SDK" |
| "`query()` is async iterator" (code-readable) | "`query()` SDK calls cost money — always mock in tests" (operational) |
| "ESM only, .js extensions required" (tsconfig says it) | ".env must be in agent-server/, not project root" (convention) |
| "factory pattern: createXxxAgent()" (code says it) | "build없이 프로덕션 배포하면 깨진다" (operational experience) |

**A good CLAUDE.md tells Claude HOW to behave, not WHAT the project is.**

## Anti-Pattern: "Fill Empty Sections"

If no KEEP items survive the filter for a section (Code Conventions, Workflow, rules files), leave it out. Do not fill sections with code-readable content to avoid empty space. Less is more.
