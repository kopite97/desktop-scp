# PLAN-017 run-054

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `./scripts/observe-tray-startup.ps1 -Executable .artifacts/PLAN-017/windows-x64/SiteDesktop.exe -EvidenceDirectory docs/plans/evidence/PLAN-017/run-054 -SettingsFile docs/plans/evidence/PLAN-017/run-048/settings.json -ExpectedActors 'SCP-999','SCP-049'`
- 결과: FAIL.
- 해석: 통합 판정 FAIL: 초기 64px 투명 부트 창이 1개 관찰 샘플에서 foreground. 제어창은 계속 숨김, SCP-173 OFF 복원/999·049 표시 및 정상 종료는 확인. 실패를 통과로 바꾸지 않음. 시작 관찰은 테스트 개수 대신 PID·샘플·표시/foreground·종료 조건으로 판정한다.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [execution.log](execution.log)
- [results.json](results.json)
