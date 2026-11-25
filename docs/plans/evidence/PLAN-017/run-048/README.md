# PLAN-017 run-048

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `SiteDesktop.exe --max-fps 60 --quit-after 18000 --log-file <run-048>/execution.log -- --monitor-id GSM5BDE --scp-settings-file <run-048>/settings.json; 사용자 Show controls/X/Alt+F4/Lockdown/Resume/설정/Exit`
- 결과: PASS.
- 해석: 사용자 실제 트레이 조작 + 로그/프로세스 확인. 두 번째 Exit 확인 후 PID 종료. 메뉴 자동 조작으로 기록하지 않음.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [execution.log](execution.log)
- [launch.json](launch.json)
- [results.json](results.json)
- [settings.json](settings.json)
