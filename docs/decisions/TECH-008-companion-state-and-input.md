---
id: TECH-008
status: accepted
title: 공통 행동 상태와 활동 입력·장식 효과 창
updated: 2026-11-24
---

# TECH-008 공통 행동 상태와 활동 입력·장식 효과 창

- 결정일·위임: 2026-11-24, 사용자의 PLAN-005 계속 구현 요청 범위에서 채택.
- 관련 [PLAN-005](../plans/PLAN-005-scp-999.md), [기획 MAP](../game-design/README.md), [TECH-003](TECH-003-overlay-islands.md), [TECH-007](TECH-007-unlit-outline-and-reactions.md). 기존 결정을 대체하지 않고 확장한다.
- 실제 적용: [Scp999State](../../GodotProject/Scripts/Companions/Scp999State.cs), [컨트롤러](../../GodotProject/Scripts/Overlay/Scp999Controller.cs), [활동 감시](../../GodotProject/Scripts/Platform/Windows/KeyboardActivityMonitor.cs), [Win32 경계](../../GodotProject/Scripts/Platform/Windows/WindowsDesktop.cs).

## 문제와 제약

제품과 하네스가 같은 행동·스트레스를 다뤄야 하며, 비활성 상태의 타이핑 활동은 내용 수집 없이 관찰해야 한다. 장식 효과는 별도 앱의 입력을 막지 않으면서 본체 창 밖까지 표시해야 한다.

## 결정

- Main이 StressState를 소유하고 SCP-999에 전달한다. 행동/gesture/태그 시간/효과는 Scp999State의 단일 원본이다. UI·native 관찰·하네스는 같은 명령/Tick을 호출한다. 다른 SCP의 개별 AI는 선행 구현하지 않는다.
- 타이핑은 WH_KEYBOARD_LL key-down 종류만 보고 한 비트 활동으로 병합한다. 포인터의 키 데이터는 읽지 않으며 문자열이나 keycode를 상태/로그에 남기지 않는다. callback은 즉시 CallNextHookEx로 전달한다. 제품 숨김·종료 시 해제하고 복원 시 새 구독을 만든다.
- 본체 입력은 실루엣별 polygon, 하트/음표·점액은 소형 native 표시 창이다. 효과는 Godot MousePassthrough에 WS_EX_LAYERED/WS_EX_TRANSPARENT와 alpha255를 추가한다. SetWindowPos NOACTIVATE로 점액을 본체 뒤에 배치한다.

## 대안과 선택 이유
- Godot MousePassthrough의 HTTRANSPARENT만 사용하면 별도 프로세스 수신 시험에서 실패했다. EnableWindow(false) 대안도 실패하여 제거했다. layered/transparent 방식은 별도 수신기의 down/up과 실제 투명 렌더 양쪽을 통과했다.
- 전체 데스크톱 크기의 장식 창 대신 소형 창으로 표시 영역을 제한한다. 기본 최대16개 효과이며 초과 시 해당 종류의 가장 오래된 것을 정리한다. 생성/소멸 비용은 별도 제품 성능 측정 대상이고 지금은 풀링을 추가하지 않는다.
- PLAN-004 이전에는 작업 영역 바닥만 사용한다. 물리와 그림을 별도 상태로 두어 typing_react의 그림 튀기를 물리 점프로 중복 적용하지 않는다. 실제 창 발판은 이 선택으로 구현된 것으로 간주하지 않는다.

## 영향과 검증

- [단계 B 증거](../plans/evidence/PLAN-005/run-025/README.md): headless56, native 상태66, 별도 입력77개 검사. 실패 실행과 foreground 제한도 보존한다.
- 실제 입력은 소유한 별도 receiver에만 합성한다. 사용자 앱 내용이나 키 입력 내용은 증거로 수집하지 않는다. 포커스가 달라지면 입력을 중단하고 fixture 소유 프로세스만 정리한다.
- 고정 조명·튜닝 값은 프로토타입이며 제품 수치 확정은 아니다. 실제 창 물리·혼합 DPI 장비·제품 전체 비용·배포는 미검증이다.

## 공식 기준

- [LowLevelKeyboardProc](https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc): callback 신속 반환과 메시지 루프 요구.
- [Godot Windows backend](https://github.com/godotengine/godot/blob/master/platform/windows/display_server_windows.cpp): WM_NCHITTEST의 mpass/HTTRANSPARENT 경로. 설치 버전의 실제 결과는 위 실행 증거로 별도 확인했다.
- [Layered windows](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features#layered-windows): WS_EX_TRANSPARENT layered 창의 하위 창 입력 전달.
- [SendInput](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput): 소유한 시험 창에 대한 합성 입력, UIPI/foreground 조건은 하네스에서 성공으로 가정하지 않는다.

## 후속 통합

- 2026-11-24 [TECH-010](TECH-010-companion-platform-integration.md)에서 SCP-999 위치를 공통 물리에 연결했다. 위 초기 단계의 개별 SCP 미통합/바닥 정책은 당시 기록으로 보존하며 현재 통합 결과는 후속 결정과 PLAN-005 최종 증거를 따른다.
