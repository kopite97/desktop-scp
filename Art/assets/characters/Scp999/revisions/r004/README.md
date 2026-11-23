# Scp999 r004 — 전체 50프레임 조명 리소스

- 2026-11-24, [ART-001](../../../../../docs/plans/ART-001-scp-999-character.md), 요청 PLAN-005-SPR-001.
- r002 4장과 r003 idle/move 검증 뒤 사용자가 요청한 순서로 나머지 38장을 확장했다. 이전 제출본은 보존했다.
- [편집 원본](../../source/Scp999-lighting.aseprite), [제출 사본](source/Scp999-lighting.aseprite). Aseprite 1.3.18.6 Live/Lua.

## 출력과 대응

- [기본색](export/Scp999_color.png), [노멀](export/Scp999_normal.png), [반사](export/Scp999_specular.png), [계약 JSON](export/Scp999.contract.json).
- 캔버스 64×64·RGB/sRGB. PNG 3종 모두 3200×64·50열×1행, RGBA 1배 출력. 여백/간격/트리밍/회전/중복 병합 없음.
- 출력 인덱스 0~49 = r001 UI 1~50. r001 시트의 8열×7행과 배열이 다르므로 r004 JSON의 frame 사각형을 사용한다.
- 모든 피벗 (32,56), 좌상단 원점·X 오른쪽/Y 아래. 프레임별 reactionAnchor와 trailAnchor를 r001 계약에서 복사했다.

| 태그 | 출력 인덱스 | 재생 |
| --- | --- | --- |
| idle | 0~5 | 반복 1040ms |
| move | 6~11 | 반복 740ms |
| typing_react | 12~18 | 일회 |
| pet | 19~24 | 입력 중 반복 |
| pet_recover | 25~27 | 일회 |
| takeoff | 28~31 | 일회 |
| fall | 32~35 | 체공 중 반복/유지 |
| land | 36~40 | 일회 |
| balance | 41~46 | 반복 |
| balance_recover | 47~49 | 일회 |

각 프레임 ms는 JSON에 명시했다. r001의 불균일 시간을 그대로 보존했으며 제품의 실제 이동·물리 타이밍을 그림에 굽지 않았다.

## 재질 계약

- BodyColor #F78A21D3, Outline·Face는 r001 픽셀 보존. 고정 방향 Body 명암과 Highlights 제외. 점액 팔·끌림·표정·동작은 유지한다.
- 표시 레이어 BodyColor/Outline/Face, 숨김 데이터 NormalData/SpecularData/ReferenceR001. 원본 팔레트는 편집 편의용이며 데이터 색 제한이 아니다.
- normal은 각 자세의 실루엣 높이장 기울기 기반, RGB X+/Y+/Z+, 바깥 (128,128,255), alpha 255. 밝기 사진 변환이 아니다.
- specular RGB 180(몸체)/0(윤곽·표정·바깥), alpha 255. 기본색만 alpha 커버리지를 소유한다. 데이터 색 보정·양자화 금지.
- 검증한 시험 연결: CanvasTexture, lossless ImageTexture, nearest·mipmap 없음·repeat 끄기, Sprite2D.FlipH만 사용. shininess .45·specular_color .35.
- 원본 PNG는 straight RGBA, 투명 Viewport 결과는 premultiplied RGBA. 후자를 화면에 표시할 때 PremultAlpha를 사용한다.
- 발광·접지/투사 그림자 이미지·시간대별 세트는 포함하지 않는다. 실제 광원·시간 연동·그림자 합성은 Godot 개발 담당이다.

## 검수와 전달

- [Art 검사](../../../../../docs/plans/evidence/ART-001/run-004/README.md): 50프레임 원본/출력·형태·태그·시간·데이터 검사 통과.
- [맵 비교](review/map-comparison.png), [기본색 배경 비교](review/base-backgrounds.png), [피드백](review/feedback.md).
- [Godot 전체 동작 검증](../../../../../../docs/plans/evidence/PLAN-005/run-010/README.md), 비용·입력의 선행 시험 한계도 해당 문서에 연결한다.
- 진단 복사: GodotProject/Assets/Diagnostics/Scp999Lighting/r004/. 정식 제품 미적용, 사용자 최종 시각 미승인. [handoff](../../handoff.md)에서 현재 적용 상태를 관리한다.
