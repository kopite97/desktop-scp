# Scp999Reaction r001 — 하트·음표 반응 효과

- 2026-11-24, [ART-002](../../../../../docs/plans/ART-002-scp-999-reaction.md), PLAN-005-SPR-002. 최초 제출, 본체 참조 Scp999 r004.
- Aseprite 1.3.18.6/API 41 Live. [편집 원본](../../source/Scp999Reaction.aseprite), [제출 원본](source/Scp999Reaction.aseprite).
- [PNG](export/Scp999Reaction.png), [Aseprite JSON](export/Scp999Reaction.json), [계약 JSON](export/Scp999Reaction.contract.json).
- 32×32 셀·RGBA/sRGB·straight alpha, 256×64·8열×2행, 무패딩/무간격. 트리밍·회전·중복 병합·빈 프레임 제거 없음.
- HeartFill/HeartOutline, NoteFill/NoteOutline 출력, Guide 숨김. 본체·배경은 제품 시트에서 제외한다.
- 1px 윤곽, 분홍 하트/청록 음표, RGB 6색+투명. 최대 그림 17×15/15×19. 피벗(16,16), 좌상단 원점·X 오른쪽/Y 아래.

| 태그 | JSON / Aseprite | 프레임별 ms | 알파 | 반복 |
| --- | --- | --- | --- | --- |
| heart | 0~7 / 1~8 | 70,80,100,160,160,120,100,60 | 160,230,255,255,230,150,80,0 | 일회 850ms |
| note | 8~15 / 9~16 | 동일 | 동일 | 일회 850ms |

- unlit 기호, 그래픽 색 강조만 포함. 노멀/반사/발광/그림자 맵 없음. 밤의 식별성을 위해 본체 조명과 분리한다.
- 이미지만 형태/알파를 제어한다. Godot 추가 fade 없음. 상승/스폰/이벤트 병합/정리는 개발 담당.
- reactionAnchor의 생성 순간 좌표에 중심 오프셋 하트(-14,-10), 음표(14,-10). 화면 정방향이며 본체 반전을 상속하지 않는다. 상세 [명세](../../spec.md), [handoff](../../handoff.md).

## 검수

- [기술 검사](../../../../../docs/plans/evidence/ART-002/run-001/README.md): 원본/제출/PNG 픽셀 불일치 0, 윤곽 누락·잘림·미허용 알파/색 0, 두 태그 끝 완전 투명. JSON 시간/범위 일치.
- [접촉표](review/contact-sheet.png), [1배 낮/밤·세 배경](review/composite-backgrounds-1x.png), [2배 비교](review/composite-backgrounds-2x.png).
- [하트 일회 재생](review/heart.apng), [음표 일회 재생](review/note.apng), [연속 생성 합성](review/composite-preview.apng), [Aseprite 재생 파일](review/composite-preview.aseprite).
- 합성은 실제 Godot 본체 캡처와 Aseprite 효과를 조합한 리뷰다. 상승·생성 간격은 예시이며 제품 구현 결과가 아니다. 본체는 타이핑 태그를 리뷰용으로 반복한다.
- APNG는 Aseprite 렌더 프레임의 PNG 데이터와 ms를 그대로 묶었다. 합성은 97프레임·2600ms, 원본 태그 경계와 시간 유지.
- [피드백](review/feedback.md): 최종 사용자 확인 대기. 정식 제품 미적용.
