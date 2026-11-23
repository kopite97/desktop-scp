---
id: TECH-003
status: accepted
title: 분리된 네이티브 오버레이 창과 물리 좌표 경계
updated: 2026-11-23
---

# TECH-003 분리된 네이티브 오버레이 창과 물리 좌표 경계

- 결정일·근거: 2026-11-23, 사용자의 PLAN-003 실행 승인 안에서 창·좌표 구현 방식을 선택했다.
- 관련 작업: [PLAN-003](../plans/PLAN-003-overlay-validation.md), [기획 원본](../../SCP_site-desktop.md).
- 적용: [OverlayIsland](../../GodotProject/Scripts/Overlay/OverlayIsland.cs), [OverlayController](../../GodotProject/Scripts/Overlay/OverlayController.cs), [OverlayState](../../GodotProject/Scripts/Desktop/OverlayState.cs), [WindowsDesktop](../../GodotProject/Scripts/Platform/Windows/WindowsDesktop.cs).
- 대체 관계: 최초 오버레이 결정. [TECH-002](TECH-002-development-toolchain.md)의 엔진·렌더러를 유지한다.

## 문제와 제약

떨어진 개체 둘 사이의 빈 공간과 각 개체의 투명 모서리가 아래 앱에 입력을 전달해야 한다.
Godot의 MousePassthroughPolygon은 단일 다각형이며 빈 배열은 전체 투과가 아니라 투과 기능 해제다.
Windows에서는 다각형 밖 렌더링도 잘리므로 표시 윤곽과 입력 윤곽을 일치시킨다.

## 결정

- 개체별 작은 네이티브 Window를 만들고 하나의 Contour를 그리기·히트 검사·MousePassthroughPolygon에 사용한다.
- ForceNative·Borderless·AlwaysOnTop·Unfocusable·Transparent·TransparentBg를 켠다. 프로젝트도 per_pixel_transparency/allowed를 켠다.
- 개체 창 사이에는 창이 없으므로 연결된 보이지 않는 입력 영역이 생기지 않는다. 제어 창은 별도의 일반 창이다.
- 상태 원본은 OverlayState다. UI와 진단은 같은 이동·숨김·모니터 교체 규칙을 사용하고 Controller가 창에 반영한다.
- 상태·커서·작업 영역은 Win32 물리 데스크톱 픽셀이다. DPI는 관찰 메타데이터이며 다시 곱하지 않는다.
- Godot의 가상 화면 최소 원점 기준과 Win32의 주 모니터 원점 기준은 WindowsDesktop에서 변환한다.
- native = godot + (SM_XVIRTUALSCREEN, SM_YVIRTUALSCREEN), godot = native - 같은 원점이다.
- 드래그 시작은 입력 이벤트의 로컬 좌표와 네이티브 창 위치로 계산한다. 프레임마다 현재 커서를 읽고 놓는 마지막 위치도 반영한다.
- 왼쪽 버튼 상태는 제품에서 시작한 드래그 동안만 읽는다. 전역 키보드 훅·텍스트 수집은 없다.
- 화면 배치는 0.5초마다 비교한다. 변경 시 드래그를 취소하고 남은 작업 영역으로 개체를 제한한다. 빈 화면 관찰은 기존 배치를 유지한다.
- Hide는 드래그를 취소하고 네이티브 개체 창을 숨긴다. 제어 창은 복원·종료를 위해 유지한다. Show는 위치·클릭 수를 유지한다.
- 종료는 구독 해제·Hide·QueueFree 후 SceneTree 종료로 처리한다. HWND를 장기 캐시하지 않는다.

## 대안과 선택 이유

- 전체 화면 창 하나와 복수 영역을 잇는 다각형: 연결 부분까지 입력을 가로챌 수 있어 선택하지 않았다.
- Win32 region 합집합 직접 관리: 복수 영역 처리는 가능하지만 Godot의 창 수명·렌더링과 별도로 영역을 관리해야 한다.
- 작은 native 창: Godot의 공개 입력 영역 API로 분리된 개체를 구현·검증할 수 있어 현재 두 개체 단계에 선택했다.

## 영향과 검증

- 오렌지·청록 다각형은 기능 검증용 임시 표현이며 SCP 정식 아트·게임 개체 규칙이 아니다.
- 많은 개체의 창 비용·프레임 성능은 미측정이다. 대량 개체 도입 시 이 결정을 재평가한다.
- 개체 전체가 한 모니터의 작업 영역에 들어가도록 제한한다. 창 발판 물리는 PLAN-004 범위다.
- 실제 두 96 DPI 모니터·음수 원점 배치·클릭 투과·드래그·숨김·복원·종료를 확인했다.
- 96/144 DPI와 모니터 제거는 모의 상태 검증이다. 실제 혼합 배율·핫플러그·RDP·내보내기는 미검증이다.
- 실행별 결과와 처음 발견한 원점/빠른 드래그 결함은 PLAN-003 증거에 보존한다.

## 공식 근거

- [Godot Window](https://docs.godotengine.org/en/4.7/classes/class_window.html): 네이티브 창·투명 설정·MousePassthroughPolygon 의미.
- [DisplayServer](https://docs.godotengine.org/en/4.7/classes/class_displayserver.html): 화면 위치·작업 영역·창 핸들.
- [ProjectSettings](https://docs.godotengine.org/en/4.7/classes/class_projectsettings.html): per_pixel_transparency/allowed.
- [GetCursorPos](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos), [GetDpiForWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforwindow), [GetSystemMetrics](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics): 커서·DPI·가상 화면 원점.
