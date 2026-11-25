# PLAN-017 run-021

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `./scripts/godot.ps1 -Action Scp049Smoke -Headless -EvidenceDirectory docs/plans/evidence/PLAN-017/run-021 -LogFile docs/plans/evidence/PLAN-017/run-021/execution.log`
- 결과: FAIL.
- 해석: 실패: 진단의 화면 교체가 의사만 갱신. PlagueScenario에서 월드/의사 각각 호출하도록 이전.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [execution.log](execution.log)
