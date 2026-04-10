# gs-connect

GS-OS 온톨로지 CLI 설치 및 MCP → CLI 전환을 안내하는 스킬입니다.

## 설치

```bash
claude plugin add bagelcode-gamestudio/claude-plugins --plugin gs-connect
```

## 사용법

```
/gs-connect
```

## 기능

- 기존 MCP 서버 설정 제거 안내
- gs-os CLI thin client 설치 (sparse checkout)
- 환경변수 설정 (`GS_OS_SERVER_URL`)
- 연결 검증 (`gs-os stats`)
- REST API 직접 호출 안내

## 트리거

온톨로지 서버 연결, gs-os CLI 설치, MCP 설정 전환 등을 요청할 때 자동으로 트리거됩니다.
