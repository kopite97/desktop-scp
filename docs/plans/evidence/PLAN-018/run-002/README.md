# PLAN-018 run-002

- 환경: Windows, Godot4.7.2 mono/.NET8.0.318. 모의 화면과 실제 두 화면(96dpi, 주3440×1440/보조1920×1080·원점-1920,1061)을 구분한다.
- 대상/명령(대기 상한 옵션 생략 가능): `./scripts/godot.ps1 -Action MonitorPortalSmoke -Headless -EvidenceDirectory docs/plans/evidence/PLAN-018/run-002 -LogFile docs/plans/evidence/PLAN-018/run-002/execution.log`
- 결과: FAIL.
- 해석: 실패: 삭제된 Dictionary 슬롯을 재사용하므로 Values.Last가 새 ID라는 진단 가정이 틀림. Spawn 반환 ID로 확인하도록 수정; 제품 판정 완화 없음.
- 한계: 실제 혼합 DPI/하드웨어 분리 미검증, 장시간 성능 주장 없음. 실제 입력은 승인 후 소유 수신기로만 검사.
- 선택 자료: 전환 기록은 포털 로직, 화면 캡처는 필요한 native 회차에만 생성. 비용 측정은 범위 밖.

- [execution.log](execution.log)
