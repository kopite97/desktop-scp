# PLAN-017 run-004

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `./scripts/godot.ps1 -Action Scp999Smoke -Headless -EvidenceDirectory docs/plans/evidence/PLAN-017/run-004 -LogFile docs/plans/evidence/PLAN-017/run-004/execution.log`
- 결과: FAIL.
- 해석: 실패: 커서 발판 투영이 수평 거리를 무시하여 자동 도약 회귀. 2차원 거리로 수정 후 run-005 통과.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [execution.log](execution.log)
