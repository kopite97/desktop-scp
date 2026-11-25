# PLAN-018 run-009

- 환경: Windows, Godot4.7.2 mono/.NET8.0.318. 모의 화면과 실제 두 화면(96dpi, 주3440×1440/보조1920×1080·원점-1920,1061)을 구분한다.
- 대상/명령(대기 상한 옵션 생략 가능): `./scripts/godot.ps1 -Action MonitorPortalSmoke -NativeInput -EvidenceDirectory docs/plans/evidence/PLAN-018/run-009 -LogFile docs/plans/evidence/PLAN-018/run-009/execution.log`
- 결과: FAIL.
- 해석: 행동/입력 검사는 끝났으나 Quit 이후 프레임 대기로 수신기 정리와 결과 기록이 완료되지 않아 FAIL. 시험 소유 수신기만 종료하고 종료 전 Dispose하도록 수정. 통과 증거로 사용하지 않는다.
- 한계: 실제 혼합 DPI/하드웨어 분리 미검증, 장시간 성능 주장 없음. 실제 입력은 승인 후 소유 수신기로만 검사.
- 선택 자료: 전환 기록은 포털 로직, 화면 캡처는 필요한 native 회차에만 생성. 비용 측정은 범위 밖.

- [execution.log](execution.log)
- [portal-0.png](portal-0.png)
- [portal-1.png](portal-1.png)
- [receiver.json](receiver.json)
- [receiver.log](receiver.log)
- [screens.json](screens.json)
- [transitions.jsonl](transitions.jsonl)
