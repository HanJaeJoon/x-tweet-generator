---
name: verify
description: JJ Trader 웹/데몬을 실기동해 변경을 런타임에서 검증하는 레시피
---

# JJ Trader 동작 검증 레시피

## 기동 (3-프로세스)

```bash
pnpm daemon    # trader daemon :5175 (백그라운드로)
pnpm web:api   # 웹 API :5174 (백그라운드로)
pnpm web:dev   # Vite :5173 (UI 확인 시에만)
```

- 로컬 `.env`(apps/api/.env)에 GOOGLE_CLIENT_ID가 없으면 레거시 모드 - 로그인 없이 `/api/*` 직접 호출 가능.
- 인증 모드로 뜨면 `GOOGLE_CLIENT_ID= GOOGLE_CLIENT_SECRET=` 시스템 env 오버라이드로 레거시 강제 가능(시스템 env가 .env보다 우선).
- 벤더 키 오버라이드도 같은 방식: `TELEGRAM_BOT_TOKEN=fake ... pnpm daemon`으로 실패 경로 재현.

## API 표면 예시

```bash
curl http://127.0.0.1:5175/healthz                       # daemon 생존
curl http://127.0.0.1:5174/api/connection-check          # 마지막 연결 테스트 결과
curl -X POST http://127.0.0.1:5174/api/connection-check  # 연결 테스트 실행(daemon 프록시)
```

## UI 표면

- Vite는 localhost(IPv6 ::1)로 바인딩된다 - Playwright에서 `http://localhost:5173`을 쓸 것(127.0.0.1은 거부).
- Playwright는 저장소에 없다 - 스크래치패드에 `npm i playwright` 후 스크립트 실행이 빠르다.

## 주의 (gotcha)

- `pnpm daemon`을 TaskStop으로 죽여도 node 자식이 살아남는다(Windows). 포트로 PID 찾아 `taskkill //PID <pid> //F`.
- 토스 토큰 발급은 IP allowlist 필요 + 같은 client_id의 기존 토큰을 무효화한다. 로컬에서 POST /api/connection-check를 치면 배포 서버 토큰이 1회 401(자가 회복)될 수 있음.
- 검증 산출물 `apps/api/data/connection-check.json` 등 data/*는 gitignore 대상인지 커밋 전 git status로 확인.
