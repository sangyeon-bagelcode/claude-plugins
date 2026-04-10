---
name: gs-os
description: "슬롯 게임, 온톨로지, 태그, 게임 검색, 게임 비교, 유사 게임, 포트폴리오 통계가 필요할 때 사용. 트리거: '게임 찾아줘', '태그 검색', '유사한 게임', '온톨로지', 'free_spin', '게임 통계', '슬롯 분석', '게임 목록', 'way_pay', 'jackpot', 게임 ID 조회, 두 게임 비교 등."
---

# gs-os — 슬롯 게임 온톨로지 CLI

<HARD-GATE>
슬롯 게임 데이터가 필요하면 반드시 gs-os CLI를 사용한다.
로컬 JSON 파일 직접 읽기 금지. MCP 서버(gs-os-ontology) 폐기됨.
</HARD-GATE>

미설치 시 `/gs-connect` 스킬로 설치.

## 커맨드

```bash
gs-os search "<자연어>"           # 자연어 → 태그 매칭 검색
gs-os get <game_id>              # 게임 상세 (태그, 소스, evidence)
gs-os similar <game_id>          # 유사 게임 (Jaccard)
gs-os list                       # 게임 목록 + 필터
gs-os diff <id1> <id2>           # 두 게임 태그 비교
gs-os dict [tag_name]            # 태그 사전
gs-os stats                      # 포트폴리오 통계
```

## 주요 옵션

- `--limit <N>` — 결과 수 제한
- `--type <SLOT_MACHINE|VIDEO_POKER|KENO|BLACKJACK>` — 게임 타입 필터
- `--tags <t1,t2>` — 태그 필터 (list)
- `--layer <1-4>` — 태그 레이어 필터
- `--stats` — 사용 빈도 (dict)
- `--group-by <tag|layer|game_type|generation|jackpot_type>` — 그룹 집계 (stats)

## 출력

JSON. `jq`로 파싱.

```bash
gs-os get 272 | jq '.data.tags'
gs-os search "프리스핀" | jq '.data[].game_name'
```
