# PLAN-003 run-001

2026-11-23 최초 구현 검증. 환경은 [최종 회차](../run-004/README.md)와 같다.

- Build: 0경고·0오류. Import: 성공.
- OverlaySmoke headless: 9개 통과.
- OverlaySmoke Windows: 실패. Godot 화면 원점을 그대로 Win32 상태에 사용하여 native 위치 일치 검사가 실패했다.
- 테스트를 유지하고 가상 화면 원점 변환을 WindowsDesktop에 추가했다. 수정 결과는 [run-002](../run-002/README.md).
- 로그: [build](build.log), [import](import.log), [headless](overlay-headless.log), [Windows 실패](overlay-windows.log).
- 명령: 루트에서 godot.ps1의 Build, Import, OverlaySmoke -Headless, OverlaySmoke를 실행하고 이 디렉터리에 로그 저장.
- 최종 성공 증거가 아니다. 화면·results.json은 당시 미생성이고 실패 로그를 원본으로 보존한다.
