# ART-006 run-001 — 두 발 r001 제작·검수

- 2026-11-25, Windows/LG GSM5BDE, Aseprite1.3.18.6/API41 Live.
- 사용자 순차 제작 승인으로 시작, 마지막 “ART-006 r001 시각 승인”으로 마감.
- [제출본](../../../../../assets/characters/Scp0492/revisions/r001/README.md), [spec](../../../../../assets/characters/Scp0492/spec.md), [handoff](../../../../../assets/characters/Scp0492/handoff.md).

## 실행

1. [제작 Lua](build-scp0492.lua)로 좌/우 발 레이어와30프레임/7태그·PNG2개·JSON을 작성했다. 최초 dofile 실행은 저장 보안 창에서 대기했고 사용자가 직접 허용한 뒤 완료됐다.
2. 등장/이동/낙하/착지 대표 프레임을 렌더로 확인하고 [리뷰 Lua](review-scp0492.lua)로 자체 도형6종의 부착 화면을 만들었다. 실제 바탕화면 아이콘은 읽거나 저장하지 않았다.
3. [검증 Lua](verify-scp0492.lua)는 디스크의 두 Aseprite 원본을 다시 열어 PNG와 대조했다. [34개 검사](results.json) PASS.
4. [재생 Lua](show-preview.lua)로 Live UI에서10초간 전 프레임을 순환했다. [기록](playback.json)은30개 전부 관찰했다. 이후 Aseprite의 일반 반복 재생으로 사용자에게 제시했다.
5. LG 창 origin(-1883,1091),1826×943. UI 배율 때문에 zoom1이 화면2배로 보이는 것을 확인하고 zoom0.5·타임라인/보조 미리보기 숨김으로920×620 전체 보드를 화면의 실제 픽셀 크기로 표시했다.
6. 사용자 r001 시각 승인 기록 후 ART-006을 completed, approved_revision=1로 갱신했다.

## 결과와 한계

- 파일 기술 검사, 모의 배경·형태 합성, 실제 Aseprite 프레임 재생, 사용자 시각 확인 완료.
- 결과 JSON의 `userVisualApproval:false`는 기술 검사 실행 시점의 값이다. 이후 승인은 제출본 feedback과 ART 상태 전환에 기록했다.
- Godot 제품/입력/중복 클릭/캐시 수명/조명·성능은 실행하지 않았다. 관련 구현 및 인수 조건을 handoff에 남겼다.
- Lua를 재현하려면 파일 내용을 Aseprite `run_lua`의 code로 전달한다. 임의 dofile에 도구의 args가 전달된다고 가정하지 않는다. 승인된 r001 파일은 재실행으로 덮어쓰지 않는다.
