# PLAN-005 run-009 — 윤곽 재사용 후 애니메이션 비용

- 2026-11-24. r003 idle/move 반복, Godot 4.7.2 .NET / Windows / Compatibility / NVIDIA RTX 5070.
- 실행: `./scripts/godot.ps1 -Action LightingProbe -Revision r003 -EvidenceDirectory docs/plans/evidence/PLAN-005/run-009 -LogFile docs/plans/evidence/PLAN-005/run-009/execution.log -MeasureOnly -Preview`.
- 프레임·좌우 방향별 입력 윤곽을 캐시하고 임시 native 리소스를 즉시 정리한 빌드다. 이미지·조명 파라미터는 run-007과 같다.
- 1개/2개 표시 창, 같은 128×128 렌더 텍스처·같은 애니메이션. base/lit 교차 3회, 준비 2초+측정 10초, VSync·60FPS 제한.
- 이 회차는 비용 전용이다. 이미지/재생 증거는 [run-007](../run-007/README.md), 실제 클릭은 [run-006](../run-006/README.md)에 있다.
- [로그](execution.log), [원본 수치](performance.json), [CSV](performance.csv), [대응 회차 판정](performance-summary.json).
- 판정 PASS: 6쌍 모두 추가 비용 예산 충족. 이상치를 제외하지 않았다. CPU 렌더 p95 추가 최대 .014ms, GPU 렌더 p95 추가 최대 .005ms, 프로세스 CPU 추가 최대 .936%p(1코어=100%), working set 추가 최대 .165MiB.
- 측정 전체 working set 변화 .418MiB로, run-008의 약 20.6MiB 증가가 크게 줄었다. 장시간 누수 검사를 대신하는 것은 아니다.
- 음수 차이는 실행 간 잡음/스케줄링을 포함하며 조명이 성능을 개선한다는 의미가 아니다. 프레임 간격 p95에는 VSync 대기가 포함되므로 렌더 CPU/GPU 비용과 구분한다.
- 한 개체 상태의 렌더 텍스처를 최대 두 창에 공유하는 진단 조건이다. 여러 종류/독립 동작 개체·모든 GPU·제품 전체의 성능 보증은 아니다.
