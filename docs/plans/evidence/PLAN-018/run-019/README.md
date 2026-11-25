# PLAN-018 run-019

- 환경: Windows, Godot4.7.2 mono/.NET8.0.318. 모의 화면과 실제 두 화면(96dpi, 주3440×1440/보조1920×1080·원점-1920,1061)을 구분한다.
- 대상/명령(대기 상한 옵션 생략 가능): `./scripts/observe-tray-startup.ps1 -Executable .artifacts/PLAN-018/windows-x64/SiteDesktop.exe -EvidenceDirectory docs/plans/evidence/PLAN-018/run-019 -SettingsFile docs/plans/evidence/PLAN-018/preview-settings.json -ExpectedActors 'SCP-049'`
- 결과: PASS.
- 해석: 최종 EXE 시작 관찰: 049만 활성, 제어창 숨김, 정상 종료. 샘플232/제어창205/049표시196/예상 외 표시0/exit0.
- 한계: 실제 혼합 DPI/하드웨어 분리 미검증, 장시간 성능 주장 없음. 실제 입력은 승인 후 소유 수신기로만 검사.
- 선택 자료: 전환 기록은 포털 로직, 화면 캡처는 필요한 native 회차에만 생성. 비용 측정은 범위 밖.

- [execution.log](execution.log)
- [results.json](results.json)
