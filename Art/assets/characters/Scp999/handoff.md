# Scp999 Godot 전달 기록

- 요청 출처/적용 담당: [PLAN-005](../../../../docs/plans/PLAN-005-scp-999.md), PLAN-005-SPR-001.
- 최초 제작: [ART-001](../../../docs/plans/ART-001-scp-999-character.md) completed/r004. 사용자 지정 후속 작업: [ART-002](../../../docs/plans/ART-002-scp-999-reaction.md) active, [현재 명세](spec.md).
- 이력: [r001](revisions/r001/README.md) 전체 동작·[r002](revisions/r002/README.md) 조명 샘플. 최신 r004는 2026-11-24 사용자 시각 확인을 받았다. 정식 제품 적용은 별개다.
- 엔진 샘플 검증 담당: PLAN-005 단계 A(active). PLAN-009 독립 후보를 통합했다. 4장→idle/move 전체 재생 검증 순이며 r002는 50프레임 애니메이션을 대체하지 않는다.
- 최신 제출: [r005](revisions/r005/README.md), 외곽선 분리 전체 50장. 마지막 사용자 확인은 r004이며 기존 제출본은 보존한다. 정식 제품 통합은 아직 없다.
- 적용 예정: 프로젝트 루트 GodotProject/Assets/Sprites/characters/Scp999/.
- 원본은 Art에 유지한다. 제품으로 전달할 후보는 PNG·JSON·contract JSON이다.

| 리비전 | 수령 export | 기술 확인 | 제품 적용 | 사용자 시각 확인 |
| --- | --- | --- | --- | --- |
| r001 | [PNG](revisions/r001/export/Scp999.png) · [Aseprite JSON](revisions/r001/export/Scp999.json) · [계약 JSON](revisions/r001/export/Scp999.contract.json), 2026-11-23 제출 | 원본 대응·50프레임·10태그·알파·경계 통과 | 미적용 | 미확인 |
| r002 | [기본색](revisions/r002/export/Scp999_color.png) · [노멀](revisions/r002/export/Scp999_normal.png) · [반사](revisions/r002/export/Scp999_specular.png) · [계약 JSON](revisions/r002/export/Scp999.contract.json), 2026-11-23 제출 | 4프레임 3맵 대응·표정/윤곽 보존·데이터 검사 통과; 엔진 미검증 | 미적용 | 미확인 |
| r003 | [기본색](revisions/r003/export/Scp999_color.png) · [노멀](revisions/r003/export/Scp999_normal.png) · [반사](revisions/r003/export/Scp999_specular.png) · [계약 JSON](revisions/r003/export/Scp999.contract.json), 2026-11-24 | 12프레임 파일·엔진 조명/반전/알파·반복 재생 확인 | 진단 경로에만 복사 | 미확인 |
| r004 | [기본색](revisions/r004/export/Scp999_color.png) · [노멀](revisions/r004/export/Scp999_normal.png) · [반사](revisions/r004/export/Scp999_specular.png) · [계약 JSON](revisions/r004/export/Scp999.contract.json), 2026-11-24 | 50프레임·10태그 원본/출력·조명/반전·재생 확인 | 진단 경로에만 복사 | 2026-11-24 확인, F-006 |

## r005 추가 전달

r005 추가 전달: [기본색](revisions/r005/export/Scp999_color.png) · [외곽선](revisions/r005/export/Scp999_outline.png) · [노멀](revisions/r005/export/Scp999_normal.png) · [반사](revisions/r005/export/Scp999_specular.png) · [계약](revisions/r005/export/Scp999.contract.json). 진단 경로에만 복사, 사용자 확인 대기.

- 외곽선 포함 r004 기본색과 r005 outline을 함께 쓰지 않는다. r005 lit 기본색+outline의 합으로 한 번만 표시한다.
- 두 Sprite2D의 프레임·FlipH·배율·피벗·가시성을 동일 상태에서 갱신한다. neutral parent와 외곽선 Unshaded/Mix, 몸체 CanvasTexture 조명을 사용한다.
- 진단 위치 GodotProject/Assets/Diagnostics/Scp999Lighting/r005/. [전체 검사](../../../../docs/plans/evidence/PLAN-005/run-012/README.md). 제품 입력/물리 이벤트 연결은 여전히 PLAN-005 후속이다.

## r001 전체 동작 연결 참조

