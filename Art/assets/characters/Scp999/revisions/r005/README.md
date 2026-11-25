# Scp999 r005 — 조명에서 분리한 외곽선

- 2026-11-24, 사용자 지시로 [ART-002](../../../../../docs/plans/ART-002-scp-999-reaction.md)에 추가한 본체 후속 작업. 완료된 ART-001/r004는 보존한다.
- [편집 원본](../../source/Scp999-outline-unlit.aseprite), [제출 원본](source/Scp999-outline-unlit.aseprite). Aseprite 1.3.18.6/API41 Live, RGB/sRGB.
- [lit 기본색](export/Scp999_color.png), [unlit 외곽선](export/Scp999_outline.png), [노멀](export/Scp999_normal.png), [반사](export/Scp999_specular.png), [계약 JSON](export/Scp999.contract.json).
- [재합성 참조](export/Scp999_combined.png)는 r004 기본색과 동일한 비교용이다. split 기본색 대신 사용하면 외곽선을 이중 합성하므로 새 런타임 경로에서는 사용하지 않는다.

## 제작·합성 계약

- 64×64 셀, 3200×64·50열×1행, RGBA 1배·패딩/트리밍/회전/병합 없음. 50프레임·10태그·ms·피벗(32,56)·reactionAnchor/trailAnchor는 r004와 동일하다.
- 표시 레이어 BodyColor/OutlineUnlit/FaceLit, 숨김 NormalData/SpecularData/ReferenceR004. 기존 Outline 위치는 정확히 유지하고 BodyColor의 겹치는 픽셀만 제거했다.
- 외곽선은 원래 불투명 픽셀이다. lit 기본색과 외곽선의 커버리지는 겹치지 않으며 두 출력을 합치면 r004 RGBA와 정확히 같다. 표정은 기존 lit 경로다.
- 기본색/외곽선은 straight RGBA. 기본색은 CanvasTexture 노멀·반사 연결, 외곽선은 CanvasItemMaterial Unshaded/Mix. normal/specular는 r004 데이터와 동일하다.
- 중립 부모 아래 두 Sprite2D를 두고 한 상태에서 frame/FlipH/scale/position/visibility를 갱신한다. 본체만 변조할 때 self_modulate를 사용한다.
- 원본 윤곽이 어두워야 한다는 이유로 조명 값을 다시 곱하거나 불투명 외곽선 밑에 lit 윤곽을 남기지 않는다. 추가 빛·발광·굵기 변경 없음.
- 효과 Scp999Reaction r001은 별도 전체 Unshaded 기호이며 원본 파일을 변경하지 않았다.

## 검수·상태

- [분리 레이어 비교](review/layer-comparison.png): 위부터 기존 합성/lit 기본색/외곽선/노멀, 원본 프레임 1/7/9/12.
- [Art 전체 검사](../../../../../docs/plans/evidence/ART-002/run-003/README.md): 50프레임 재합성·원본/출력·노멀/반사·시간·태그 차이 0.
- [엔진 4장 관문](../../../../../../docs/plans/evidence/PLAN-005/run-011/README.md), [전체 엔진 검사](../../../../../../docs/plans/evidence/PLAN-005/run-012/README.md). 최신 검사 진행/결과는 해당 기록을 따른다.
- 전체 렌더 1200조건의 외곽선 색·alpha·동기화 오류 0. run-012의 시작 프레임 시간 오류를 수정하고 [최종 재생](../../../../../../docs/plans/evidence/PLAN-005/run-014/README.md)에서 486개 전환 최대 오차 16.757ms/p95 13.172ms를 확인했다. [추가 비용](../../../../../../docs/plans/evidence/PLAN-005/run-013/README.md) 6쌍은 모두 예산을 통과했다.
- [피드백](review/feedback.md), [최신 전달 상태](../../handoff.md). 제출·승인 리비전5, 2026-11-24 F-008로 r005 사용자 시각 확인 완료. 정식 제품 미적용.
