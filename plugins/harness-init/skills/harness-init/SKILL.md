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

### The Code-Readable Filter (mandatory, mechanical)

After drafting CLAUDE.md, apply this filter to EVERY line. This is not optional.

**For each line, run this test:**
```
1. Does this information exist in ANY file in the repo?
   (source code, config, schema, package.json, tsconfig, README...)
   → YES → DELETE the line. No exceptions.
   → NO → go to step 2.

2. Could Claude discover this by reading the relevant file?
   (function signatures, import patterns, DB schema, config values...)
   → YES → DELETE the line.
   → NO → KEEP. This is genuine operational knowledge.
```

**After filtering, count surviving lines per section:**
- **How It Works:** MAX 2 lines. If more → you included code-readable content.
- **Things That Will Bite You:** Each item must fail BOTH tests above. If Claude could find it by reading one file, it's not a gotcha.
- **Code Conventions:** If a linter, tsconfig, or .editorconfig enforces it → DELETE.

**Common traps — these ALWAYS fail the filter:**
- Model names (Haiku, Opus) → config/code says this
- Database choices (SQLite, Neo4j) → package.json/imports say this
- Code patterns (factory, DI) → code shows this
- Framework features (WAL mode, graceful fallback) → code implements this
- Listen addresses (127.0.0.1) → code says this
- Type file locations (shared/types.ts) → code navigation finds this

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
{ONLY rules that differ from defaults. Imperative tone.}

## Workflow
{Branch naming, PR conventions, CI checks — if applicable}
```

Use `@path/to/file` imports to reference detailed docs without bloating CLAUDE.md:
```markdown
See @docs/api-guide.md for API conventions.
```

For monorepos, use subdirectory CLAUDE.md files — they load on demand when Claude works in that directory.

### Validation

Go line by line and apply the Code-Readable Filter. If ANY check fails, revise before presenting.

- **"How It Works"**: Is it ONE data flow sentence? Does it name specific files, models, or databases? If yes → rewrite.
- **"Things That Will Bite You"**: For each item, can Claude find this by reading ONE file? If yes → CUT or rewrite as workflow gotcha.
- **"Code Conventions"**: Does a linter/config already enforce this? If yes → CUT.
- **Total under 100 lines** (150 max for complex projects)
- Commands section is first
- Imperative factual tone

**Pause for user approval.**

## Step 4: Generate .claude/rules/

### Layer 1: Agent Behavioral Guide → lives in CLAUDE.md

The "## Guidelines" section in the CLAUDE.md template above IS Layer 1. It goes directly in CLAUDE.md, not in a separate rules file.

Based on Karpathy's validated guidelines (think before coding, simplicity first, surgical changes, goal-driven execution) + community-validated root cause debugging. Customize wording to match the project's tone, but keep all 6 bullet points.

### Layer 2: Tech Stack + Testing (match detected stack)

Generate per-stack rules. Include testing rules specific to that stack's test framework.

| Stack | Key rules | File |
|-------|----------|------|
| TypeScript | No `any` (use `unknown`), check tsconfig strictness | `.claude/rules/typescript.md` |
| React | Test user behavior not internals, getByRole > getByTestId | `.claude/rules/frontend.md` |
| Python | Type hints on public functions, pytest fixtures, no bare `except:` | `.claude/rules/python.md` |
| Rust | `cargo clippy` first, no `unwrap()` in production | `.claude/rules/rust.md` |
| Go | Handle every error, table-driven tests | `.claude/rules/go.md` |

Add testing rules based on what Step 1 found:

```markdown
# .claude/rules/testing.md (example — customize per project)
---
paths: ["**/*.test.*", "**/*.spec.*"]
---
- Write a failing test BEFORE fixing any bug. (motion pattern)
- Never modify existing tests to make them pass — fix the implementation. (Cribo)
- Test behavior, not implementation. Mock only at boundaries. (community consensus)
- Never use weak assertions (toBeDefined, toBeTruthy). Assert specific values.
```

Customize based on what the project's config and test patterns actually show.

### Layer 3: Project-specific (from Step 1)

Derived from the scan. Use `paths:` frontmatter to scope rules:

```markdown
# .claude/rules/api.md
---
paths: ["src/api/**", "src/routes/**"]
---
- Validate all inputs with zod before processing
- Use kebab-case for URL paths
```

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

### Auto-detect from tooling

| Detected | Hook | Command |
|----------|------|---------|
| ESLint/Biome | PostToolUse (Edit) | `npx eslint --fix {file}` |
| Prettier | PostToolUse (Edit) | `npx prettier --write {file}` |
| TypeScript | PostToolUse (Edit) | `npx tsc --noEmit` |

Only configure hooks for tools that actually exist in the project.

**Pause for user approval.**

## Step 6: Scaffold Skills & Subagents

### Project-specific skills (.claude/skills/)

If the project has repeatable workflows, create skills for them:

```markdown
# .claude/skills/fix-issue/SKILL.md
---
name: fix-issue
description: Fix a GitHub issue with test-first approach
---
1. `gh issue view $ARGUMENTS` — read the actual issue
2. Write a failing test that reproduces it
3. Fix the implementation
4. Run full test suite
5. Commit and create PR
```

### Subagents (.claude/agents/)

For projects with distinct domains, scaffold specialized subagents:

```markdown
# .claude/agents/security-reviewer.md
---
name: security-reviewer
tools: Read, Grep, Glob
---
Review code for injection vulnerabilities, auth flaws, and secrets in code.
Provide specific line references and fixes.
```

### Plugin recommendations

Match project needs to available plugins. Present to user, don't auto-install.

## Step 7: Verify Setup

| Check | Pass Condition |
|-------|----------------|
| CLAUDE.md exists, Commands first | Commands is the first content section |
| CLAUDE.md ~100 lines | Under 150 lines |
| Gotchas section has 2+ items | Non-obvious behaviors documented |
| .claude/rules/ has path-scoped files | At least 1 rule file per distinct domain |
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

## Anti-Pattern: "Blindly Copy Generic Rules"

The 3-layer system provides a menu, not a checklist. An agent that dumps all 20 generic rules into every project is defeating the purpose. Pick 5-6 that address real risks for THIS project. If the project has no tests, testing integrity rules are higher priority than code style rules.
