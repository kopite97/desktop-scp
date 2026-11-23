# ART-002 run-001 — 하트·음표 최초 제작

- 2026-11-24, Windows / Aseprite 1.3.18.6 API41 Live, 본체 Scp999 r004.
- [ART-002](../../../ART-002-scp-999-reaction.md), [제출 r001](../../../../../assets/vfx/Scp999Reaction/revisions/r001/README.md).
- [샘플 제작](build-samples.lua), [전체 제작](build-animation.lua), [픽셀 검사](audit.lua), [합성 리뷰 제작](build-review.lua), [본체 연결점 입력](body-frames.json), [APNG 포장/메타데이터 검사](package-review.mjs).
- 모든 Aseprite 경로를 C:/Dev/scp_desktop/Art/ 절대 경로로 지정했다. 다른 프로젝트의 기본 workDir는 사용하지 않았다.

## 실행과 결과

- Live 연결과 대상 파일 확인→32×32 정지 2장→16프레임 제작→Aseprite ExportSpriteSheet(8열/JSON_ARRAY/무트리밍/빈 끝 프레임 유지)→파일 재개방 대조.
- [픽셀 결과](results.json): 원본/출력 불일치 0, 제출 원본 불일치 0, 잘림 0, 윤곽 누락/불필요 윤곽 0, 미허용 색/알파 0. 끝 프레임 8/16은 완전 투명.
- [메타데이터·SHA256](package-result.json): native JSON과 계약의 16개 frame 사각형/시간 및 두 태그 범위/일회 재생 일치. 작업/제출 source 해시 동일, 본체 r004 해시 보존.
- [합성 결과](composite-review.json): 본체 50자세×두 효과 전체 프레임의 생성 위치에서 Face 겹침 0. 낮/밤·3배경·1배/2배 시각 확인.
- [접촉표](../../../../../assets/vfx/Scp999Reaction/revisions/r001/review/contact-sheet.png), [2배 합성](../../../../../assets/vfx/Scp999Reaction/revisions/r001/review/composite-backgrounds-2x.png), [1배 합성](../../../../../assets/vfx/Scp999Reaction/revisions/r001/review/composite-backgrounds-1x.png).
- 실제 Godot 본체 렌더에 효과를 합성한 97프레임·2600ms Aseprite 리뷰를 열어 재생했다. 효과의 원래 프레임 시간과 생성 경계를 유지한다. PlayAnimation Live 성공, native 창에서 다른 재생 자세 확인.
- [실행 기록](execution.log). 리뷰 제작 중 Lua 괄호 문법 오류는 픽셀 작업 실행 전에 발생했고 수정 후 정상 생성했다. 생성 실패를 검수 성공으로 계산하지 않았다.
- `node scripts/validate-agent-docs.mjs` 통과: 개발 PLAN active 1(005), ART active 1(002), ART-001 completed/approved_revision=4. 로컬 링크 760개 확인. 문서 검사는 이미지 품질을 대신하지 않는다.

## 한계

- Aseprite 합성/재생 검수이며 Godot VFX 통합 검수가 아니다. 실제 입력 투과·다중 창 경계·생성 상한·성능·제품 상태 전환은 PLAN-005 담당이다.
- 야간 리뷰는 PLAN-005 run-010의 실제 본체 캡처 위에 동일 색의 효과를 합성했다. 엔진 unlit/CanvasModulate 연결이 검증됐다는 뜻이 아니다.
- 효과 상승 18원본 px/850ms 및 생성 간격은 반복 생성 가독성 예시다. 제품 수치로 확정하지 않았다.
- 사용자 시각 확인 대기, ART-002 active/approved_revision=0 유지. ART-003 proposed.
