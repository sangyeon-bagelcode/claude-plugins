---
description: Request a new Photoshop MCP tool — interview, generate, test, merge, notify
argument-hint: <what you want to do in Photoshop, e.g. "Inner Glow 넣고 싶어">
---

# New Photoshop Tool

비개발자가 자연어로 요청하면 Photoshop MCP 도구를 자동 생성한다.

## The Iron Law

```
NO CODE WITHOUT A PASSING FEASIBILITY CHECK
NO MERGE WITHOUT ALL 4 GATES PASSING
NO GATE SKIP — EVER
```

Thinking "skip this gate just this once"? Stop. That's rationalization.

## Prerequisites

<HARD-GATE>
Before ANYTHING else, verify photoshop-mcp repo exists locally:

```bash
REPO=$(find ~ -maxdepth 3 -name "photoshop-mcp" -type d -exec test -f {}/mcp/server.py \; -print -quit 2>/dev/null)
echo "REPO=$REPO"
```

- Output shows a path → PASS. Set `$REPO` for all subsequent commands.
- Empty → FAIL. Clone first: `git clone https://github.com/bagelcode-gamestudio/photoshop-mcp.git`

Do NOT proceed until REPO path is confirmed.
</HARD-GATE>

---

## Phase 1: Interview

<HARD-GATE>
You MUST complete the interview before ANY code work. No exceptions.
</HARD-GATE>

**Rules — violating ANY of these is violating the spirit:**
- NEVER use technical terms (opacity, blend mode, descriptor, batchPlay)
- ALWAYS offer choices ("강하게 / 보통 / 은은하게")
- MAX 3 questions
- If user gave argument, start from there

### Step 1.1: Understand intent

Ask ONE question at a time:
1. "어떤 효과를 원하세요?" (if not provided as argument)
2. "어떤 느낌으로요? (강하게 / 보통 / 은은하게)"
3. "색상 선호가 있나요? (없으면 기본값으로 할게요)"

### Step 1.2: Confirm

> "이런 느낌으로 만들게요: [자연어 설명]. 괜찮으세요?"

User confirms → Phase 2. User declines → re-interview.

---

## Phase 2: Registry Check

<HARD-GATE>
You MUST check the registry BEFORE any feasibility work. Run the actual command.
</HARD-GATE>

```bash
cd $REPO && python3 -c "
import json
registry = json.load(open('registry/tools.json'))
for t in registry['tools']:
    if t['enabled']:
        print(f\"{t['name']}: {t['description']}\")
"
```

**Read the output. Then decide:**

| Result | Action |
|--------|--------|
| Exact match exists | "이미 있는 도구로 바로 할 수 있어요!" → 실행. 종료. |
| Similar tool exists | "비슷한 도구가 있는데, 써볼까요?" → user decides |
| No match | → Phase 3 |

---

## Phase 3: Feasibility Check

<HARD-GATE>
You MUST verify the Photoshop API supports this effect BEFORE writing any code.
Skipping this = shipping broken tools to non-developers.
</HARD-GATE>

### Step 3.1: API Research

Use context7 to search Photoshop batchPlay API documentation:
1. Search for the effect name (e.g., "innerGlow", "bevelEmboss")
2. Confirm the `_obj` descriptor name exists
3. Identify required parameters

### Step 3.2: Pattern Match

```bash
cd $REPO && grep -r "layerEffects" plugin/commands/ | head -20
```

Find existing layer style handlers. The new tool MUST follow the same batchPlay pattern.

### Step 3.3: Feasibility Verdict

| Verdict | Criteria | Action |
|---------|----------|--------|
| ✅ POSSIBLE | batchPlay descriptor found + existing pattern match | → Phase 4 |
| ⚠️ LIMITED | descriptor exists but missing some params | Explain limitation to user. User approves → Phase 4 |
| ❌ IMPOSSIBLE | no descriptor / UXP blocked / requires external dep | "이건 Photoshop API로 자동화할 수 없어요." 수동 방법 안내. 종료. |

**Evidence required:** You MUST show which descriptor you found and which existing handler you're basing the pattern on. "Should work" is NOT evidence.

---

## Phase 4: Code Generation

<HARD-GATE>
You MUST work on a feature branch. NEVER commit directly to main.
</HARD-GATE>

