# PLAN-018 run-020

- 환경: Windows, Godot4.7.2 mono/.NET8.0.318. 모의 화면과 실제 두 화면(96dpi, 주3440×1440/보조1920×1080·원점-1920,1061)을 구분한다.
- 대상/명령(대기 상한 옵션 생략 가능): `SiteDesktop.exe --max-fps 60 --quit-after 18000 --log-file <run-020>/execution.log -- --scp-settings-file <PLAN-018>/preview-settings.json; 사용자 생성/드래그/포털/Exit 조작`
- 결과: PASS.
- 해석: 사용자 최종 EXE 생성 위치·드래그·포털 정상 확인 및 Exit. 로그 EXIT_REQUESTED/STOPPED, PID44488 소멸 확인.
- 한계: 실제 혼합 DPI/하드웨어 분리 미검증, 장시간 성능 주장 없음. 실제 입력은 승인 후 소유 수신기로만 검사.
- 선택 자료: 전환 기록은 포털 로직, 화면 캡처는 필요한 native 회차에만 생성. 비용 측정은 범위 밖.

- [execution.log](execution.log)
- [launch.json](launch.json)
- [results.json](results.json)
- [stderr.log](stderr.log)
- [stdout.log](stdout.log)
