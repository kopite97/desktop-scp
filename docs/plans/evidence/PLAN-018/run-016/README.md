# PLAN-018 run-016

- 환경: Windows, Godot4.7.2 mono/.NET8.0.318. 모의 화면과 실제 두 화면(96dpi, 주3440×1440/보조1920×1080·원점-1920,1061)을 구분한다.
- 대상/명령(대기 상한 옵션 생략 가능): `./scripts/godot.ps1 -Action MonitorPortalSmoke -NativeInput -EvidenceDirectory docs/plans/evidence/PLAN-018/run-016 -LogFile docs/plans/evidence/PLAN-018/run-016/execution.log`
- 결과: PASS, 검사103개.
- 해석: 별도 소유 수신기가 화면을 덮은 상태에서 실제 데스크톱 포털 영역 캡처와 청록색 픽셀 확인. 양쪽 모두 눈으로 확인. 실제 클릭/우클릭 드래그/포커스·20회 정리 포함103개 통과.
- 한계: 실제 혼합 DPI/하드웨어 분리 미검증, 장시간 성능 주장 없음. 실제 입력은 승인 후 소유 수신기로만 검사.
- 선택 자료: 전환 기록은 포털 로직, 화면 캡처는 필요한 native 회차에만 생성. 비용 측정은 범위 밖.

- [execution.log](execution.log)
- [portal-desktop-0.png](portal-desktop-0.png)
- [portal-desktop-1.png](portal-desktop-1.png)
- [receiver.json](receiver.json)
- [receiver.log](receiver.log)
- [results.json](results.json)
- [screens.json](screens.json)
- [transitions.jsonl](transitions.jsonl)
