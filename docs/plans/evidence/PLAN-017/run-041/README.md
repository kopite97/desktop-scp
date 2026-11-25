# PLAN-017 run-041

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `./scripts/godot.ps1 -Action BehaviorSmoke -MonitorId GSM5BDE -Measure -EvidenceDirectory docs/plans/evidence/PLAN-017/run-041 -LogFile docs/plans/evidence/PLAN-017/run-041/execution.log`
- 결과: PASS, 검사 21개.
- 해석: 실시간 3회 비용 및 동일 프레임 20개 소비자의 관찰 공유 확인. 고정 입력 CPU 비교 아님.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [execution.log](execution.log)
- [metrics.json](metrics.json)
- [results.json](results.json)
- [transitions.jsonl](transitions.jsonl)
