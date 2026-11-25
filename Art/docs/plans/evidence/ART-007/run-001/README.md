# ART-007 run-001 — 치료·변이 오라 r001

- 2026-11-25, Windows/LG GSM5BDE, Aseprite1.3.18.6/API41 Live.
- ART-006 r001의 사용자 시각 승인 이후, 기존 순차 실행 승인에 따라 제작했다.
- [제출본](../../../../../assets/vfx/Scp049Mutation/revisions/r001/README.md), [spec](../../../../../assets/vfx/Scp049Mutation/spec.md), [handoff](../../../../../assets/vfx/Scp049Mutation/handoff.md).

## 실행·증거

1. [제작 Lua](build-mutation.lua)의 sample 모드로 치료·변이4개 대표 그림을 만들고 렌더로 확인했다. 24×24 중앙을 비운 얇은 보라 호·초록 입자이며 독립 발/몸체는 없다.
2. treatment의 반경 맥동을 다듬고16프레임/2태그로 확장했다. 두 원본과 PNG/JSON을 저장했다.
3. [리뷰 Lua](review-mutation.lua)로 ART-006에서 만든 자체 도형과 승인된 발을 합성했다. 실제 사용자 아이콘을 읽거나 저장하지 않았다. 작업 영역 마스크는 몸체와 발이 영역 안에 있는 예시로 조정했다.
4. [검증 Lua](verify-mutation.lua)가 원본을 재개방해 PNG·시간·태그·투명 중앙·경계·여섯 로고를 검사했다. [23개 검사](results.json) PASS. 모의 사건의 정확한 시간은 [타임라인](mock-timeline.json)에 남겼다.
5. [재생 Lua](show-preview.lua)로 LG Aseprite에서 모의 타임라인을12초간 순환 관찰 후 일반 반복 재생으로 이어갔다. [재생 결과](playback.json)에86프레임 전부 관찰을 기록했다.
6. 사용자 “ART-007 r001 시각 승인”을 기록하고 ART를 completed/approved_revision1로 마감했다.

## 결과·한계

- 소스16프레임, 검수86프레임/9.72초. 같은120ms treat를 일반3초/과부하2초 동안 재생하고 성공 시 transform800ms와 발 spawn을 함께 시작한다.
- 취소 구간은 효과가 즉시 없어지고 발/transform이 생기지 않는다. 재시작은 treat 프레임1, 삭제는 표시 전체 제거를 보여준다.
- 위 사건·경계 마스크는 아트 설명용 모의 구현이다. 실제 Godot 치료/입력/위치/창 순서/봉쇄/자원 정리는 검증하지 않았다.
- r001 시각 승인 완료. results.json의 userVisualApproval:false는 기술 검사 당시 값이며, 이후 승인 사실은 리비전 feedback/ART 상태 전환에 기록했다.
- 마감 중 열린 작업용 소스 탭에 modified 표시가 있어 읽기 전용으로 비교했으며,16개 합성 프레임은 제출 PNG와 전부 일치했다. 검증된 저장 파일을 전달 기준으로 삼고 제출본을 재저장하지 않았다.
- 재현은 Lua 파일 내용을 run_lua code로 전달한다. 제출 후 변경은 신규 리비전으로 보존하며 승인본을 재실행으로 덮어쓰지 않는다.
