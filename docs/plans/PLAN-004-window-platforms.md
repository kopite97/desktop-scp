---
id: PLAN-004
type: development
status: completed
title: 창 발판·물리와 환경 재현
updated: 2026-11-24
---

# PLAN-004 창 발판·물리와 환경 재현

## 목표와 범위

- 목표·포함: 창 상태 관찰, 발판 생성·변경·제거, 보행·낙하·착지·인접 발판 도약, 환경 입력 재현.
- 제외: 개별 SCP AI·실제 사용자 창 흔들기.
- 요청 근거: 2026-11-24 “Plan-005를 on hold상태로 바꾸고 Plan-004를 진행해줘. 진행은 LG모니터에서” 요청에 따라 활성화한다. PLAN-003 완료 결과와 기존 물리 좌표 경계를 재사용한다.

## 기준 문서와 선행 조건

- [기획 원본](../../SCP_site-desktop.md), [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md).
- 선행: [PLAN-003](PLAN-003-overlay-validation.md).
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](../agents/plan-management.md), [관련 구현 지침](../agents/harness.md), [검증](../agents/verification.md).
- 활성화 전: 가려진 창·최대화·작업 표시줄 영역·창 이동 관성과 발판 유효성 규칙을 정한다. 실제 관찰과 모의 입력이 같은 판정 경로를 사용하게 한다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 최초 예정 run-001은 LG 미리보기 로그, 최종 자동 검사 증거는 run-007/008에 저장했다.

- 제품 규칙: [PlatformWorld](../../GodotProject/Scripts/Platforms/PlatformWorld.cs), [PlatformBody](../../GodotProject/Scripts/Platforms/PlatformBody.cs), [관찰 계약](../../GodotProject/Scripts/Platforms/WindowObservation.cs).
- Windows 관찰/선택: [WindowsWindowObserver](../../GodotProject/Scripts/Platform/Windows/WindowsWindowObserver.cs), [WindowsMonitors](../../GodotProject/Scripts/Platform/Windows/WindowsMonitors.cs), [DesktopPlacement](../../GodotProject/Scripts/Desktop/DesktopPlacement.cs).
- 실제 실행: [PlatformController](../../GodotProject/Scripts/Overlay/PlatformController.cs), [마커 창](../../GodotProject/Scripts/Overlay/PlatformBodyWindow.cs), [PlatformDesktop](../../GodotProject/Scripts/App/PlatformDesktop.cs)·[씬](../../GodotProject/Scenes/App/PlatformDesktop.tscn).
- 하네스: [PlatformSmoke](../../GodotProject/Scripts/Diagnostics/PlatformSmoke.cs)·[씬](../../GodotProject/Scenes/Diagnostics/PlatformSmoke.tscn), [재현 입력](evidence/PLAN-004/run-008/scenario-inputs.json), [결과 trace](evidence/PLAN-004/run-008/replay-trace.json).
- 작업 기록: `docs/plans/PLAN-004-window-platforms.md` 갱신.
- 검증 색인: [run-008 README](evidence/PLAN-004/run-008/README.md).
- 검증 결과: [run-008 results](evidence/PLAN-004/run-008/results.json), [headless run-007](evidence/PLAN-004/run-007/results.json).
- 실행 로그: [run-008 execution](evidence/PLAN-004/run-008/execution.log). 미리보기 run-001에는 자동 결과 파일이 없다.
- 검증 회차는 run-001부터 증가시키며 기존 증거를 덮어쓰지 않는다. 선택 자료가 미생성인 경우 이유를 기록한다.
- 여러 기능에 영향을 주는 기술 선택이 생기면 다음 미사용 TECH ID로 기록한다.

## 작업 목록

### 승인된 실행 구체화

- 화면: WMI가 확인한 LG FULL HD(GSM5BDE)를 Win32 모니터 물리 좌표에 연결해 실행·시험 창을 배치한다. 시스템 주 모니터/해상도는 변경하지 않는다.
- 관찰: EnumWindows의 앞→뒤 순서, DWM visible frame bounds·cloaked·최소화·가시성. 제목/본문은 수집하지 않는다. 자체 UI/효과·바탕화면/작업표시줄은 발판에서 제외한다.
- 판정: 작업 영역 안의 노출된 창 상단만 일방향 발판. 앞 창에 가려진 구간은 뺀다. 몸체를 둘 높이가 없는 최대화/화면 꼭대기 상단은 발판이 아니며 다른 창을 가리는 효과는 유지한다. 바닥은 작업 영역 하단이다.
- 물리 초안: 물리 px 단위, 발 크기48px·높이96px, 속도160px/s·중력1800px/s²·도약 초기속도780px/s·목표 발판 높이차160px 이하/착지 중심까지 수평거리260px 이하. 조정 가능한 프로토타입 값이며 제품 확정값이 아니다.
- 창 이동: 붙은 발판의 변위를 반영하고 수평 이동에 제한된 관성 미끄러짐을 더한다. 큰 순간 이동(200px 초과)·최소화/종료·가려짐·지지면 소실 시 지지를 풀고 낙하한다. 보행 끝에서 지지가 없어지면 낙하한다.
- 시간: 최대 delta100ms, 120Hz 이하 소분할로 상단 통과 충돌을 검사한다. 숨김 중 시간/관찰 반영을 중단하고 복원 시 최신 snapshot부터 지지 상태를 재판정한다. 모니터 변경은 사용 가능한 작업 영역으로 복구한다.
- 예정 제품 파일: `Scripts/Platforms/WindowObservation.cs`, `PlatformWorld.cs`, `PlatformBody.cs`, `Scripts/Platform/Windows/WindowsWindowObserver.cs`, `WindowsMonitors.cs`, `Scripts/Overlay/PlatformController.cs`, `PlatformBodyWindow.cs`, `Scripts/App/PlatformDesktop.cs` 및 해당 씬. 관찰·규칙·렌더를 분리한다.
- 예정 하네스: `Scripts/Diagnostics/PlatformSmoke.cs` 및 씬, 별도 소유 시험 창. `godot.ps1 PlatformSmoke/PlatformPreview`·MonitorId 옵션. 실제 사용자 창을 움직이지 않으며 소유한 시험 창만 이동/최소화/최대화/종료한다.
- PLAN-005는 보류로 유지한다. 이 PLAN은 공통 물리와 단독 실행 경로까지 구현하며 SCP-999의 기존 AI/점액 부착 최종 통합은 재개 시 수행한다.

