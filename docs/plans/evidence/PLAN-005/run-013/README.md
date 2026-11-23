# PLAN-005 run-013 — 재생 시계 수정과 외곽선 추가 비용

- [PLAN-005](../../../PLAN-005-scp-999.md), ART-002 추가 범위. 본체 Scp999 r005, 효과 Scp999Reaction r001.
- 실행: `./scripts/godot.ps1 -Action LightingProbe -Revision r005 -EvidenceDirectory docs/plans/evidence/PLAN-005/run-013 -LogFile docs/plans/evidence/PLAN-005/run-013/execution.log -PlaybackOnly -Measure -Preview`.
- run-012 시작 프레임 단축을 해결하기 위해 진단 재생에 Stopwatch 경과 시간을 사용한다. 제품 게임 시간 정책의 변경이 아니다.
- PlaybackOnly는 50장의 좌측/낮 이미지를 새로 렌더하고 6조건의 실제 재생 및 VFX 16장/4조건 검사를 수행한다. 전체 조명 행렬의 렌더 증거는 run-012다.
- 비용은 r004 합성 lit 경로와 r005 split lit 경로를 같은 128×128 공유 Viewport·1개/2개 표시 창·전체 50프레임 재생으로 교차 3회씩 비교한다. 각 준비2초/측정10초, VSync·60fps 제한, 두 진단 VFX 표시는 양쪽에 동일하게 유지한다.
- [로그](execution.log), [재생 범위](playback-scope.json), [재생 분석](playback-review.json), [효과 검사](reaction-results.json), [효과 시간](reaction-playback.json), [비용 원시값](performance.json), [비용 판정](performance-summary.json), [CSV](performance.csv).
- 비용 6쌍 모두 예산 통과. 추가 렌더 p95 최대 CPU .023ms/GPU .022ms, 프로세스 CPU 최대 +1.405%p(1코어=100%), 쌍별 working set 최대 +.157MiB. 전체 working set 변화 .821MiB 이내.
- 음수 차이는 측정 편차이며 분리 방식이 항상 더 빠르다는 근거로 해석하지 않는다. 전체 제품·독립 상태 다수 개체·모든 DPI의 성능 보증은 아니다.
- 몸체 실제 전환 486개, 최대 시간 오차 16.856ms·p95 13.338ms. run-012의 74.705ms 단축은 해결됐으나 시작/관측 경계가 한 프레임 섞여 있는 기록을 확인했다.
- 효과 첫 단계는 생성 후 다음 Process에서 시계를 시작해 약 30ms 더 길게 관찰됐다. 생성 시점에 시계를 시작하고 상태 변경 직후 직접 기록하도록 보완했다. 최종 재생은 [run-014](../run-014/README.md)로 판정한다.
- 비용은 준비2초 후 정상 재생 구간을 측정했다. 이후 변경은 시계 시작/관측 위치이며 렌더 노드·재질·측정 정상 구간은 동일하다. run-014에서 비용을 다시 측정했다고 보고하지 않는다.
