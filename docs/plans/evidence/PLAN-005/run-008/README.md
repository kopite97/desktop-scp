# PLAN-005 run-008 — 애니메이션 비용과 할당 개선

- 2026-11-24. `LightingProbe -Revision r003 -MeasureOnly -Preview`로 비용만 실행했다. 이미지/재생 검사 결과는 run-007을 참조한다.
- [로그](execution.log), [측정 원본](performance.json). Windows/Compatibility, 1개/2개 표시 창, 같은 128×128 공유 Viewport, idle/move 반복.
- base/lit 교차 순서로 각 준비 2초+측정 10초×3, VSync 활성·60FPS 제한. 기존 정지 5초 측정보다 입력 조건을 맞췄다.
- 렌더 추가 비용은 작았지만 전체 working set이 약 20.6MiB 증가했다. 프레임 전환마다 Image/Bitmap을 만들어 입력 윤곽을 재계산하는 코드가 원인이 될 수 있어 수정했다.
- 수정: 시작할 때 프레임·방향별 폴리곤을 한 번 계산하고 임시 Image/Bitmap을 Dispose한다. 재생 시 동일한 폴리곤을 재사용한다. 알파 경계 판정·좌표·반전 규칙은 바꾸지 않았다.
- 최종 비용 판정은 [run-009](../run-009/README.md)에 기록한다. 이 실행의 원본 수치는 보존했다.
