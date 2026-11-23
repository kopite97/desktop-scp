# Scp999 r003 — idle/move 전체 조명 맵

- 2026-11-24, [ART-001](../../../../../docs/plans/ART-001-scp-999-character.md), 요청 PLAN-005-SPR-001. 최종 시각 미승인.
- 사용자 지정 순서에 따라 r002 4장 검증 후 idle 6장·move 6장으로 확장했다. 다른 동작 38장은 r001 참조만 있고 조명 맵은 아직 없다.
- [편집 원본](../../source/Scp999-lighting-idle-move.aseprite), [제출 사본](source/Scp999-lighting-idle-move.aseprite). Aseprite 1.3.18.6 Live/Lua 제작·출력.

## 출력·재생 계약

- [기본색](export/Scp999_color.png), [노멀](export/Scp999_normal.png), [반사](export/Scp999_specular.png), [계약 JSON](export/Scp999.contract.json).
- RGB/sRGB 원본 64×64, PNG RGBA 768×64·12열×1행, 간격/패딩/트리밍/회전 없음. 출력 인덱스 0~11 = r001 UI 1~12.
- idle(UI 1~6, 출력 0~5): 220/180/100/160/180/200ms, 1040ms 반복.
- move(UI 7~12, 출력 6~11): 120/120/100/100/140/160ms, 740ms 반복.
- 피벗 (32,56), 원점 좌상단·X 오른쪽/Y 아래. 바닥 y=56 유지. 실제 이동은 Godot 담당이며 상하 바운스를 추가하지 않는다.
- BodyColor·Outline·Face만 기본색에 표시한다. NormalData·SpecularData·ReferenceR001은 숨김 데이터/참조다.
- BodyColor #F78A21D3, 표정·1px 윤곽·팔·끌림은 r001 보존. 방향 고정 명암·재질 Highlights 제외.
- normal: 각 프레임 실루엣 거리 높이장의 평활 기울기, RGB X+/Y+/Z+, 바깥 (128,128,255), alpha 255.
- specular: RGB 동일값 180(몸체) / 0(표정·윤곽·바깥), alpha 255. 기본색만 커버리지/반투명을 소유한다.
- 데이터 맵은 색 보정·팔레트 양자화하지 않는다. 6색 편집 팔레트는 데이터 색 제한이 아니다.

## 확인한 Godot 연결

- PLAN-005 단계 A, Godot 4.7.2 .NET / Compatibility / NVIDIA RTX 5070.
- PNG를 lossless ImageTexture로 읽고 CanvasTexture에 연결, nearest·mipmap 없음·repeat 비활성. Sprite2D.FlipH를 사용하고 수동 법선 채널 반전은 하지 않는다.
- 주광원 하나, 높이 100, 낮 energy .72·주변 밝기 .42. 밤 energy .35·주변 RGB (.40,.40,.48). 실제 로컬 시각 연동은 없다.
- specular_shininess .45, specular_color RGB .35. 검정 맵 부위의 반사 억제와 반사 ON/OFF 차이를 실제 렌더에서 확인했다. .45는 일반 3D Blinn 지수 24 같은 값으로 해석하지 않는다.
- 투명 SubViewport 결과는 premultiplied alpha다. 화면 합성은 PremultAlpha, PNG 증거는 한 번만 straight RGBA로 복원한다. 원본 PNG alpha를 이중 적용하지 않는다.
- 위 설정은 진단 환경의 후보이며 제품 전역 조명/시간대 수치 확정은 아니다.

## 검수·전달

- [Art 검사](../../../../../docs/plans/evidence/ART-001/run-003/results.json): 12프레임 3맵 원본/출력 일치·표정/윤곽·커버리지·노멀·반사값·잘림 검사 통과.
- [원본 맵 비교](review/map-comparison.png), [기본색 배경 비교](review/base-backgrounds.png), [피드백](review/feedback.md).
- [Godot 검증](../../../../../../docs/plans/evidence/PLAN-005/run-007/README.md): 실제 광원/반전/재생과 입력/비용 증거 색인.
- 정식 제품 미적용. 진단 경로 GodotProject/Assets/Diagnostics/Scp999Lighting/r003/에 복사했다. 현재 수령/적용은 [handoff](../../handoff.md)에 기록한다.
- 최종 시각 승인 리비전 0 유지. 기술 검사가 사용자 시각 승인을 대신하지 않는다.
