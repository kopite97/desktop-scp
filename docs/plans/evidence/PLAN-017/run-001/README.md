# PLAN-017 run-001

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `./scripts/godot.ps1 -Action BehaviorSmoke -Headless -Baseline -EvidenceDirectory docs/plans/evidence/PLAN-017/run-001 -LogFile docs/plans/evidence/PLAN-017/run-001/execution.log`
- 결과: FAIL.
- 해석: 실패: 진단 초기 갱신이 착지 프레임을 선택. 초기화 보완 후 run-002에서 변경 전 문제 재현.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [execution.log](execution.log)
