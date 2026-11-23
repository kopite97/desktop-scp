# Scp999Reaction 전달 기록

- 요청 [PLAN-005](../../../../docs/plans/PLAN-005-scp-999.md) / PLAN-005-SPR-002, 제작 [ART-002](../../../docs/plans/ART-002-scp-999-reaction.md), [명세](spec.md).
- 기준 본체 Scp999 r004. 타이핑 반응용 하트·음표이며 쓰다듬기 등 추가 이벤트에 재사용할지는 개발 상태 계약에서 정한다.

| 리비전 | 제출 | 기술 확인 | 실제 적용 | 사용자 확인 |
| --- | --- | --- | --- | --- |
| r001 | 2026-11-24, [PNG](revisions/r001/export/Scp999Reaction.png) · [Aseprite JSON](revisions/r001/export/Scp999Reaction.json) · [계약 JSON](revisions/r001/export/Scp999Reaction.contract.json) | 16프레임·2태그·파일/윤곽/알파 검사 통과, 합성 리뷰 | 미적용 | 대기 |

- 최신 제출 r001, 승인 리비전 0, 제품 적용 없음. 같은 저장소의 export를 전달하며 불필요한 수령 사본을 만들지 않았다.
- 예정 제품 경로: GodotProject/Assets/Sprites/vfx/Scp999Reaction/. 런타임은 source/review를 직접 사용하지 않는다.
- 8열×2행, 32×32 셀. heart 0~7, note 8~15. Aseprite JSON은 0 기반이며 실제 프레임별 duration을 읽는다. 태그 repeat=1, contract loop=false.
- 프레임 끝 완전 투명, 총 850ms 후 제거. 스프라이트 알파만 소멸에 사용하고 별도 페이드 없음. nearest, mipmap/repeat 끄기, RGBA 색·알파 보존.
- 피벗 (16,16). 본체 현재 reactionAnchor의 화면 좌표 + 하트(-14,-10)/음표(14,-10)×정수 배율로 생성한다. 생성 좌표를 고정한 뒤 세계 위치를 위로 이동한다. 본체 FlipH를 효과에 상속하지 않는다.
- unlit 소형 기호: normal/specular/emission 맵 없음. 밤에 본체와 같은 어둡기 변조를 받지 않게 연결하고 실제 렌더 결과를 검증한다.
- ART-002 후속 엔진 시험: r001 파일 그대로 GodotProject/Assets/Diagnostics/Scp999Lighting/reaction-r001/에 복사했다. Unshaded/Mix·중립 부모·원본 소멸 알파를 사용한다. [샘플 검사](../../../../docs/plans/evidence/PLAN-005/run-011/README.md), [전체 검사](../../../../docs/plans/evidence/PLAN-005/run-012/README.md). 정식 제품 스폰/입력 적용과 구분한다.
- 레이어 순서: 본체보다 앞. 본체 표정 가독성은 생성 위치로 확보한다. 효과는 클릭/드래그 비대상이며 표시 창만 커졌다고 입력 영역을 확장하지 않는다.
- PLAN-005 담당: 생성 빈도/이벤트 병합/동시 상한, 상승 거리·수명, 창 경계, 숨김·복원·종료 정리, 실제 입력 투과·DPI·성능.
- [초기 검수](../../../docs/plans/evidence/ART-002/run-001/README.md)는 Aseprite 제작/합성이다. [최종 Godot 진단](../../../../docs/plans/evidence/PLAN-005/run-014/README.md)에서 16프레임 조명 제외·알파·850ms 소멸 재생을 확인했다. 제품 생성/클릭 투과·native VFX 창 통합은 미검증이다.
