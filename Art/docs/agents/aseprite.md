# Aseprite 운영

적용: 도구 상태·원본 편집·저장·출력. 탐색: [리비전](revisions.md), [전달](handoff.md).

## 현재 확인된 환경

2026-11-23 status 응답은 [기록](../evidence/harness-2026-11-23/aseprite-status.json)에 보존했다.

| 항목 | 실제 응답 |
| --- | --- |
| 실행 파일 | C:/Program Files (x86)/Steam/steamapps/common/Aseprite/Aseprite.exe |
| Aseprite / Lua API | 1.3.18.6 / 41 |
| MCP runtime | 0.1.1 |
| batch | 정상 응답 |
| Live 연결 | disconnected |
| Bridge extensionInstalled | false |
| MCP 기본 workDir | C:/Dev/Unity/Art |

참고 Art 문서의 과거 Live 연결 성공 기록을 현재 연결 상태로 취급하지 않는다.
실행 직전 status를 다시 확인한다. batch 응답 성공은 Live 편집 성공 증거가 아니다.

## 이 프로젝트의 경로

- MCP의 기본 workDir은 다른 프로젝트를 가리킨다. 모든 입력·출력 경로에 `C:/Dev/scp_desktop/Art/...` 절대 경로를 명시한다.
- 기본 작업 디렉터리에 상대 경로로 저장하거나 다른 프로젝트의 활성 Sprite를 암묵적으로 선택하지 않는다.
- 전역 MCP 설정·다른 프로젝트의 기본 경로는 이 하네스 설치 과정에서 변경하지 않았다.
- 도구나 Lua 코드에서 대상 파일·저장 경로를 검토한다. 현재 프로젝트 경로를 빠뜨린 상태로 새 파일을 생성하지 않는다.

## batch와 live

- batch 작업은 `backend: batch`와 원본 경로를 명시한다. 호출 간 선택·undo·미저장 상태가 유지되지 않는다.
- live는 실제 연결이 확인된 경우 `backend: live`를 명시하고 app_state로 열린 파일·레이어·프레임을 확인한다.
- auto가 batch로 전환되는 것을 Live 검증으로 기록하지 않는다.
- Live가 필요할 때 연결된 도구의 안내로 Bridge 설치·연결을 진행한다. 사용자가 작업 중인 Aseprite를 강제 종료하지 않는다.
- 연결 문제를 해결하려고 다른 세션이나 다른 프로젝트의 서버 설정을 임의 변경하지 않는다.

## 편집 순서

1. 요청·PLAN의 대상 원본과 리비전을 확인한다.
2. get_sprite_info로 크기·모드·레이어·프레임·태그를 읽고 수정할 대상을 명시한다.
3. 새 제작은 작업용 source에 저장한다. 수정은 제출 원본 보존 후 작업용 파일에서 진행한다.
4. 실제 제공되는 도구 서명을 확인해 픽셀·레이어·프레임을 편집한다. 외부 스킬의 도구 이름을 현재 API로 가정하지 않는다.
5. render_preview 등으로 실제 결과를 관찰하고 파일을 다시 읽어 저장 상태를 확인한다.
6. source를 보존한 채 해당 리비전 export에 PNG와 필요한 메타데이터를 출력한다.

색상 모드·프로파일·팔레트·레이어 가시성을 기록한다. 원본의 임의 평탄화·재색칠·리사이즈로 제출 규격을 맞추지 않는다.
편집 가능한 .aseprite를 유일한 PNG로 대체하지 않는다. 작업 중 파일과 제출 보존본의 덮어쓰기를 구분한다.
