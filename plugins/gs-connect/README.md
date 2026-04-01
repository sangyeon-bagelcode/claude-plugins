# gs-connect

GS-OS Ontology MCP 서버 연결 설정을 도와주는 스킬입니다.

## 설치

```bash
claude plugin add bagelcode-gamestudio/claude-plugins --plugin gs-connect
```

## 사용법

```
/gs-connect
```

## 기능

- CloudFlare VPN 접속 안내
- 1Password에서 서버 IP 확인 안내
- MCP 클라이언트별 설정 자동 생성 (Claude Desktop, Claude Code, Cursor, Windsurf)
- stdio / SSE 트랜스포트 지원
- 연결 검증 (health check)

## 트리거

MCP 서버 연결, GS-OS 온톨로지 서버 접속, MCP 설정 등을 요청할 때 자동으로 트리거됩니다.
