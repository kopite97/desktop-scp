# PLAN-011 수정 전 기준 관찰

- Windows 11 / Godot 4.7.2 .NET, LG GSM5BDE, 경계·DPI는 [monitor.json](monitor.json).
- 당시 명령: `./scripts/godot.ps1 -Action OverlayStackSmoke -Baseline -MonitorId GSM5BDE -EvidenceDirectory docs/plans/evidence/PLAN-011/run-001 -LogFile docs/plans/evidence/PLAN-011/run-001/execution.log`.
- Baseline 임시 옵션은 수정 전 계측용이며 현재 스크립트에서는 제거했다. 당시 소스는 [해시](source-hashes.json)로 식별한다.
- 제품 두 컨트롤러의 자동 입력은 끄고 같은 상태의 BeginDrag/Tick/Publish와 약 50ms Refresh 경로를 사용했다. 실제 OS 입력 재현은 아니다.
- 정지·느린 드래그·빠른 드래그 각 3초. 두 Refresh 직후와 프레임 경계의 실제 HWND 상대 순서를 기록했다.
- [trace](stack-trace.json): 각 구간 120회, 총 360회 상대 순서 반전. [로그](execution.log), [관찰 결과](results.json), 실제 화면은 `frames/`.
- 541 프레임 진행, 매 2프레임 화면 기록. 프레임 경계에서는 주로 049가 앞이며, 서브프레임 순서 교대가 모두 캡처되지는 않았다. 창 순서 반전 수를 화면 깜빡임 횟수로 해석하지 않는다.
- 결과의 passed=true는 계측 완료를 뜻하며, checks=0인 이 실행을 제품 수정 검증 통과로 계산하지 않는다.
