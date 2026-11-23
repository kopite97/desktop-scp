# PLAN-003 run-002

2026-11-23 원점 수정 후 검증. [최종 환경·종합 결과](../run-004/README.md).

- [Build](build.log): 0경고·0오류. [Windows 자동 검사](overlay-windows.log): 20개 통과.
- [제품 실행 로그](overlay-interactive.log), [입력 수신 프로세스 로그](input-probe.log), [초기 화면](overlay.png).
- 개체 사이 빈 곳: InputProbe Down/Up 0→1. 투명 모서리: 1→2. 첫 개체 몸체: 수신 수 2 유지, 제품 count 1.
- 빠른 드래그 실패: 제품 count는 2가 되었으나 위치 (240,430) 유지. 눌림 처리 때 현재 커서가 이미 목적지라 시작 오프셋을 잘못 계산했다.
- sky가 창 밖 드래그와 다른 창이 가리는 좌표를 거부한 시도는 제품 검증으로 세지 않았다.
- 수정은 입력 이벤트의 눌림 좌표 사용 및 release 최종 위치 반영이다. [run-003](../run-003/README.md)에서 실제 재검증했다.
- InputProbe는 run-003까지 동일 프로세스로 유지했다. 이 로그의 수신 3~5는 run-003 조작이며 복제하거나 다른 실행으로 표시하지 않는다.
- 제품과 InputProbe 모두 UI로 종료, 종료 코드 0. hidden 화면·input-checks·results는 이 회차에서 별도로 만들지 않았고 다음 회차에 기록했다.
- 명령: godot.ps1 Build, OverlaySmoke, InputProbe, Run. 각 실행의 LogFile은 이 디렉터리의 해당 로그.
