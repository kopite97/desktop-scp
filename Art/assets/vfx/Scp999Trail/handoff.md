# Scp999Trail Godot 전달 기록

- 제작 [ART-003](../../../docs/plans/ART-003-scp-999-trail.md), [명세](spec.md). 개발 [PLAN-005](../../../../docs/plans/PLAN-005-scp-999.md), 요청 PLAN-005-SPR-003.
- 기준 본체 Scp999 r005. 독립된 이동 점액이며 본체 이미지·그림자·충돌/클릭 영역을 포함하지 않는다.

| 리비전 | 제출 | 기술 확인 | 제품 적용 | 사용자 확인 |
| --- | --- | --- | --- | --- |
| r001 | 2026-11-24, [PNG](revisions/r001/export/Scp999Trail.png), [Aseprite JSON](revisions/r001/export/Scp999Trail.json), [계약 JSON](revisions/r001/export/Scp999Trail.contract.json) | 18프레임·2태그, 원본/PNG·윤곽·알파·시간·끝 투명 검사 통과, 합성 리뷰 | PLAN-010 정식 적용 완료 | 2026-11-24 F-004 승인 |

- 최신 제출/승인/정식 제품 적용 r001. 2026-11-24 F-004 사용자 승인, PLAN-010 기술 검증 후 ART-003 및 PLAN-010 completed. 아래 PLAN-005 기록은 당시 임시 적용 이력이다.
- 실제 제품 경로: GodotProject/Assets/Sprites/vfx/Scp999Trail/. PNG/contract만 사용하며 source/review는 런타임 참조 대상이 아니다.
- [제품 적용 계약](../../../../docs/plans/evidence/PLAN-005/run-025/art-integration.md): 간격300ms+이동48px·상한12·유지1800ms의 프로토타입 값, 바닥 위치 고정·본체 뒤 배치·숨김/종료 정리. [실제 창/입력 증거](../../../../docs/plans/evidence/PLAN-005/run-025/README.md), 창 발판 이동/소멸 및 최악 조건 비용은 미검증이다.
- 32×16 셀·9열×2행. 출력 인덱스0~8=puddle_a, 9~17=puddle_b. Aseprite UI 인덱스는 +1.
- 피벗(16,8)을 생성 시점 본체 trailAnchor 좌표에 배치한다. 본체 뒤에 그리며 독립 위치를 유지한다. 좌우 반전 허용, 정수 배율/nearest, mipmap·repeat 끄기.
- 생성180ms → 유지 프레임(출력2/11) → 증발720ms → 제거. 원본 유지200ms와 리뷰 유지1800ms는 제품 확정 수명이 아니다. 표시 유지 시간·생성 간격·상한은 개발에서 정한다.
- 전체 Unshaded/Mix·중립 부모·원본 straight alpha. 추가 fade·주야간 변조 없음. 노멀·반사·발광·그림자 맵 없음.
- [낮/밤 합성 검수](revisions/r001/review/backgrounds-2x.png)는 본체 r005의 기존 엔진 렌더에 새 점액을 Aseprite로 합성했다. 새 점액의 실제 Godot 적용이나 재질 불변성 시험으로 기록하지 않는다.
- PLAN-005 후속 검증: 조명/CanvasModulate/부모 색 변조 제외, 생성·수명·상한, 발판 이동/소멸, 창 경계(y부착점 아래 최대4px 포함), 클릭 투과·숨김/복원/종료·성능.
- [검수 근거](../../../docs/plans/evidence/ART-003/run-001/README.md), [피드백](revisions/r001/review/feedback.md). ART 완료와 제품 적용 완료는 별개다.

## 2026-11-24 최종 제품 통합

- [최종 적용 계약](../../../../docs/plans/evidence/PLAN-005/run-035/art-integration.md): 원본/제출/런타임 파일 변경 없이 조명 밝기 수정·실제 창 물리·점액 지지면 부착/이동/제거 완료.
- [LG native105개 검사](../../../../docs/plans/evidence/PLAN-005/run-035/README.md), [공통 규칙66개/해시](../../../../docs/plans/evidence/PLAN-005/run-037/README.md). 위 초기 적용 시점의 창 물리 미검증은 이번 증거로 해소했다. 전체 비용은 새로 측정하지 않았다.
- 본체r005/Reactionr001 승인, Trailr001 미승인 임시 상태 유지. 점액의 사용자 승인/정식 적용은 [PLAN-010 후보](../../../../docs/plans/PLAN-010-scp-999-trail-integration.md)로 추적한다.

## 2026-11-24 승인본 정식 적용

- [F-004](revisions/r001/review/feedback.md)로 기존 LG 표시 r001 승인. [PLAN-010 결과](../../../../docs/plans/evidence/PLAN-010/run-001/README.md), [SHA256·매핑](../../../../docs/plans/evidence/PLAN-010/run-001/asset-receipt.json).
- 18프레임 실제 렌더의 원본 알파/조명 불변성/프레임 선택 오류0, 제품 native111개 통과. 기존 PNG/contract 그대로 정식 확정, 제품은 r001-approved로 기록한다.
- 남은 사용자 승인/정식 적용 항목 없음. 원본/제출 contract의 과거 pending 문구는 당시 제출 이력으로 보존한다.
