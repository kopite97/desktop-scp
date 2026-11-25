# PLAN-018 run-006

- 환경: Windows, Godot4.7.2 mono/.NET8.0.318. 모의 화면과 실제 두 화면(96dpi, 주3440×1440/보조1920×1080·원점-1920,1061)을 구분한다.
- 대상/명령(대기 상한 옵션 생략 가능): `./scripts/godot.ps1 -Action ScpActivationSmoke -Headless -EvidenceDirectory docs/plans/evidence/PLAN-018/run-006 -LogFile docs/plans/evidence/PLAN-018/run-006/execution.log`
- 결과: PASS, 검사70개.
- 해석: 기존 제품 기능의 관련 회귀 통과. 검사별 기대값은 results.json checks/tests와 로그 참조.
- 한계: 실제 혼합 DPI/하드웨어 분리 미검증, 장시간 성능 주장 없음. 실제 입력은 승인 후 소유 수신기로만 검사.
- 선택 자료: 전환 기록은 포털 로직, 화면 캡처는 필요한 native 회차에만 생성. 비용 측정은 범위 밖.

- [damaged.json](damaged.json)
- [damaged.json.recovered.json](damaged.json.recovered.json)
- [execution.log](execution.log)
- [future.json](future.json)
- [future.json.recovered.json](future.json.recovered.json)
- [not-a-directory](not-a-directory)
- [restart.json](restart.json)
- [results.json](results.json)
- [settings.json](settings.json)
