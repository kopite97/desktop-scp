# PLAN-003 run-003

2026-11-23 빠른 드래그 수정 및 실제 입력·수명 검증. [종합 환경·최종 검사](../run-004/README.md).

## 실행과 결과

- 루트 PowerShell 7: godot.ps1 Build, OverlaySmoke -Headless, OverlaySmoke, Smoke -Headless, Run.
- [Build](build.log): 0경고·0오류. [headless](overlay-headless.log): 10개, [Windows](overlay-windows.log): 21개 통과.
- [환경 회귀](environment-headless.log): Main 초기화·3프레임·공통 종료·정리 통과.
- [제품 로그](overlay-interactive.log): 실제 드래그 (240,430)→(286,476), 클릭, Hide/Show, 종료 기록.
- 수신 창은 [run-002 InputProbe 로그](../run-002/input-probe.log)를 계속 사용했다. 초기 Down/Up=2/2.

## 실제 UI 조작

| 조작 | 기대와 실제 결과 |
| --- | --- |
| 첫 개체 로컬 (64,64)→(110,110) 드래그 | 제품 위치 +46,+46, count 1. 수신 2/2 유지 |
| 비워진 이전 몸체 위치 클릭 | 수신 3/3 |
| 이동된 첫 개체 몸체 클릭 | 제품 count 2. 수신 3/3 유지 |
| Hide 후 이전 몸체 클릭 | 둘 다 사라짐. 수신 4/4 |
| Show | 첫 개체 위치 (286,476), count 2 유지 |
| 두 번째 몸체 클릭 | 두 번째 count 1. 수신 4/4 유지 |
| Exit 후 두 번째의 이전 몸체 클릭 | 제품 창 종료, 수신 5/5 |
| InputProbe 닫기 | 종료 코드 0. SCP 테스트 창 0개 확인 |

- 화면: [이동](moved.jpg), [숨김](hidden.jpg), [복원·두 번째 클릭](restored.jpg), [종료 후 수신](exited.jpg).
- 정리된 조작 결과: [input-checks.json](input-checks.json). 이는 관찰 기록이며 자동 검사기 출력으로 표시하지 않는다.
- 제품과 수신 프로세스 종료 코드 0. 실제 마우스 이동 검증은 주 모니터 내에서 수행했다.
- 실제 혼합 DPI·물리 모니터 분리·RDP·성능·배포 내보내기는 미검증.
