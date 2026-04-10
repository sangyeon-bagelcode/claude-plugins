---
name: gs-os
description: "슬롯 게임, 온톨로지, 태그, 게임 검색, 게임 비교, 유사 게임, 포트폴리오 통계가 필요할 때 사용. 트리거: '게임 찾아줘', '태그 검색', '유사한 게임', '온톨로지', 'free_spin 태그', '게임 통계', '슬롯 분석', '게임 목록', 'way_pay', 'jackpot', 게임 ID로 조회, 두 게임 비교 등. gs-os CLI를 사용하여 서버 API에서 데이터를 가져온다."
---

# gs-os — 슬롯 게임 온톨로지 CLI

364개 슬롯 게임의 온톨로지(태그 기반 구조화된 지식)를 검색/조회하는 CLI 도구.

<HARD-GATE>
슬롯 게임 데이터, 태그, 온톨로지가 필요할 때 반드시 gs-os CLI를 사용한다.
로컬 JSON 파일(catalog_enriched.json 등)을 직접 읽지 않는다.
MCP 서버(gs-os-ontology)를 사용하지 않는다 — 폐기됨.
</HARD-GATE>

## 선행 조건

gs-os CLI가 설치되어 있어야 한다. 설치 여부 확인:

```bash
gs-os --version
```

미설치 시 `/gs-connect` 스킬을 호출하여 설치를 진행한다.

## 사용법

모든 출력은 JSON. `jq`로 파싱하여 필요한 데이터를 추출한다.

### 자연어 검색

슬롯 게임을 자연어로 검색한다. 내부적으로 쿼리를 태그로 변환 후 매칭.

```bash
gs-os search "와일드가 확장되는 프리스핀" --limit 10
gs-os search "hold and spin respin" --limit 5
gs-os search "캐스케이드 릴" --type SLOT_MACHINE
```

### 게임 상세 조회

game_id로 게임의 전체 정보(태그, 소스 위치, 클라이언트 메타, evidence)를 조회.

```bash
gs-os get 272
gs-os get 243 | jq '.data.tags'
gs-os get 1 | jq '.data.sources'
```

### 유사 게임

특정 게임과 태그 구성이 유사한 게임을 Jaccard 유사도로 검색.

```bash
gs-os similar 272 --limit 5
gs-os similar 243 --limit 10 | jq '.data[] | {game_name, similarity_score}'
```

### 게임 목록 + 필터

태그, 게임 타입, 태그 레이어로 필터링.

```bash
gs-os list --tags free_spin --limit 20
gs-os list --tags free_spin,way_pay --type SLOT_MACHINE
gs-os list --layer 1 --limit 50
```

### 두 게임 비교

두 게임의 태그 차이를 비교. 공통 태그, 각 게임 고유 태그, Jaccard 점수.

```bash
gs-os diff 272 243
gs-os diff 1 2 | jq '.data.common'
```

### 태그 사전

태그의 정의, 동의어, 사용 빈도를 조회.

```bash
gs-os dict free_spin
gs-os dict --stats
gs-os dict --layer 3 --stats
```

### 포트폴리오 통계

전체 게임 포트폴리오의 통계 요약 또는 그룹별 집계.

```bash
gs-os stats
gs-os stats --group-by game_type
gs-os stats --group-by tag
gs-os stats --group-by generation
```

## 언제 어떤 커맨드를 쓸까

| 상황 | 커맨드 |
|------|--------|
| "프리스핀이 있는 게임 찾아줘" | `gs-os search "프리스핀"` |
| "272번 게임 뭐야?" | `gs-os get 272` |
| "272번이랑 비슷한 게임?" | `gs-os similar 272` |
| "way_pay 태그 달린 게임 목록" | `gs-os list --tags way_pay` |
| "272랑 243 차이점" | `gs-os diff 272 243` |
| "free_spin 태그가 뭐야?" | `gs-os dict free_spin` |
| "전체 게임 통계" | `gs-os stats` |
| "게임 타입별 분포" | `gs-os stats --group-by game_type` |

## 주의사항

- 모든 출력은 JSON이므로 `jq`로 필요한 필드만 추출해서 사용한다.
- `--limit` 없이 실행하면 기본값이 적용된다 (search: 20, list: 50, similar: 10).
- 검색어는 한국어, 영어 모두 가능. 태그 이름, 동의어, 설명 텍스트를 모두 매칭한다.