### Step 4.1: Create branch

```bash
cd $REPO && git checkout main && git pull origin main
TOOL_NAME="<snake_case_function_name>"
git checkout -b feat/add-$TOOL_NAME
```

### Step 4.2: Write Python test FIRST (TDD Red)

Create or append to `tests/unit/test_<category>.py`:

```python
def test_<tool_name>_creates_correct_command():
    # ... mock setup ...
    registered_tools["<tool_name>"](layer_id=1)
    mock_create.assert_called_once_with("<actionName>", {
        "layerId": 1,
        # ... expected defaults ...
    })
```

### Step 4.3: Verify test FAILS

```bash
cd $REPO && python -m pytest tests/unit/test_<category>.py -v -k "<tool_name>"
```

<HARD-GATE>
Test MUST fail. If it passes, you're testing existing behavior. Fix the test.
If test errors (not fails), fix the error first.
Do NOT proceed to Step 4.4 until you see a FAILURE (not error).
</HARD-GATE>

### Step 4.4: Write Python MCP function (TDD Green)

Add to `mcp/tools/<category>.py` inside `register_tools(mcp)`:

**Rules:**
- Function name: snake_case
- ALL parameters have defaults (except `layer_id`)
- Mutable defaults → `None` + guard: `if color is None: color = {...}`
- Docstring in English (Claude sees this)
- `createCommand` action name: camelCase, MUST match JS handler name exactly

### Step 4.5: Verify test PASSES

```bash
cd $REPO && python -m pytest tests/unit/test_<category>.py -v -k "<tool_name>"
```

<HARD-GATE>
Test MUST pass. Do NOT proceed until green.
</HARD-GATE>

### Step 4.6: Write JS UXP handler

Add to `plugin/commands/<category>.js`:

**Rules:**
- Handler name: camelCase, MUST match Python `createCommand` action name
- MUST use `findLayer(layerId)` + null check + `execute()` + `selectLayer()` pattern
- batchPlay descriptor MUST match the API docs from Phase 3
- Export in `commandHandlers` object
- If new file → add import in `plugin/commands/index.js`

### Step 4.7: Update registry

Add entry to `registry/tools.json`:

```json
{
  "name": "<snake_case>",
  "description": "<한국어 설명 — 기술 용어 금지>",
  "category": "<category>",
  "parameters": { ... },
  "enabled": true,
  "added_at": "<YYYY-MM-DD>",
  "files": {
    "mcp": "mcp/tools/<category>.py",
    "plugin": "plugin/commands/<category>.js",
    "test": "tests/unit/test_<category>.py"
  }
}
```

### Step 4.8: Commit on branch

```bash
cd $REPO && git add -A && git commit -m "feat: add <tool_name> tool"
```

---

## Phase 5: 4-Gate QA

<HARD-GATE>
ALL 4 gates MUST pass. You MUST run every command and read every output.
"Should pass" is NOT evidence. Run it.
</HARD-GATE>

### Gate 1: Code Quality

```bash
cd $REPO && python -m ruff check mcp/ && echo "GATE1-LINT: PASS" || echo "GATE1-LINT: FAIL"
```

```bash
cd $REPO && python -m pytest tests/unit/ -v && echo "GATE1-TEST: PASS" || echo "GATE1-TEST: FAIL"
```

<HARD-GATE>
Both MUST show PASS. Fix and re-run until green. Do NOT proceed with any FAIL.
</HARD-GATE>

### Gate 2: Regression

```bash
cd $REPO && python -m pytest tests/regression/ -v && echo "GATE2-REGRESSION: PASS" || echo "GATE2-REGRESSION: FAIL"
```

```bash
cd $REPO && python scripts/check_registry.py && echo "GATE2-REGISTRY: PASS" || echo "GATE2-REGISTRY: FAIL"
```

<HARD-GATE>
Both MUST show PASS. A regression failure means you broke an existing tool. Fix before proceeding.
</HARD-GATE>

### Gate 3: Non-Developer Safety

Verify these 4 items manually by reading the code and registry:

- [ ] **Default-callable**: Can be called with ONLY `layer_id` (all others have defaults)
- [ ] **Korean description**: `registry/tools.json` description is in Korean, no tech jargon
- [ ] **Parameter descriptions**: Every parameter in registry has a `description` field
- [ ] **Error message**: `findLayer` null check produces a readable error

```bash
cd $REPO && python3 -c "
import json
reg = json.load(open('registry/tools.json'))
tool = [t for t in reg['tools'] if t['name'] == '$TOOL_NAME'][0]
print('Description:', tool['description'])
missing = [k for k,v in tool['parameters'].items() if 'description' not in v and not v.get('required')]
print('Missing descriptions:', missing or 'NONE')
has_defaults = all('default' in v or v.get('required') for v in tool['parameters'].values())
print('All have defaults:', has_defaults)
"
```

<HARD-GATE>
ALL 4 checks MUST pass. A non-developer will use this tool. If the description has tech jargon or defaults are missing, the tool is NOT ready.
</HARD-GATE>

### Gate 4: E2E (Human-Assisted)

**Pre-flight — run ALL 3 checks:**

```bash
lsof -i :3001 > /dev/null 2>&1 && echo "PROXY: RUNNING" || echo "PROXY: NOT RUNNING"
pgrep -x "Adobe Photoshop" > /dev/null 2>&1 && echo "PHOTOSHOP: RUNNING" || echo "PHOTOSHOP: NOT RUNNING"
```

If any NOT RUNNING, tell user exactly what to start:
> "Gate 4를 진행하려면:
> [미충족 항목만]
> 준비되면 알려주세요."

**Wait for user confirmation. Do NOT fake this check.**

Once ready, call the new tool with default parameters and confirm with user:
> "효과가 적용된 것 같나요?"

- User confirms → GATE 4: PASS
- User says no → Fix and re-run from Gate 1

---

## Phase 6: PR + Merge

<HARD-GATE>
You MUST create a PR. NEVER push directly to main.
</HARD-GATE>

### Step 6.1: Push branch + create PR

```bash
cd $REPO && git push origin feat/add-$TOOL_NAME
```

```bash
cd $REPO && gh pr create \
  --title "feat: add $TOOL_NAME tool" \
  --body "$(cat <<'PREOF'
## New Tool: $TOOL_NAME

**Description:** <한국어 설명>
**Category:** <category>

## QA Results
- [x] Gate 1: Code Quality (lint + unit tests)
- [x] Gate 2: Regression (all existing tools unaffected)
- [x] Gate 3: Non-Developer Safety (defaults, Korean desc, error msgs)
- [x] Gate 4: E2E (tested in Photoshop)

🤖 Generated with [Claude Code](https://claude.com/claude-code)
PREOF
)"
```

### Step 6.2: Merge (after CI passes)

```bash
cd $REPO && gh pr merge --squash --auto
```

If merge fails → check CI output → fix → re-push.

### Step 6.3: Confirm to user

> "도구 준비됐어요! [도구 설명]. 써볼까요?"

---

## Failure Protocol

| 상황 | 대응 |
|------|------|
| Gate 1-3 실패 | 수정 → 해당 Gate부터 재실행 |
| Gate 4 실패 | batchPlay descriptor 재검토 → Phase 3부터 |
| 3회 연속 실패 | 사용자에게 설명: "자동 생성이 어려운 도구예요. 엔지니어에게 전달할게요." |
| Merge 실패 | CI 로그 확인 → 수정 → re-push |

## Red Flags — STOP Immediately

- About to write code without feasibility check
- About to merge without all 4 gates passing
- About to claim "PASS" without running the command
- Skipping a gate because "it's obvious"
- Using "should work" instead of running verification
- Writing JS handler without checking batchPlay docs first
- Committing directly to main
- Proceeding after a FAIL without fixing

**All of these mean: STOP. Go back to the last passing gate.**

## Naming Conventions

| Item | Convention | Example |
|------|-----------|---------|
| Python function | snake_case | `add_inner_glow_layer_style` |
| JS handler | camelCase | `addInnerGlowLayerStyle` |
| Action name | camelCase | `addInnerGlowLayerStyle` |
| Branch | feat/add-{name} | `feat/add-inner_glow` |
| Registry description | 한국어, 비기술 | "레이어 안쪽에 빛나는 효과를 적용합니다" |