- 64×64 셀, nearest 2배, 피벗 (32,56). JSON 프레임 사각형은 0부터 시작하는 픽셀 좌표다.
- 태그 범위는 양끝 포함 0부터, Aseprite 편집 UI는 1부터다. duration은 프레임별 ms.
- JSON의 repeat 생략은 무한 반복 태그다. contract.tags.loop를 명시적으로 참고한다.
- 이동은 좌표의 수평 이동+끌림 그림이며 본체에 추가 상하 바운스를 주지 않는다.
- typing_react의 이미지 y 오프셋은 최대 -6px. 물리 점프 중복을 피한다.
- pet_recover·balance_recover는 별도 일회 태그다. 실제 상태 종료 시 연결한다.
- reactionAnchor는 프레임별, trailAnchor는 (32,56)이다. 효과 크기를 포함한 창 영역과 클릭 투과를 개발에서 검증한다.
- 좌측 이동은 전체 레이어 수평 반전. 물리 피벗 x=32 고정, 픽셀 점/이미지 경계 좌표의 반전 규약을 구분한다.
- 본체 alpha와 네이티브 입력 윤곽은 다르다. 팔·꼬리의 클릭 판정, 효과/빈 공간 투과를 실제 Windows에서 확인한다.
- r001에는 조명용 노멀/반사/발광 맵이 없다. r002의 조명 샘플은 다음 절을 따른다.
- 실제 적용 파일·날짜·리비전·Godot 재생·좌표/입력 검증 링크: 미실행.

## r002 샘플 수령·시험 계약

- 상세 [제출 명세](revisions/r002/README.md)와 JSON을 함께 수령한다. PNG 3종 모두 256×64, 4열, 셀 64×64, 피벗 (32,56), 트리밍 없음.
- 샘플 인덱스 0·1·2·3은 r001 UI 1·7·9·12에 대응한다. 단축된 move_key_samples를 실제 move 애니메이션으로 연결하지 않는다.
- 기본색에만 커버리지/반투명이 있고 normal/specular alpha는 255다. 데이터 맵은 수치 데이터로 취급한다.
- normal RGB 방향(X 오른쪽/Y 위/Z 관찰자)과 specular 회색값의 CanvasTexture 파라미터 반응은 엔진에서 확정한다. 좌우 반전 채널을 이중 보정하지 않는다.
- 시험 복사 완료: GodotProject/Assets/Diagnostics/Scp999Lighting/r002/, r003/, r004/. 제품 Sprites 경로에는 미적용.
- PLAN-005 단계 A가 광원/배경/반전·창/입력·비용 증거와 채택 설정을 제공한다. r002는 run-004, r003은 run-005 이후에 검증했다. 표의 r002 엔진 미검증 표기는 제출 당시 상태다.
- 샘플 합격 후 r004 전체 50프레임 맵을 제작·진단 전달했다. 최종 승인본의 제품 전달은 시각 확인 후 기록한다. 발광·시간대별 이미지 세트는 이번 요구에 없다.

## r003 시험 적용

- r003은 768×64·12열, idle 0~5 / move 6~11, 각 프레임 시간·피벗은 JSON을 따른다. r002의 4장 샘플 매핑을 재사용하지 않는다.
- 원본 PNG는 straight RGBA, normal/specular는 수치 데이터. Viewport 결과의 premultiplied alpha와 원본 PNG의 합성 방식을 구분한다.
- 채택한 시험 파라미터·남은 제한은 r003 제출 README에 있다. r003 최종 사용자 시각 승인과 전체 제품 적용은 미완료다.

## r004 전체 동작 전달

- GodotProject/Assets/Diagnostics/Scp999Lighting/r004/에 PNG 3종·계약 JSON·r001 비교 참조를 복사했다.
- 시트는 3200×64·50열×1행이다. r001의 8열×7행이나 r003의 12열을 적용하지 않는다. 실제 제품 연결은 r004 JSON을 읽는다.
- 10태그의 프레임별 ms·반복/일회, 피벗·반응/점액 연결점은 원래 계약을 유지한다. 미구현 게임 이벤트/물리를 진단 재생 순서로 대체하지 않는다.
- 승인 리비전 4(2026-11-24 F-006), 정식 제품 적용 없음. 제품 상태/입력/좌표 통합은 PLAN-005 후속 단계다.
