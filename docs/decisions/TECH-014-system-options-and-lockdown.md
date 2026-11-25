---
id: TECH-014
status: accepted
title: 시스템 지표 관찰과 단일 봉쇄·외부 효과 소유자
updated: 2026-11-25
---

# TECH-014 시스템 지표 관찰과 단일 봉쇄·외부 효과 소유자

근거: [PLAN-008](../plans/PLAN-008-system-options.md), [제품 규칙](../game-design/system-reactions.md).

## 문제와 제약

여러 입력 경로가 별도 봉쇄 상태를 유지하면 표시만 숨기고 게임 시간이나 외부 창 효과가 계속될 수 있다. 지표 관찰은 UI를 막지 않아야 하며 사용자 창 이동과 취소·복원을 구분해야 한다.

## 결정

- Main 소유 [SystemController](../../GodotProject/Scripts/App/SystemController.cs)가 [SystemOptions](../../GodotProject/Scripts/Simulation/SystemOptions.cs)의 유일한 소유자다. UI·트레이·글로벌 단축키 모두 ToggleLockdown/SetLocked를 호출한다.
- [WindowsSystemMetrics](../../GodotProject/Scripts/Platform/Windows/WindowsSystemMetrics.cs)는 GetSystemTimes·GlobalMemoryStatusEx·Core Audio 읽기 전용 호출을 UI 밖의 단일 작업에서 수행한다. 결과는 세대 번호를 검사한 뒤 메인 스레드에서만 반영한다. COM 포인터는 수집 작업의 finally에서 해제한다.
- [LockdownHotkey](../../GodotProject/Scripts/Platform/Windows/LockdownHotkey.cs)는 별도 메시지 스레드에서 MOD_NOREPEAT로 등록하고 발생 횟수만 전달한다. 키 문자열/임의 입력은 수집하지 않는다. 종료 시 WM_QUIT·UnregisterHotKey·Join을 수행한다.
- [SiteTray](../../GodotProject/Scripts/App/SiteTray.cs)는 설치된 Godot의 DisplayServer status indicator와 NativeMenu를 사용한다. Win32 트레이 복제 구현 대신 엔진의 메뉴 수명을 이용한다.
- Godot는 메인 Window.Hide를 거부한다. 트레이 대기는 WindowsDesktop.SetControlVisible의 ShowWindow로 처리하고 엔진 이벤트 루프는 계속 실행한다. 복귀 역시 같은 OS 경로를 사용한다.
- PLAN-015: Main이 제어창 표시·숨김을 소유하고 X/Alt+F4와 Hide controls를 같은 명령에 연결한다. SystemController는 봉쇄 상태만 변경하며 재개 시 제어창을 자동으로 열지 않는다. 종료는 기존 RequestExit로 통합한다.
- 엔진 부트 구간은 투명·테두리 없음·no_focus의 1px 요청으로 시작한다. Windows/Godot가 빈 표면을64px로 보정할 수 있다. [WindowStartup](../../GodotProject/Scripts/App/WindowStartup.cs)이 네이티브로 숨기며 정상 크기·테두리는 ShowControls 때만 복구한다. 숨긴 직후 플래그를 바꾸면 엔진이 창을 다시 표시하므로 이를 피한다. 트레이 실패와 독립 진단은 제어창을 복구한다.
- 트레이 등록 실패는 메뉴/아이콘 자원을 정리하고 제어창·Exit 버튼으로 복구한다. 성공 후 숨김과 실패 후 접근 경로를 실제 공통 코드 및 실패 주입으로 검증한다.
- [WindowsWindowShake](../../GodotProject/Scripts/Platform/Windows/WindowsWindowShake.cs)는 요청 시점과 각 이동 직전 허용 여부를 확인한다. 원래 사각형·마지막 적용 사각형·PID·스레드를 보유하고 사용자 이동을 우선한다.
- [SystemCueWindow](../../GodotProject/Scripts/Overlay/SystemCueWindow.cs)는 장식용 layered/transparent 창이며 기존 단일 OverlayStackCoordinator에 등록한다. 입력 영역을 추가하지 않는다.

## 대안과 선택 이유

- WMI 장기 쿼리·오디오 캡처보다 작은 집계/설정 API를 사용한다. CPU/RAM 표본은 게임 속도 반응용이며 정밀 성능 분석값이 아니다.
- 지표 작업은 하나만 허용해 지연 시 작업이 누적되지 않는다. OS 오디오 드라이버가 영구 정지하는 극단 조건은 강제 중단하지 않으며 5초 후 반응을 해제한다. UI는 기다리지 않는다. Shell 확장 격리와는 다른 경계다.
- 외부 창 자체가 사용자 이동을 처리하는 동안은 복원하지 않는다. 앱 비정상 강제 종료 후 창 위치 복원은 보장하지 않으며 정상 종료만 이번 범위다.
- 트레이 메뉴의 OS 생성/항목과 등록 콜백, 별도 프로세스에서 실제 글로벌 단축키를 검증한다. 자동 메뉴 클릭 시험과 수동 청감 평가는 별도다.

## 영향과 검증

- Main·세 개체·아이콘 서비스가 공통 봉쇄 경계를 사용하며 기존 개체의 상태 소유자는 유지한다. 새로운 세션에서 강한 연출은 항상 OFF다.
- [PLAN-008 실행 색인](../plans/evidence/PLAN-008/run-001/README.md): 실제 지표, 모의 임계값, LG 창 이동/취소/복원·글로벌 단축키·클릭 수신, 종료 및 재등록을 검증했다. 세부 미검증 조건은 해당 기록을 따른다.
- [PLAN-015](../plans/PLAN-015-tray-residency.md)는 트레이 상주·실제 EXE를 후속 검증한다. 배포 패키지의 아트 계약은 Godot FileAccess, 픽셀은 임포트된 Texture2D.GetImage로 읽어 프로젝트 원본 폴더 의존을 제거한다. 기존 이미지 해시/색/알파 검사는 유지한다.
- PLAN-015 추가 요청: 기존 전체 화면 가로줄 셔터 대신1.5초·440×112 봉쇄 배너를 사용한다. SystemController의 중단 책임과 SystemCueWindow의 장식 표현 수명은 분리하며 [실행 증거](../plans/evidence/PLAN-015/run-001/README.md)에 최종 화면·시험 한계를 기록한다.

## 공식 API

- [마스터 볼륨 설정값](https://learn.microsoft.com/en-us/windows/win32/api/endpointvolume/nf-endpointvolume-iaudioendpointvolume-getmastervolumelevelscalar)
- [RegisterHotKey](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey)
- [Godot DisplayServer](https://docs.godotengine.org/en/stable/classes/class_displayserver.html), [NativeMenu](https://docs.godotengine.org/en/stable/classes/class_nativemenu.html). 실제 시그니처는 설치된 GodotSharp.xml로 대조했다.
- [Godot 애플리케이션의 시작 화면 숨김](https://docs.godotengine.org/en/latest/tutorials/ui/creating_applications.html#hiding-the-window-during-the-splash-screen): 완전한 네이티브 hidden 시작 옵션 대신 최소 크기 투명 창을 사용하는 근거. 실제 적용은 설치 엔진과 Windows 실행으로 검증한다.

## 후속 적용

[TECH-016](TECH-016-scp-activation-and-shared-world.md)이 공용 대상 소유권·SCP별 활동·봉쇄 연결을 확장한다. 이 문서의 기존 검증은 당시 구현의 증거로 보존한다.
