# ART-001 run-001 제작·검사 기록

- 날짜: 2026-11-23. [ART-001](../../../ART-001-scp-999-character.md) 실행.
- 사용자 지시: ART-001부터 실행, 실행 중인 Aseprite에 스프라이트를 열어 진행 상황 표시.
- 수정 지시: 점액 팔 추가, 이동 시 뒤쪽 몸체가 질질 끌리는 표현.
- 도구: Windows·Aseprite 1.3.18.6/API 41·MCP Live.
- 최초 status는 Live disconnected/extensionInstalled false였다. launch_aseprite로 Bridge 설치·연결 후 sprites 0을 확인하고 새 Scp999를 열었다.
- 기존에 열려 있던 별도 Aseprite 창은 종료하지 않았다. 현재 source 창에서 Live 편집했다.

## 제작

- [제작 Lua](build-sprite.lua): Aseprite 안에서 RGBA 픽셀·레이어·태그·시간을 생성. 최초 문법 오류를 수정한 뒤 실행했다.
- 초기 idle/move에서 실루엣·normal 합성·표정을 검토하고 사용자 요청으로 팔과 접지 이동을 추가했다.
- 프레임 추가 시 태그 끝 범위가 늘어나는 문제를 확인해 전체 프레임 생성 후 태그를 설정하도록 수정했다.
- 도구 호출 후 12칸 팔레트를 설정하고 편집 source를 저장했다. 팔레트 설정은 별도 Live 호출로 수행했다.
- [r001 제출본](../../../../../assets/characters/Scp999/revisions/r001/README.md), [현재 명세](../../../../../assets/characters/Scp999/spec.md).
- 생성 결과: 64×64·50프레임·4레이어·10태그. 움직임 외형은 Aseprite에서 생성, 리뷰 포장은 Node에서 수행했다.

## 실행 검증

- Aseprite Image:drawSprite 결과와 출력 PNG의 셀을 50×64×64 픽셀 대조: 불일치 0.
- 4방향 실루엣 경계에 불투명 윤곽 존재 검사: 누락 0.
- 모든 프레임 반투명 픽셀 존재, 캔버스 가장자리 접촉 없음.
- move의 6프레임은 하단 y=55 고정, 몸체 너비 변화·후방 늘어짐 확인.
- [픽셀 원자료](pixel-audit.json), [검사/리뷰 포장 스크립트](package-review.mjs).
- 명령: node Art/docs/plans/evidence/ART-001/run-001/package-review.mjs.
- [결과](results.json), [실행 로그](execution.log): PASS. 사용자 승인·Godot 검증은 별도 미실행으로 기록.
- 접촉표·밝은/어두운/무늬 배경·1배/2배·좌우 반전 비교를 시각 검토했다.
- Aseprite Live에서 PlayAnimation 실행 후 창을 전면에 표시했다. 서로 다른 시점의 화면과 activeFrame 변화로 실제 재생을 확인했다.
- 빠른 전체 재생의 모든 상태 전환을 개별 시나리오로 검증한 것은 아니다. 개별 APNG는 정확한 프레임별 시간·일회/반복으로 제공한다.
- 제품상의 이동 속도·이벤트 동기화·태그 전환·클릭 투과는 PLAN-005에서 확인해야 한다.

## 상태와 한계

- ART-001은 active, 사용자 최종 시각 확인 대기. approved_revision=0.
- ART-002/003은 미착수. PLAN-005는 proposed 유지, 제품 복사/실행 없음.
- 그림 수정 제출은 기존 r001을 보존하고 r002로 진행한다.
