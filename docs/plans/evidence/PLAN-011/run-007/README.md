# PLAN-011 실제 겹침 드래그 입력 최종 검증

- 명령: `./scripts/godot.ps1 -Action OverlayStackSmoke -NativeInput -MonitorId GSM5BDE -EvidenceDirectory docs/plans/evidence/PLAN-011/run-007 -LogFile docs/plans/evidence/PLAN-011/run-007/execution.log`.
- 사용자의 최종 30초 마우스 중지 응답 후 소유한 별도 수신기를 활성화하고 실제 SendInput 경로를 검사했다. 종료 시 커서·포커스 복원.
- [결과](results.json): 197개 PASS. 999·049·데이터·좀비 각각 실제 오른쪽 누름, 18단계 겹침 이동, 해제를 확인했다. 각 단계에서 커서 위치와 드래그 대상의 실제 native 우선순위를 대조했다.
- 좀비는 제품 치료 명령으로 생성했다. 앞쪽 좀비 클릭은 회수되고 뒤쪽 수신기에 전달되지 않으며, 숨긴 영역은 뒤쪽 수신기에 down/up 모두 전달됐다.
- [trace](stack-trace.json): 각 드래그 구간의 999/049 상대 순서 반전 0회. 데이터·좀비 우선순위는 매 단계 별도의 실제 HWND 순서 검사로 확인했다.
- `frames/` 72장. 대표: [999 앞](frames/00012-native-999.png), [049 앞](frames/00030-native-049.png), [데이터 앞](frames/00048-native-data.png), [좀비 앞](frames/00066-native-zombie.png).
- [로그](execution.log), [수신 결과](receiver.json), [환경](monitor.json), [소스 해시](source-hashes.json). 실제 캡처에서 각 드래그 대상이 앞에 유지되는 것을 확인했다.