- [x] 실행 요청·선행 결과 확인, 미정 규칙·제품 파일 경로 확정
- [x] 창 관찰·가려짐·일방향 발판·보행/낙하/착지/도약·관성 구현
- [x] LG 선택·실제 물리 실행 창·같은 경로의 재현 하네스 구현
- [x] 모의/실제 창·표시·재현 입력·수명 검증과 실패/한계 기록

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 발판이 창 상태와 함께 갱신됨 | 모의 이동·최소화·종료 입력과 실제 테스트 창에서 결과 비교 | [native54 결과](evidence/PLAN-004/run-008/results.json)·[관찰](evidence/PLAN-004/run-008/fixture-observations.json) |
| 보행·낙하·착지·도약이 규칙을 지킴 | 동일 게임 갱신 경로에 환경·시간 입력, 실제 표시 연결 확인 | [상태 trace](evidence/PLAN-004/run-008/native-trace.json)·[실제 화면](evidence/PLAN-004/run-008/native-platforms.png) |
| 실행 조건과 실패 상태를 재현할 수 있음 | 입력 순서·초기 상태·진행량·결과 기록 후 해당 시나리오 재실행 | [재현 입력](evidence/PLAN-004/run-008/scenario-inputs.json)·[일치 trace](evidence/PLAN-004/run-008/replay-trace.json)·[실패 이력](evidence/PLAN-004/run-008/README.md) |

## 현재 상황과 다음 행동

- 현재: completed. 공통 창 발판/물리·단독 실행·재현 하네스를 구현했고 LG FULL HD에서 실제 창·화면 결과를 확인했다.
- 다음 행동: 사용자가 PLAN-005 재개를 요청하면 이 공통 물리에 SCP-999 표현·제스처·점액 부착을 연결한다. PLAN-005는 on hold로 유지했으며 자동 재개하지 않았다.
- 단독 확인: `./scripts/godot.ps1 -Action PlatformPreview -MonitorId GSM5BDE`. 오른쪽 드래그 후 놓기=낙하, 왼쪽 클릭=가능한 인접 발판 도약, 제어 창 보행 버튼=좌우 이동/자동 도약.

## 검증 결과

- [최종 검증](evidence/PLAN-004/run-008/README.md): headless36/native54 통과, C# 경고/오류0·Import 성공·기존 Main headless 회귀 통과.
- 실제 LG FULL HD:1920×1080·원점(-1920,1061)·작업 영역1920×1032·DPI96. 실제 창 이동/최소화/최대화/종료/가려짐과 도약·자동 관찰 갱신·화면 픽셀을 확인했다. 주 화면 설정과 사용자 창은 변경하지 않았다.
- 도약 적분 시점 오류, native fixture 초기 가시성 오류, 마커가 배경에 가려진 문제를 수정했다. 실패 run-002/004와 중간 run-005의 시각 문제를 보존했다.
- 사각 DWM 가림 근사·GetWindowRect fallback·poll 사이 변화와 HWND 재사용 한계, 실제 혼합 DPI/가상 데스크톱/장기 비용 미검증은 증거에 명시했다. 범용 물리 마커를 SCP-999 최종 통합으로 보고하지 않는다.
- 마감 문서 검사 `node scripts/validate-agent-docs.mjs` 통과: 개발 active0·ART active1·TECH9, 목록·상태·로컬 링크 일치. `git diff --check` 통과. PLAN-005는 on hold 유지.

## 관련 결정과 상태 전환

- 관련 기술 결정: [TECH-009](../decisions/TECH-009-window-platform-physics.md).
- 2026-11-23: proposed 생성. 후속 작업을 proposed로 작성하라는 사용자 지시를 따른다.
- 2026-11-24: PLAN-005 보류 및 PLAN-004 실행·LG 화면 사용 지시에 따라 proposed → active.
- 2026-11-24: 공통 구현·LG native54/headless36·재현 및 필수 검증 증거를 충족해 active → completed. PLAN-005 보류 유지.

