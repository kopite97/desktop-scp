# PLAN-017 run-053

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `./scripts/export-windows.ps1 -OutputDirectory .artifacts/PLAN-017/windows-x64 -LogFile docs/plans/evidence/PLAN-017/run-053/export.log -TimeoutSeconds 300`
- 결과: PASS.
- 해석: 최종 경로 캐시/공유 갱신 검증 보완으로 재내보내기. 동일 격리 산출물; 외부 배포 없음.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [export.log](export.log)
