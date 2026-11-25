# PLAN-011 실제 창 순서·수명 최종 검증

- 명령: `./scripts/godot.ps1 -Action OverlayStackSmoke -MonitorId GSM5BDE -EvidenceDirectory docs/plans/evidence/PLAN-011/run-006 -LogFile docs/plans/evidence/PLAN-011/run-006/execution.log -TimeoutSeconds 90`.
- 환경: Windows 11, Godot 4.7.2 .NET Compatibility / RTX 5070, LG GSM5BDE. [monitor.json](monitor.json), [실행 소스 해시](source-hashes.json).
- [결과](results.json): 25개 PASS. 1798 실제 프레임, 정지·느린 드래그·빠른 드래그 각각 10초.
- [trace](stack-trace.json): 각 구간 내 의도하지 않은 상대 순서 반전 0회. 각 안정 구간의 반복 native 배치 적용 0회. 전체 21배치는 드래그/수명/고의 이탈 같은 상태 전환을 포함한 수치다.
- 드래그 시작/해제 5회, 의사·데이터 우선순위, 객체 제거, 드래그 중 숨김/복원, 고의 순서 이탈 복구, 반응 생성·만료·다른 개체 드래그 우선순위를 확인했다.
- `frames/`는 실제 Windows 합성 화면을 매 2프레임 캡처했다. 대표 [느린 드래그](frames/00602-slow-drag.png), [빠른 드래그](frames/01202-fast-drag.png). 캡처는 소유 테스트 배경 위이며 별도 렌더를 합성한 이미지가 아니다.
- 이 실행의 이동은 제품 공통 상태 경로를 사용한 시나리오다. 실제 마우스 이벤트 검증은 [run-007](../run-007/README.md)에서 수행했다.
- [로그](execution.log) 오류 없음. 혼합 DPI 실기기·핫플러그·장시간 성능·배포 내보내기 미검증.
