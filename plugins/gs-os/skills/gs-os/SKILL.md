---
name: gs-os
description: "슬롯 게임, 온톨로지, 태그, 게임 검색, 게임 비교, 유사 게임, 포트폴리오 통계가 필요할 때 사용. 트리거: '게임 찾아줘', '태그 검색', '유사한 게임', '온톨로지', 'free_spin', '게임 통계', '슬롯 분석', '게임 목록', 'way_pay', 'jackpot', 게임 ID 조회, 두 게임 비교 등."
---

# gs-os — 슬롯 게임 온톨로지 CLI

<HARD-GATE>
슬롯 게임 데이터가 필요하면 반드시 gs-os CLI를 사용한다.
로컬 JSON 파일 직접 읽기 금지. MCP 서버(gs-os-ontology) 폐기됨.
</HARD-GATE>

## 사용

1. `gs-os --help`로 사용 가능한 커맨드를 확인한다.
2. 각 커맨드의 옵션은 `gs-os <command> --help`로 확인한다.
3. 출력은 JSON. `jq`로 파싱한다.

미설치 시 `/gs-connect` 스킬로 설치.
