# claude-plugins

GS팀의 Claude Code 플러그인 마켓플레이스

## 포함된 플러그인

| 플러그인 | 설명 | 커맨드 | 스킬 |
|---------|------|--------|------|
| **dev** | 슬롯 게임 서버 개발 도구 | `/dev:slot-server-impl` `/dev:run-sim` | `conventions` `slot-server-dev` `math-model` `bonus-flow` `spin-flow` `game-mapping` `run-simulation` `blackboard-path` `contents-event` `new-api` `engine-architecture` `feature-patterns` `data-systems` `coding-rules` |
| **qa** | QA · 테스트 도구 | `/qa:test-case` | `bug-report` |
| **ta** | 테크니컬 아트 파이프라인 도구 | `/ta:asset-check` | `performance-budget` |
| **design** | 기획 도구 | `/design:spec-review` | `data-table-rules` |
| **sound** | 사운드 디자인 도구 | `/sound:audio-check` | `naming-conventions` |
| **pm** | 프로젝트 관리 도구 | `/pm:sprint-summary` | `meeting-notes` |
| **adb-mcp** | Adobe MCP 설정 가이드 | `/adb-mcp:setup` | `adb-mcp-setup` |
| **onboard** | 마켓플레이스 온보딩 | `/onboard:setup` | `marketplace-guide` |
| **gws-cli-setup** | Google Workspace CLI 설치/설정 | `/gws-cli-setup` | `gws-cli-setup` |
| **gs-connect** | GS-OS Ontology MCP 서버 연결 설정 | `/gs-connect` | `gs-connect` |
| **jackpot-board-generator** | 슬롯 게임 에셋 자동 생성 (Unity MCP) | | `jackpot-board-generator` |

## 빠른 시작 (새 환경)

```bash
# 1. 마켓플레이스 추가
/plugin marketplace add bagelcode-gamestudio/claude-plugins

# 2. 온보딩 실행 — 역할 선택 후 자동 설치
/onboard:setup

# 또는 역할 직접 지정
/onboard:setup dev
/onboard:setup dev,qa
/onboard:setup all
```

## 수동 설치

```bash
# 필요한 플러그인만 골라서
/plugin install dev@claude-plugins
/plugin install qa@claude-plugins
/plugin install design@claude-plugins
```

## 로컬 테스트 (GitHub에 올리기 전)

```bash
# 이 레포를 clone한 디렉토리에서
/plugin marketplace add .
/plugin install dev@claude-plugins
```

## 사용법

```bash
# 커맨드: 직접 호출
/dev:slot-server-impl 343
/dev:run-sim bbd
/qa:test-case 로그인 기능
/design:spec-review docs/gacha-spec.md

# 스킬: 관련 파일 작업 시 자동 활성화 (별도 호출 불필요)
```

## 구조

```
.claude-plugin/
  marketplace.json          ← 마켓플레이스 카탈로그
plugins/
  {plugin-name}/
    .claude-plugin/
      plugin.json            ← 플러그인 메타데이터
    commands/
      {command}.md           ← 슬래시 커맨드
    skills/
      {skill-name}/
        SKILL.md             ← 자동 활성화 스킬
```
