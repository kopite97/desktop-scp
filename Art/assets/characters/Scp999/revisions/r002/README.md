# Scp999 r002 — 조명용 대표 샘플

- 제작: [ART-001](../../../../../docs/plans/ART-001-scp-999-character.md), 2026-11-23. 최종 사용자 시각 미승인, Godot 미적용.
- 기준: [r001](../r001/README.md)의 형태·점액 팔·끌림 동작을 보존하고 고정 명암을 제거했다.
- 목적: 동적 조명 검증용 idle 1장·move 3장. 전체 50프레임이나 완전한 애니메이션 교체본이 아니다.
- 시험 담당: [PLAN-009](../../../../../../docs/plans/PLAN-009-scp-999-lighting-sample.md), proposed·미실행.
- 도구: Aseprite 1.3.18.6 Live/Lua, Image PNG 출력. 작업 원본은 [source/Scp999-lighting-sample.aseprite](../../source/Scp999-lighting-sample.aseprite)다.

## 파일과 규격

| 파일 | 역할 |
| --- | --- |
| [source/Scp999-lighting-sample.aseprite](source/Scp999-lighting-sample.aseprite) | 제출 원본, 64×64·RGB/sRGB·4프레임·6레이어 |
| [Scp999_color.png](export/Scp999_color.png) | 기본색, 반투명 몸체·윤곽·표정 |
| [Scp999_normal.png](export/Scp999_normal.png) | 표면 방향 후보 데이터 |
| [Scp999_specular.png](export/Scp999_specular.png) | CanvasTexture 스페큘러/광택 입력 후보 |
| [Scp999.contract.json](export/Scp999.contract.json) | 셀·순서·시간·피벗·채널 의미 |
| [map-comparison.png](review/map-comparison.png) | 위부터 r001 / 중립 기본색 / 노멀 / 반사 맵, 열은 샘플 1~4, nearest 2배 |
| [base-backgrounds.png](review/base-backgrounds.png) | idle·최대 끌림의 밝음/어두움/무늬 배경, nearest 2배 |
| [r001-reference.png](review/r001-reference.png) | 동일한 원본 4장, 엔진 비교용 참조 시트 |
| [feedback.md](review/feedback.md) | 수정 근거와 사용자 확인 상태 |

- PNG 3종은 모두 256×64·RGBA, 4열×1행·64×64 고정 셀, 여백/간격 0. 트리밍·회전·프레임 병합 없음.
- 샘플 UI 1·2·3·4 / 출력 인덱스 0·1·2·3 → r001 UI 1·7·9·12 / 인덱스 0·6·8·11.
- 프레임 시간 220·120·100·160ms, 모든 피벗 (32,56). 원점 좌상단, X 오른쪽/Y 아래.
- 태그 idle_sample(UI 1), move_key_samples(UI 2~4·1회). 생략된 프레임 시간을 포함하지 않으므로 완전한 루프가 아니다.
- APNG는 만들지 않았다. 이 샘플은 프레임별 재질 비교용이며 실제 전체 동작 재생은 r001 리뷰를 참조한다.

## 재질과 편집 계약

- 표시 레이어 BodyColor·Outline·Face, 숨김 레이어 NormalData·SpecularData·ReferenceR001.
- BodyColor #F78A21D3(알파 211). r001 Body의 방향 명암과 Highlights를 제거했다. Outline·Face 픽셀은 그대로다. 눈의 밝은 점은 표정 그래픽으로 유지했다.
- 기본색은 표시용 색 데이터다. 기본색만 alpha로 커버리지·반투명을 정한다. 몸체가 평평하게 보이는 것은 중립화 결과이며 최종 조명 품질을 뜻하지 않는다.
- 노멀은 기존 이미지 밝기 변환이 아니라 실루엣 거리 기반 높이장의 평활 기울기다. 둥근 몸체·팔·꼬리의 초기 근사이며 엔진 조명 후 수정할 수 있다.
- normal RGB는 X 오른쪽/Y 위/Z 관찰자 방향, [-1,1]→[0,255], 외부 (128,128,255), 전체 alpha 255다.
- specular RGB는 동일 회색값, 몸체 180·윤곽/표정/외부 0, alpha 255. 반투명 마스크가 아니다. 값과 CanvasTexture shininess/색 파라미터의 실제 반사 폭·밝기 관계는 아직 미검증이다.
- normal/specular는 수치 데이터이며 표시색 보정·팔레트 양자화 대상이 아니다. 6칸 편집 팔레트가 데이터 색을 제한하지 않는다.
- Godot의 필터 nearest·정수 배율·repeat 끄기를 시험 기준으로 한다. 색 공간 처리·경계·좌우 반전 법선 처리는 PLAN-009에서 확정한다. 미확인 수동 채널 반전을 겹쳐 적용하지 않는다.
- 발광·접지 그림자·시간대별 색상 시트는 포함하지 않는다. 시간·광원·그림자 합성은 개발 담당이다.

## 검증과 남은 작업

- [run-002](../../../../../docs/plans/evidence/ART-001/run-002/README.md)의 파일 검사 통과: 원본/출력 불일치·윤곽/표정 변경·커버리지 변경·잘림·비균일 몸체·잘못된 노멀/반사 값 0.
- 비교 시트에서 고정 몸체 하이라이트 제거와 팔·꼬리 보존을 확인했다. 사용자 시각 승인과 실제 조명 품질 확인은 별개다.
- 엔진의 좌/우/위·낮/밤·반전·투명 창·입력·성능은 미실행이다. 샘플 시험 통과 후 전체 50프레임을 새 리비전에 확장한다.
- r001 제출물과 전체 동작 작업 원본은 수정하지 않았다. r002 편집 원본과 제출 사본의 해시는 검사 기록에 남겼다.
