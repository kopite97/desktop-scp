---
id: PLAN-003
type: development
status: completed
title: 투명 오버레이·클릭 투과·좌표 검증
updated: 2026-11-23
---

# PLAN-003 투명 오버레이·클릭 투과·좌표 검증

## 목표와 범위

- 목표·포함: 투명 오버레이, 떨어진 복수 상호작용 영역과 빈 영역 클릭 투과, 모니터 좌표·배율, 최소 즉시 숨김·종료.
- 제외: SCP AI·창 발판 물리·정식 봉쇄 연출·트레이 UI.
- 요청 근거: 2026-11-23 사용자 지시 “PLAN-003 승인할게 작업 시작해줘.”로 이 PLAN 실행을 명시적으로 승인받았다.

## 기준 문서와 선행 조건

- [기획 원본](../../SCP_site-desktop.md), [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md).
- 선행: [PLAN-002](PLAN-002-development-environment.md).
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](../agents/plan-management.md), [관련 구현 지침](../agents/windows-integration.md), [검증](../agents/verification.md).
- 활성화 전: 표시/입력 좌표의 단위·변환, 창 구성 방식, 테스트 모니터 환경을 정한다. 복수 개체 사이 빈 공간까지 투과되는지 검증하고 구현 전략을 결정한다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 아래 제품 경로로 구현하고 실행별 증거를 보존한다.

- 제품 상태·좌표: `GodotProject/Scripts/Desktop/DesktopGeometry.cs`, `OverlayState.cs`.
- 네이티브 연결: `GodotProject/Scripts/Platform/Windows/WindowsDesktop.cs`.
- 오버레이 표현·수명: `GodotProject/Scripts/Overlay/OverlayController.cs`, `OverlayIsland.cs`.
- 제품 진입 변경: `GodotProject/Scripts/App/Main.cs`, `GodotProject/Scenes/App/Main.tscn`, `GodotProject/project.godot`.
- 진단: `GodotProject/Scripts/Diagnostics/OverlaySmoke.cs`, `InputProbe.cs`와 대응하는 `Scenes/Diagnostics/*.tscn` 파일.
- 실행 경로: `scripts/godot.ps1`의 OverlaySmoke·InputProbe 동작.
- 작업 기록: `docs/plans/PLAN-003-overlay-validation.md` 갱신.
- 검증 색인·최종 결과: [run-004 README](evidence/PLAN-003/run-004/README.md), [results.json](evidence/PLAN-003/run-004/results.json).
- 실행 로그: run-001~004의 build·headless·Windows 로그. 실패 회차도 각 README와 함께 보존한다.
- 실제 입력: [run-003 관찰 기록](evidence/PLAN-003/run-003/input-checks.json), [제품 로그](evidence/PLAN-003/run-003/overlay-interactive.log), [공유 수신 프로세스 로그](evidence/PLAN-003/run-002/input-probe.log).
- UI 증거: run-002의 overlay.png, run-003의 moved.jpg·hidden.jpg·restored.jpg·exited.jpg.
- 검증 회차는 run-001부터 증가시키며 기존 증거를 덮어쓰지 않는다. 선택 자료가 미생성인 경우 이유를 기록한다.
- 여러 기능에 영향을 주는 기술 선택이 생기면 다음 미사용 TECH ID로 기록한다.

## 작업 목록

- [x] 실행 요청·선행 결과 확인, 미정 규칙·제품 파일 경로 확정
- [x] 범위의 제품 동작과 필요한 실행 하네스 구현
- [x] 아래 필수 검증·실제 결과·미검증 한계 기록

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 투명 표시와 복수 입력 영역이 동작함 | 실제 Windows에서 개체 영역과 개체 사이 빈 영역을 조작하고 뒤쪽 테스트 창 입력 수신 관찰 | 해당 실행 README·결과·로그 |
| 표시·입력 좌표가 일치함 | 음수 원점·혼합 배율·모니터 경계의 가능한 실제 환경 확인, 미확인 조합 별도 기록 | 해당 실행 README·결과·로그 |
| 즉시 숨김·종료 시 입력이 남지 않음 | 숨김·복귀·종료 후 뒤쪽 창 조작과 자원 정리 확인 | 해당 실행 README·결과·로그 |

## 현재 상황과 다음 행동

- 현재: 범위 구현·실제 Windows 입력 검증·증거 기록 완료. 테스트 창과 프로세스 모두 정상 종료했다.
- 다음 행동: PLAN-004는 proposed로 유지한다. 해당 PLAN의 별도 사용자 실행 승인 후 착수한다.
- 막힌 점: 없음.

## 검증 결과

- 최종 C# 빌드 0경고·0오류, headless 상태 검사 10개, Windows 검사 23개 통과. 기존 환경 smoke도 통과.
- 실제 두 개체 사이·잘린 모서리 클릭은 별도 프로세스에 전달되고 각 몸체 클릭은 제품이 수신했다.
- 빠른 드래그 후 이전 위치는 투과, 이동 위치는 제품 수신을 확인했다. 숨김 때 수신 창 4/4, 종료 후 5/5 클릭 수신.
- Show가 위치·클릭 수를 복원했다. 제품과 수신 프로세스 종료 코드 0, 남은 테스트 창 0개.
- 실제 96 DPI 모니터 두 대에서 native 위치 일치, 보조 모니터 음수 위치 (-1856,1125) 배치를 확인했다.
- 최초 Godot/Win32 원점 혼동과 빠른 드래그 좌표 결함은 수정하고 실패 로그를 보존했다.
- 혼합 96/144 DPI·화면 제거·경계는 모의 상태 검증이다. 실제 혼합 배율·핫플러그·모니터 간 마우스 드래그·RDP·성능·내보내기는 미검증.
- [종합 실행 증거](evidence/PLAN-003/run-004/README.md)에 환경·명령·관찰·한계를 기록했다. 문서 검사 통과 결과는 같은 회차 docs-validation-final.log에 보관한다.

## 관련 결정과 상태 전환

- 관련 기술 결정: [TECH-003](../decisions/TECH-003-overlay-islands.md). 두 개체별 native 창·공통 표시/입력 윤곽·Win32 물리 좌표 변환.
- 임시 표현: 주황·청록 다각형 두 개와 클릭 수. 정식 SCP 아트·행동 규칙을 의미하지 않는다.
- 2026-11-23: proposed 생성. 후속 작업을 proposed로 작성하라는 사용자 지시를 따른다.
- 2026-11-23: proposed → active. 사용자 명시적 승인 “PLAN-003 승인할게 작업 시작해줘.”에 따라 전환. PLAN-004 이후 실행은 포함하지 않는다.
- 2026-11-23: active → completed. 구현과 필수 실제 입력/숨김/종료 검증, 가능한 모니터 환경 확인 및 미검증 한계 기록 완료.

