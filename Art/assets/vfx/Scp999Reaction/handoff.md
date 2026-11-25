# Scp999Reaction 전달 기록

- 요청 [PLAN-005](../../../../docs/plans/PLAN-005-scp-999.md) / PLAN-005-SPR-002, 제작 [ART-002](../../../docs/plans/ART-002-scp-999-reaction.md), [명세](spec.md).
- 기준 본체 Scp999 r004. 타이핑 반응용 하트·음표이며 쓰다듬기 등 추가 이벤트에 재사용할지는 개발 상태 계약에서 정한다.

| 리비전 | 제출 | 기술 확인 | 실제 적용 | 사용자 확인 |
| --- | --- | --- | --- | --- |
| r001 | 2026-11-24, [PNG](revisions/r001/export/Scp999Reaction.png) · [Aseprite JSON](revisions/r001/export/Scp999Reaction.json) · [계약 JSON](revisions/r001/export/Scp999Reaction.contract.json) | 16프레임·2태그·파일/윤곽/알파 검사 통과, 합성 리뷰 | PLAN-005 제품 적용 | 2026-11-24 확인, F-002 |

- 최신 제출 r001, 승인 리비전 1, 2026-11-24 제품 적용. [파일·태그·생성 계약](../../../../docs/plans/evidence/PLAN-005/run-025/art-integration.md)과 [실제 입력/수명 검증](../../../../docs/plans/evidence/PLAN-005/run-025/README.md)을 연결한다.
- 2026-11-24 재개 시 원본 보존·진단 PNG/contract 사본 일치를 확인했다. [시각 검수 안내](../../../docs/plans/evidence/ART-002/run-004/README.md)에 하트·음표 재생과 실제 엔진 결과를 연결했고, 이후 r001 최종 사용자 확인을 받았다([F-002](revisions/r001/review/feedback.md)).
- 실제 제품 경로: GodotProject/Assets/Sprites/vfx/Scp999Reaction/. PNG/contract만 사용하며 런타임은 source/review를 직접 사용하지 않는다.
- 8열×2행, 32×32 셀. heart 0~7, note 8~15. Aseprite JSON은 0 기반이며 실제 프레임별 duration을 읽는다. 태그 repeat=1, contract loop=false.
- 프레임 끝 완전 투명, 총 850ms 후 제거. 스프라이트 알파만 소멸에 사용하고 별도 페이드 없음. nearest, mipmap/repeat 끄기, RGBA 색·알파 보존.
- 피벗 (16,16). 본체 현재 reactionAnchor의 화면 좌표 + 하트(-14,-10)/음표(14,-10)×정수 배율로 생성한다. 생성 좌표를 고정한 뒤 세계 위치를 위로 이동한다. 본체 FlipH를 효과에 상속하지 않는다.
- unlit 소형 기호: normal/specular/emission 맵 없음. 밤에 본체와 같은 어둡기 변조를 받지 않게 연결하고 실제 렌더 결과를 검증한다.
- ART-002 후속 엔진 시험: r001 파일 그대로 GodotProject/Assets/Diagnostics/Scp999Lighting/reaction-r001/에 복사했다. Unshaded/Mix·중립 부모·원본 소멸 알파를 사용한다. [샘플 검사](../../../../docs/plans/evidence/PLAN-005/run-011/README.md), [전체 검사](../../../../docs/plans/evidence/PLAN-005/run-012/README.md). 정식 제품 스폰/입력 적용과 구분한다.
- 레이어 순서: 본체보다 앞. 본체 표정 가독성은 생성 위치로 확보한다. 효과는 클릭/드래그 비대상이며 표시 창만 커졌다고 입력 영역을 확장하지 않는다.
- PLAN-005 담당: 생성 빈도/이벤트 병합/동시 상한, 상승 거리·수명, 창 경계, 숨김·복원·종료 정리, 실제 입력 투과·DPI·성능.
- [초기 검수](../../../docs/plans/evidence/ART-002/run-001/README.md)는 Aseprite 제작/합성이다. [최종 Godot 진단](../../../../docs/plans/evidence/PLAN-005/run-014/README.md)에서 16프레임 조명 제외·알파·850ms 소멸 재생을 확인했다. 이후 제품 생성/클릭 투과·native VFX 창 정리를 위 단계 B 증거로 확인했다.

## 2026-11-24 최종 제품 통합

- [최종 적용 계약](../../../../docs/plans/evidence/PLAN-005/run-035/art-integration.md): 원본/제출/런타임 파일 변경 없이 조명 밝기 수정·실제 창 물리·점액 지지면 부착/이동/제거 완료.
- [LG native105개 검사](../../../../docs/plans/evidence/PLAN-005/run-035/README.md), [공통 규칙66개/해시](../../../../docs/plans/evidence/PLAN-005/run-037/README.md). 위 초기 적용 시점의 창 물리 미검증은 이번 증거로 해소했다. 전체 비용은 새로 측정하지 않았다.
- 본체r005/Reactionr001 승인, Trailr001 미승인 임시 상태 유지. 점액의 사용자 승인/정식 적용은 [PLAN-010 후보](../../../../docs/plans/PLAN-010-scp-999-trail-integration.md)로 추적한다.

- 후속 2026-11-24: Scp999Trail r001도 F-004 사용자 승인 후 [PLAN-010](../../../../docs/plans/PLAN-010-scp-999-trail-integration.md) 정식 적용 완료. 위 PLAN-005 마감 당시의 점액 승인 대기는 해소되었다.
