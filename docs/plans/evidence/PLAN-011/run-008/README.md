# PLAN-011 SCP-049 입력 회귀 최종 검증

- 명령: `./scripts/godot.ps1 -Action Scp049Smoke -NativeInput -MonitorId GSM5BDE -EvidenceDirectory docs/plans/evidence/PLAN-011/run-008 -LogFile docs/plans/evidence/PLAN-011/run-008/execution.log`.
- run-007에 이어 사용자 마우스 중지 구간 안에서 실행했다. 두 실행 종료 후 커서 복원과 입력 완료를 알렸다.
- [결과](results.json) 71개 PASS. [로그](execution.log) 오류 없음. 테스트 수신기는 일반 창이며 제품 TOPMOST 창을 덮지 않는다.
- 실제 의사·데이터 오른쪽 드래그와 놓기, 데이터/좀비 클릭 회수, 999 쓰다듬기에 따른 실제 049 공통 스트레스 감소, 빈 영역·숨김 입력 투과 포함.
- 제품 공통 관리자와 양쪽 캐릭터가 함께 있는 경로를 검증했다. [소스 해시](source-hashes.json).
