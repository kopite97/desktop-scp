# PLAN-005 run-011 — 외곽선/VFX 조명 제외 샘플

- Windows / Godot 4.7.2 .NET / Compatibility OpenGL / NVIDIA RTX 5070, [PLAN-005](../../../PLAN-005-scp-999.md), ART-002 추가 범위.
- 실행: `./scripts/godot.ps1 -Action LightingProbe -Revision r005-sample -EvidenceDirectory docs/plans/evidence/PLAN-005/run-011 -LogFile docs/plans/evidence/PLAN-005/run-011/execution.log -TimeoutSeconds 120`.
- [로그](execution.log), [기본 조명 검사](results.json), [외곽선 검사](outline-results.json), [효과 검사](reaction-results.json).
- 4자세 × 2배율 × 좌/우/위 × 낮/밤 × 정/반전 = 96조건. 외곽선 RGB 오차·alpha 오류·상태 동기화 오류 모두 0. lit 몸체의 방향별 색 차이는 양수로 조명 반응 유지.
- 부모 modulate를 일부러 적용하면 외곽선 최대 RGB 차이 .156863이 생겼다. 부모 중립+몸체 self_modulate 구성에서는 외곽선 차이 0. Unshaded가 부모 색 변조를 없앤다고 가정하지 않는다.
- 하트·음표 16프레임×4 광원/주변색 조건: 색·alpha·투명 끝 프레임 오류 0. 색 비교는 8비트 premultiplied readback의 1단계/alpha 오차를 허용하며 alpha는 별도로 검사한다.
- [이전/분리 비교](outline-before-after.png): 기존 낮/분리 낮/기존 밤/분리 밤 순, 좌→우 원본 1/7/9/12. [1배](outline-1x.png), [효과](reaction-unshaded.png).
- 외곽선은 r004의 고유색을 유지하므로 기존 조명에 의해 더 검게 보이던 선과 차이가 있다. 굵기·도형 변경은 없다. 기술 관문 통과, 사용자 최종 시각 승인 아님.
- 진단 실제 렌더와 두 native 창 속성 확인이다. 제품 행동/물리/스폰·새 입력 수신 시험·전체 비용 검증은 이 회차에 포함하지 않는다.
