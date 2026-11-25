# PLAN-017 run-047

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `./scripts/observe-tray-startup.ps1 -Executable .artifacts/PLAN-017/windows-x64/SiteDesktop.exe -EvidenceDirectory docs/plans/evidence/PLAN-017/run-047 -SettingsFile docs/plans/evidence/PLAN-017/isolated-exe-settings.json`
- 결과: PASS.
- 해석: 해당 제품 상태/표현/입력 계약 회귀. 검사별 기대값은 results.json의 checks/tests와 실행 로그 참조. 시작 관찰은 테스트 개수 대신 PID·샘플·표시/foreground·종료 조건으로 판정한다.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [execution.log](execution.log)
- [results.json](results.json)
