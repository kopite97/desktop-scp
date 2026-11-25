# PLAN-007 SCP-173 검증

- 2026-11-25 Windows/Godot 4.7.2 .NET, LG GSM5BDE. 임시 도형의 173 본체와 실제 런타임 아이콘으로 검증했다.
- 명령: Build, `StatueSmoke -Headless` [run-001](../run-001/results.json), `StatueSmoke -MonitorId GSM5BDE -EvidenceDirectory docs/plans/evidence/PLAN-007/run-003 -LogFile docs/plans/evidence/PLAN-007/run-003/execution.log`.
- [최종 결과](results.json), [실행 로그](execution.log). C# 빌드 경고/오류0.
- 실제 상태의 180px 경계/8초/스트레스70 이상4초, 다른 모니터 공격, 관찰 재진입 리셋, 예약 경합, 빈 대상, 중복 파손, .8초 정리, 숨김/종료, 999 공통 스트레스 감소 PASS.
- native: 실제 아이콘 취득/분할 그림 alpha/원본 appearance ID 유지, LG 물리 좌표, 창 정리, 실제 커서 관찰값→제품 상태, 숨김/복원·공통 스트레스 해제 PASS.
- native 시간 경계/공격은 같은 제품 상태에 재현 커서와 경과 시간을 전달했다. 실제 커서 관찰 연결은 별도로 검사했다. 자동 마우스 드래그 검사는 이번에 수행하지 않았다.
- 사용자 실제 아이콘의 그림/이름/경로를 증거에 저장하지 않았다. 173 정식 아트·효과음과 강한 외부 연출은 이번 기능 검증에 포함하지 않았다. 강한 연출은 PLAN-008 담당이다.
