---
id: TECH-009
status: accepted
title: DWM 창 관찰과 공통 일방향 발판 물리
updated: 2026-11-24
---

# TECH-009 DWM 창 관찰과 공통 일방향 발판 물리

- 결정일·위임 근거: 2026-11-24, 사용자의 PLAN-004 실행 및 LG 모니터 사용 요청.
- 관련 [PLAN-004](../plans/PLAN-004-window-platforms.md), [원본 기획](../../SCP_site-desktop.md) 4절, [TECH-003](TECH-003-overlay-islands.md).
- 적용: [WindowsWindowObserver](../../GodotProject/Scripts/Platform/Windows/WindowsWindowObserver.cs), [PlatformWorld](../../GodotProject/Scripts/Platforms/PlatformWorld.cs), [PlatformController](../../GodotProject/Scripts/Overlay/PlatformController.cs), [PlatformDesktop](../../GodotProject/Scripts/App/PlatformDesktop.cs).
- 대체 관계: 기존 네이티브 창/물리 좌표 경계를 확장한다. PLAN-005의 상태 소유를 대체하거나 개별 AI를 자동 이관하지 않는다.

## 문제와 제약

실제 창 상태와 재현 입력이 같은 발판 규칙을 사용해야 한다. 창 프레임 전체가 보행 영역은 아니며 앞 창에 가려진 구간, 최소화·종료·최대화·모니터 원점/작업 영역을 구분해야 한다. 사용자의 창은 읽기만 하고 검증에서 이동하는 대상은 소유한 시험 창으로 제한한다.

## 결정

- EnumWindows 앞→뒤 순서에서 visible/non-minimized/non-cloaked 창의 DWM extended frame bounds를 읽는다. 제목·본문·키 입력은 읽지 않는다. own process, shell, input-transparent 장식은 제외하며 일반 caption 창만 발판 후보로 삼는다. 발판 비대상 창도 시각 가림에는 반영한다.
- 작업 영역/몸체 높이로 후보 상단을 자른 뒤 앞 창의 사각 영역을 차감한다. 발 전체 폭이 놓일 수 있는 구간만 일방향 표면이 된다. 최대화된 화면 꼭대기는 몸체 높이가 없어 발판이 되지 않는다. 바닥은 usable 하단-8px이다.
- PlatformWorld는 순수 관찰값과 경과 시간을 받아 단일 상태를 갱신한다. 100ms 입력 상한/최대1⁄120초 소분할, 일정 중력의 정확한 위치 적분과 하강 교차 검사로 얇은 발판 통과를 막는다.
- 지지 창의 원점 변위를 적용하고 제한된 수평 미끄러짐을 추가한다. 지지면 소실/가림/큰 순간 이동은 낙하로 전환한다. 도약은 탄도 도착 시간·거리/높이 제한과 중간 착지 면을 확인하며 가장 가까운 가능한 인접 발판을 선택한다.
- native 관찰은50ms, 모니터 확인은500ms. 숨김 중 중단하고 복원 시 새 관찰로 지지를 확인한다. 관측 generation으로 HWND 재사용을 구분하되 poll 사이에 놓친 재사용은 한계로 기록한다.
- LG 선택은 WMI 제품 GSM5BDE와 EnumDisplayDevices의 하드웨어 ID·GetMonitorInfo의 물리 좌표를 대조한다. 시스템 화면 설정은 변경하지 않는다. [WindowsMonitors](../../GodotProject/Scripts/Platform/Windows/WindowsMonitors.cs), [DesktopPlacement](../../GodotProject/Scripts/Desktop/DesktopPlacement.cs).

## 대안과 선택 이유

- Godot PhysicsBody2D의 충돌 세계와 별도 검증용 시뮬레이션 대신 작은 공통 solver를 선택했다. 현재 필요한 것은 창 상단의 일방향 충돌이며 상태/시간 재현을 동일 경로로 유지할 수 있다.
- 이벤트 hook 기반 창 변화 수집은 도입하지 않았다. bounded polling은 수명/스레드 정리가 단순하며 원본 프로토타입 범위에 충분하다. 빠른 변화는 관측 변위로 처리하고 보간하지 않는다.
- 최초 반암시적 적분은 도약 예측과 착지 시점이 달라 좁은 여유에서 실패했다. 정확한 등가속 위치식으로 바꾸어 예측과 실행을 맞췄다.
- native AlwaysOnTop만으로는 나중에 만들어진 시험 배경 위 표시를 보장하지 못했다. NOACTIVATE로 갱신 시 마커를 위에 배치하고 실제 픽셀을 검증한다.

## 영향과 검증

- [최종 증거](../plans/evidence/PLAN-004/run-008/README.md): headless36/native54, LG 실제 창 관찰/이동/최소화/최대화/가려짐/종료/도약, 입력 재현 trace 일치, native 화면 픽셀 확인.
- 005의 개체 표현/제스처/점액 최종 연결은 보류 상태에서 진행하지 않았다. 일반 물리 마커 실행 경로를 제공한다.
- 사각 가림, fallback GetWindowRect, poll 사이의 변화, 보호/비정형/투명 창·실제 혼합 DPI·장기 비용의 한계는 증거에 남겼다. 기술 선택으로 수치나 제품 지원 범위를 확정하지 않는다.

## 공식 기준

- [EnumWindows](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumwindows), [DwmGetWindowAttribute](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmgetwindowattribute).
- [GetWindowRect](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect): resize border와 DWM 경계의 DPI 차이.
- [EnumDisplayDevices](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaydevicesw): GDI device와 모니터 hardware interface의 연결.

## 후속 통합

- 2026-11-24 [TECH-010](TECH-010-companion-platform-integration.md)에서 SCP-999 위치를 공통 물리에 연결했다. 위 초기 단계의 개별 SCP 미통합/바닥 정책은 당시 기록으로 보존하며 현재 통합 결과는 후속 결정과 PLAN-005 최종 증거를 따른다.
