# PLAN-018 실행 증거

2026-11-26 실행 승인에 따른 구현/검증. PLAN-017의 미커밋 변경 위에 작업했고 실패를 보존했다.

- [기준선](run-001/README.md): 실제 제품 상태로 화면 분산/모니터 간 경로 부재 재현.
- [최종 로직44개](run-017/README.md): 생성·포털·유예/우선순위·예약/봉쇄/취소·단일/세/세로 화면·장거리·자연 만료·중간 종료·공통 주체 완료1회.
- [실제 두 모니터103개](run-016/README.md): 양방향 실제 포털 가시성, 별도 수신기 down/up, 우클릭 드래그, 포커스 유지, 20회 창 정리. 화면 영역 캡처는 시험 수신기 위로 제한했다.
- 기존 회귀: [04943개](run-004/README.md), [공통 행동20개](run-005/README.md), [활성화70개](run-006/README.md), [99966개](run-012/README.md), [시스템18개](run-013/README.md), [좀비109개](run-014/README.md), [의사 아트68개](run-015/README.md).
- [최종 컴파일/내보내기](run-018/README.md), [EXE 시작 관찰](run-019/README.md), [사용자 EXE 확인/종료](run-020/README.md).

실제 화면 환경은 run-016/screens.json에 기록했다. 양쪽96dpi, 원점/높이가 다르며 모의144/192dpi와 구분한다. 모니터 변경/제거는 모의 제품 상태 경로로 검사했으며 하드웨어를 뽑거나 OS 배율을 바꾸지 않았다. 보조 화면 Window.GetTexture RGBA는 검게 읽힐 수 있어 run-010/011 PNG를 실제 시각 통과 근거로 사용하지 않는다. run-016의 실제 바탕 화면 캡처에서는 양쪽 포털을 확인했다.

| 회차 | 대상 | 판정 (검사 수) |
| --- | --- | --- |
| [run-001](run-001/README.md) | MonitorPortalSmoke | PASS (2) |
| [run-002](run-002/README.md) | MonitorPortalSmoke | FAIL |
| [run-003](run-003/README.md) | MonitorPortalSmoke | PASS (35) |
| [run-004](run-004/README.md) | Scp049Smoke | PASS (43) |
| [run-005](run-005/README.md) | BehaviorSmoke | PASS (20) |
| [run-006](run-006/README.md) | ScpActivationSmoke | PASS (70) |
| [run-007](run-007/README.md) | MonitorPortalSmoke | FAIL |
| [run-008](run-008/README.md) | MonitorPortalSmoke | FAIL |
| [run-009](run-009/README.md) | MonitorPortalSmoke | FAIL |
| [run-010](run-010/README.md) | MonitorPortalSmoke | PASS (101) |
| [run-011](run-011/README.md) | MonitorPortalSmoke | PASS (94) |
| [run-012](run-012/README.md) | Scp999Smoke | PASS (66) |
| [run-013](run-013/README.md) | SystemSmoke | PASS (18) |
| [run-014](run-014/README.md) | ZombieArtSmoke | PASS (109) |
| [run-015](run-015/README.md) | DoctorArtSmoke | PASS (68) |
| [run-016](run-016/README.md) | MonitorPortalSmoke | PASS (103) |
| [run-017](run-017/README.md) | MonitorPortalSmoke | PASS (44) |
| [run-018](run-018/README.md) | 빌드/EXE | PASS |
| [run-019](run-019/README.md) | 빌드/EXE | PASS |
| [run-020](run-020/README.md) | 빌드/EXE | PASS |
| [run-021](run-021/README.md) | 최종 문서·산출물 해시 | PASS |
