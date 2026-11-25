# PLAN-018 run-011

- 환경: Windows, Godot4.7.2 mono/.NET8.0.318. 모의 화면과 실제 두 화면(96dpi, 주3440×1440/보조1920×1080·원점-1920,1061)을 구분한다.
- 대상/명령(대기 상한 옵션 생략 가능): `./scripts/godot.ps1 -Action MonitorPortalSmoke  -EvidenceDirectory docs/plans/evidence/PLAN-018/run-011 -LogFile docs/plans/evidence/PLAN-018/run-011/execution.log`
- 결과: PASS, 검사94개.
- 해석: 비입력 native94개 통과. 당시 RGBA 알파 개수만 확인한 검사는 검은 캡처를 판별하지 못함. 실제 화면 가시성 근거로 사용하지 않고 run-016을 사용한다.
- 한계: 실제 혼합 DPI/하드웨어 분리 미검증, 장시간 성능 주장 없음. 실제 입력은 승인 후 소유 수신기로만 검사.
- 선택 자료: 전환 기록은 포털 로직, 화면 캡처는 필요한 native 회차에만 생성. 비용 측정은 범위 밖.

- [execution.log](execution.log)
- [portal-0.png](portal-0.png)
- [portal-1.png](portal-1.png)
- [results.json](results.json)
- [screens.json](screens.json)
- [transitions.jsonl](transitions.jsonl)
