---
id: TECH-012
status: accepted
title: 제품 오버레이의 단일 창 순서 소유자와 드래그 우선순위
updated: 2026-11-24
---

# TECH-012 제품 오버레이의 단일 창 순서 소유자와 드래그 우선순위

- 결정일·위임 근거: 2026-11-24 사용자 PLAN-011 실행 승인 안에서 창 순서 충돌 해결 방법을 채택했다.
- 관련: [PLAN-011](../plans/PLAN-011-overlay-stacking.md), [기획 원본](../../SCP_site-desktop.md) 3·5절.
- 대체 관계: [TECH-003](TECH-003-overlay-islands.md)의 개체별 native 창 구조를 보완한다. 좌표·입력 윤곽·아트/조명 계약은 유지한다.
- 실제 적용: [Main](../../GodotProject/Scripts/App/Main.cs), [OverlayStackCoordinator](../../GodotProject/Scripts/Overlay/OverlayStackCoordinator.cs), [WindowsDesktop](../../GodotProject/Scripts/Platform/Windows/WindowsDesktop.cs), SCP-999/049 컨트롤러.

## 문제와 제약

두 컨트롤러가 50ms마다 각자 HWND_TOPMOST를 요청해 서로의 창을 앞뒤로 바꿨다.
반응과 점액도 별도 경로에서 순서를 변경했다. 겹침·드래그 중 하나의 일관된 순서가 필요하다.
작업 창 포커스·투명 영역 입력 투과·네이티브 창 수명은 보존해야 한다.

## 결정

- Main이 단일 OverlayStackCoordinator를 소유한다. 컨트롤러는 소유 그룹의 창·레이어·개체 ID·드래그 여부를 Publish하며 종료 시 그룹을 제거한다.
- 평상시 앞→뒤 순서: 999 반응(40), 049(30), 데이터/좀비(20), 999 본체(10), 점액(0). 같은 레이어는 증가 ID의 역순이다. 기존 초기화/관찰 이후의 상대 순서를 기본으로 삼았다.
- 드래그 중인 개체를 기본 레이어보다 우선한다. 999를 드래그하면 반응도 본체와 함께 우선하며 점액은 뒤에 남는다. 다른 개체를 드래그하면 그 개체가 반응보다 앞선다. 종료·취소 시 기본 순서로 복귀한다.
- 컨트롤러 갱신 뒤 ProcessPriority 1000에서 한 번 조정한다. 표시 창의 유효 HWND를 매번 취득하고 이전 배열과 비교한다. 숨김·삭제·복원은 새 집합/핸들로 처리한다.
- 집합·우선순위가 바뀌면 적용하고, 50ms마다 실제 제품 창의 상대 순서와 TOPMOST 플래그를 읽어 이탈을 복구한다. 위치 변경만으로 순서를 다시 적용하지 않는다.
- 적용은 BeginDeferWindowPos/DeferWindowPos/EndDeferWindowPos의 한 배치다. SWP_NOSIZE·NOMOVE·NOACTIVATE·NOOWNERZORDER를 사용한다. 첫 창은 TOPMOST, 나머지는 바로 앞 제품 창 뒤로 배치한다.
- 순서 검사에서 등록되지 않은 외부 창은 제외한다. 다른 앱의 최상단 창·메뉴와 지속적인 최상단 경쟁을 하지 않는다. 모든 제품 창의 TOPMOST 성질은 유지한다.
- 창 발판 관찰 주기와 AI/물리·클릭 판정은 변경하지 않는다. 독립 PlatformPreview의 단일 마커는 제품 공통 관리자 범위 밖이다.

## 대안과 선택 이유

- 각 컨트롤러의 호출 주기만 늘리기: 충돌을 줄일 수 있어도 단일 순서를 보장하지 못한다.
- 한쪽 캐릭터만 항상 앞에 고정: 드래그 중인 대상이 가려지는 문제가 남는다.
- 요청값만 캐시: Godot 숨김/복원이나 외부 순서 변경 후 실제 HWND 순서 이탈을 놓친다.
- 화면 전체 단일 렌더 창: 이번 결함 해결보다 범위가 크고 기존 부분 클릭 투과 구조를 다시 검증해야 한다.
- 모든 외부 창보다 항상 앞인지 비교: 첫 구현에서 안정 구간의 불필요한 재적용이 발생했다. 제품 창 사이의 순서만 검증하도록 좁혔다.

## 영향과 검증

- [30초 순서·수명 검사](../plans/evidence/PLAN-011/run-006/README.md), [실제 겹침 입력](../plans/evidence/PLAN-011/run-007/README.md), 기존 999/049 native 회귀가 통과했다.
- 새 개체를 도입할 때 컨트롤러가 직접 RaiseWithoutFocus를 호출하지 않고 그룹/레이어 계약에 참가해야 한다.
- 혼합 DPI 실기기·모니터 핫플러그·장시간 성능은 이번에 측정하지 않았다. 다른 앱의 TOPMOST 창이 제품을 가리는 경우는 상대 순서 결함과 구분한다.
- 공식 API 계약: [DeferWindowPos](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-deferwindowpos), [EndDeferWindowPos](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enddeferwindowpos).
