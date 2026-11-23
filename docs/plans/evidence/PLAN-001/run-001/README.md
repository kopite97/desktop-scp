# PLAN-001 검증 증거 / run-001

- 날짜: 2026-11-23 (Asia/Seoul 사용자 날짜 기준).
- 작업 루트: `C:/Dev/scp_desktop`.
- 환경: Windows, PowerShell, Node.js v26.13.0. 별도 npm 패키지 설치 없음.
- 대상: PLAN-001의 문서 체계·구조 검사기·검사기 회귀 테스트.
- 관련 작업: [PLAN-001](../../../PLAN-001-agent-harness.md).

## 실행과 결과

| 실행 | 방법 | 결과 | 증거 |
| --- | --- | --- | --- |
| 구현 후 문서 검사 | `node scripts/validate-agent-docs.mjs` | 종료 코드 0, PLAN 8개·active 1개(PLAN-001), TECH 1개, 로컬 파일 링크 143개 검사 통과 | [최초 문서 검사 로그](doc-validation-active.log) |
| 검사기 회귀 검증 | `node --test scripts/validate-agent-docs.test.mjs` | 종료 코드 0, 테스트 10개·통과 10·실패 0·건너뜀 0 | [테스트 로그](validator-tests.log) |
| 완료 상태 전환 후 문서 검사 | `node scripts/validate-agent-docs.mjs` | 종료 코드 0, PLAN 8개·active 0개, TECH 1개, 로컬 파일 링크 148개 검사 통과 | [상태 전환 후 로그](doc-validation.log) |

완료 상태 전환 후 문서 검사는 통과했다. PLAN-001 completed, PLAN-002~008 proposed, active 0개다.
위 결과 링크 추가 후 문서 검사를 다시 실행하여 `record-validation.log`에 기록한다.
이 파일의 로그는 서로 다른 검증 단계의 파일이며 과거 실행을 덮어쓰지 않는다.

## 오류 탐지 범위

- 정상 proposed-only 프로젝트와 active 1개 허용, active 2개 거부.
- 필수 문서 누락·줄 수 초과 거부.
- 메타데이터 누락·필드 중복·잘못된 상태·유효하지 않은 날짜 거부.
- PLAN ID 중복·파일명 불일치·필수 섹션 누락·목록 타입/상태 불일치 거부.
- 목록 중복·미등록·잘못된 형식 탐지.
- TECH ID·상태·날짜·목록 일치 검사.
- 실제 링크 누락·잘못된 URL 인코딩 탐지, 코드 블록의 예시·웹 주소·앵커 제외.
- evidence Markdown은 링크를 검사하지만 PLAN으로 인식하지 않음.
- BOM·CRLF 허용, front matter 없는 PLAN 거부.

테스트는 임시 fixture에서 실행하며 제품 문서를 손상시키지 않는다.
0개 테스트나 실행 불가는 통과로 기록하지 않았다.

## 내용 검토와 보존

- 루트 AGENTS.md는 28줄의 탐색 MAP이다. 절차·명령·상태 전환 규칙은 하위 지침에 있다.
- 작업 지침 8종은 주제별로 분리했다. PLAN 관리도 docs/agents에 두었다.
- 기획 원본을 수정하거나 복제하지 않았고 MAP·로드맵으로 연결했다.
- 후속 PLAN 7개는 proposed이며 실제 제품 구현·설치·테스트 기록을 주장하지 않는다.
- Doom_Inc의 Unity·Android·자산 경로·특정 승인 규칙을 복사하지 않았다.
- 시작 시 Git 저장소가 아닌 것을 확인했으며 Git 초기화는 실행 범위 밖이다.
- 기획 원본의 검증 전·후 SHA-256: `7D7ADC3BFE0AB12373D7CFAF3A9BFA6E4A4354381F02515EAAE5FD12ADFB0E44`.

## 한계

- 실제 Godot·Windows 연동·SCP 행동·시각 효과·성능은 미검증이다.
- 자동 검사기는 문서 구조를 검증하며 승인 근거·완료 증거의 타당성을 판정하지 않는다.
- Markdown 인라인 로컬 파일 링크만 검사한다. 제목 앵커·참조형 링크·웹 주소는 검사하지 않는다.
