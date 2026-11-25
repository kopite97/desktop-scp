---
id: PLAN-015
type: development
status: completed
title: 제어창 없는 트레이 상주와 숨김·종료 분리
updated: 2026-11-25
---

# PLAN-015 제어창 없는 트레이 상주와 숨김·종료 분리

작성·실행·상태 전환은 [PLAN 관리 지침](plan.md)을 따른다.

## 목표와 범위

- 목표: Windows에서 실행하면 제어창 없이 트레이에 상주하고 SCP는 계속 활동한다. 제어창 X는 숨김으로, 트레이 종료는 프로그램 완전 종료로 동작한다.
- 요청 근거: 2026-11-25 사용자 “실행하면 제어창 없이 트레이에 상주하면서 SCP는 계속 활동하고, 제어창 X는 숨기기만 하고 트레이 메뉴에서 완전히 종료되게 해줘.”
- 최신 지시: 계획 작성 후 2026-11-25 사용자 “PLAN 을 진행해줘.”로 본 PLAN의 구현·검증 실행을 승인했다.
- 추가 요청: 같은 작업 중 “셔터 연출이라면 너무 빨리사라져서 모니터 오류처럼 보일 수 도 있을 것 같아. 연출을 수정해서 오해의 여지가 없게 해줘”에 따라 가로줄 셔터를 잠금 아이콘·상태/재개 안내가 있는1.5초 배너로 교체했다.
- 포함 예정: 시작 시 제어창 숨김, 창 닫기와 종료 분리, 트레이 복귀, 봉쇄와 제어창 표시의 독립성, 트레이 생성 실패 시 접근 경로, 종료 자원 정리, Windows 실행 및 로컬 EXE 검증.
- 제외: Windows 로그인 자동 실행, Windows 서비스, 설치 프로그램·서명·외부 배포, 새 아이콘 아트, 게임 규칙·아트 변경, 장시간 성능 측정, 중복 실행 정책 추가, Explorer 강제 재시작 시험.

## 기준 문서와 선행 조건

- 기준: [기획 MAP](../game-design/README.md), [개발 로드맵](../game-design/roadmap.md), [현재 시스템 반응·봉쇄](../game-design/system-reactions.md).
- 선행: [PLAN-008](PLAN-008-system-options.md)의 트레이·봉쇄·자원 정리, [PLAN-014](PLAN-014-zombie-art-integration.md)의 현재 제품 상태. 두 PLAN은 completed이며 과거 증거는 보존한다.
- 지침: [작업 절차](../agents/workflow.md), [구조](../agents/project-structure.md), [아키텍처](../agents/architecture.md), [Windows 연동](../agents/windows-integration.md), [Godot 도구](../agents/godot-tools.md), [하네스](../agents/harness.md), [검증](../agents/verification.md).
- 기존 결정: [TECH-014](../decisions/TECH-014-system-options-and-lockdown.md). DisplayServer/NativeMenu와 WindowsDesktop의 네이티브 숨김 경로를 재사용한다.
- 실행 전 active 0개와 기존 변경을 확인했다. 이전 PLAN-014 미리보기는 정상 종료한 뒤 검증했으며 사용자 작업 파일을 덮어쓰지 않았다.

## 구현할 동작

| 상황 | 목표 동작 |
| --- | --- |
| 일반 실행 | 트레이 등록 후 제어창을 숨긴 채 SCP·복제 아이콘·게임 시간이 정상 진행한다. 강한 연출 기본 OFF를 유지한다. |
| 시작 화면 | 제어창의 초기 표시·포커스 탈취를 방지할 수 있는 엔진 시작 설정과 네이티브 숨김 순서를 확인한다. 단순히 준비 완료 뒤 숨긴 것을 무깜빡임으로 판정하지 않는다. |
| 트레이 좌클릭 / Show controls | 같은 세션의 제어창을 표시·활성화한다. 게임이나 봉쇄 상태는 바꾸지 않는다. |
| 제어창 X / Alt+F4 | 제어창만 숨긴다. SCP·게임 시간·트레이·입력 감시는 해당 세션 상태를 유지한다. |
| 제어창의 기존 Exit 버튼 | 정상 트레이 사용 시 Hide controls 버튼으로 바꾸어 X와 동일한 숨김 명령을 사용한다. 완전 종료 경로는 트레이 Exit로 안내한다. |
| 봉쇄 / 재개 | 기존 공통 명령과 상태 보존을 유지한다. 봉쇄 시 기존처럼 제어창을 숨기되, 재개만으로 제어창을 강제로 열지 않는다. 봉쇄 중 명시적으로 연 제어창은 재개 후에도 유지한다. |
| 트레이 Exit | 표시·봉쇄 여부와 무관하게 공통 종료 경로로 입력 감시·핫키·트레이·효과·아이콘 작업·창을 정리하고 프로세스를 끝낸다. |
| 트레이 생성 실패 | 숨김으로 접근 불가능해지지 않도록 제어창과 명시적 종료 버튼을 유지하고 실패를 안내한다. X로 숨기지 않고 접근 가능한 상태를 유지한다. |

- 제어창의 표시 책임은 Main에 두고, 게임 봉쇄 상태는 기존 SystemController/SystemOptions가 소유한다. 숨김 명령에서 게임 Pause/Lockdown을 호출하지 않는다.
- 메인 Window.Hide 제약을 고려해 기존 WindowsDesktop.SetControlVisible을 우선 재사용한다. 신규 Win32 호출이 필요하면 Windows 경계에만 둔다.
- 트레이 등록 성공 여부를 확인한 뒤 숨김을 확정한다. 초기 숨김을 사용한다면 실패 시 제어창을 확실히 복구한다.
- 아이콘의 숨겨진 영역/시계 옆 배치는 Windows 사용자 설정을 따른다. 특정 위치를 강제하지 않는다.
- headless 및 시스템 옵션 비활성 진단에는 트레이를 요구하지 않는다. 진단 종료는 공통 명시적 종료 경로로 유지한다.
- 조명·아트 제작은 해당 없음. 기존 임시 트레이 아이콘과 승인된 제품 아트를 재사용한다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 아래 제품·문서·검증 산출물을 변경·생성했다.

| 산출물 | 경로 | 상태 |
| --- | --- | --- |
| 작업 계획·목록 | [본 PLAN](PLAN-015-tray-residency.md), [PLAN 목록](README.md) | 이번 작성·갱신 |
| 시작·숨김·종료 연결 | `GodotProject/Scripts/App/Main.cs`, `SystemController.cs`, `SiteTray.cs`, `GodotProject/Scenes/App/Main.tscn` | 변경 |
| 초기 창 설정·Windows 경계 | `GodotProject/project.godot`, `GodotProject/Scripts/App/WindowStartup.cs` 및 `.uid` | 설정 변경·오토로드 신규. WindowsDesktop은 기존 코드 재사용 |
| 배포 리소스 읽기 | `GodotProject/Scripts/Companions/SpriteAnimationSet.cs`, `Scripts/Simulation/DoctorAnimation.cs`, `Scripts/Overlay/DoctorSprite.cs`, `ZombieSprite.cs`, `Scp999Window.cs` | 원본 파일 직접 읽기를 패키지 지원 API로 변경 |
| 봉쇄 배너 | `GodotProject/Scripts/Overlay/SystemCueWindow.cs` | 기존 가로줄 제거·텍스트/잠금 아이콘/페이드로 변경 |
| 동작·자원 회귀 | `GodotProject/Scripts/Diagnostics/SystemSmoke.cs`, `EnvironmentSmoke.cs` | 기존 진단 확장·주석 수정 |
| 재현 실행·로컬 EXE 설정 | `scripts/export-windows.ps1`, `scripts/observe-tray-startup.ps1`, `GodotProject/export_presets.cfg`, `.gitignore` | 스크립트·프리셋 신규, 산출물 무시 추가. godot.ps1/csproj는 기존 경로 재사용 |
| 제품 규칙·기술·실행 안내 | `docs/game-design/system-reactions.md`, `docs/decisions/TECH-014-system-options-and-lockdown.md`, `docs/agents/godot-tools.md`, `docs/agents/harness.md` | 변경 |
| 검증 증거 | [실행 색인](evidence/PLAN-015/run-001/README.md), 각 회차 `results.json`·로그·필요 화면 | run-001~025 생성. 중간 실패 보존 |
| 로컬 배포 검증 산출물 | `.artifacts/PLAN-015/windows-x64/SiteDesktop.exe` 및 PCK/.NET/아이콘 worker 의존 파일 | 생성·실행 검증. 원본 관리 제외 |

필요한 화면만 실행별 증거에 저장한다. 단순 문서 작성 단계에서는 빈 증거 폴더·가상 결과를 만들지 않는다.

## 작업 목록

- [x] 현재 구현·기존 검증 한계와 사용자 목표를 확인하고 proposed PLAN 작성
- [x] PLAN 검토 이후 실행 지시에 따라 활성화하고 현재 파일·도구·프로세스 상태 재확인
- [x] 제어창 표시·숨김·닫기 경로를 분리하고 시작·실패·종료 흐름 구현
- [x] 봉쇄·재개에서 불필요한 제어창 복귀 제거, 트레이 메뉴·제어창 안내 정리
- [x] 기존 진단을 확장하고 C# 빌드·headless 회귀·실제 Windows 동작 확인
- [x] 로컬 Windows EXE 내보내기와 독립 실행·종료 확인
- [x] 추가 요청의 가로줄 제거·명확한 봉쇄 배너·즉시 중단/예약 해제 유지 및 실제 렌더 확인
- [x] 제품 규칙·기술 기록·실행 안내·검증 증거를 갱신하고 완료 기준 판정

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 빌드와 기존 종료 경로 유지 | C# Build, EnvironmentSmoke 및 SystemSmoke headless를 실제 제품 상태로 실행 | 실행별 로그·결과·검사 수 |
| 시작 시 트레이 상주·SCP 활동 | Windows 제품 실행의 시작 구간 화면과 HWND 표시 상태, 작업 표시줄/Alt+Tab의 숨긴 제어창 부재, SCP 위치·게임 시간 진행 확인 | 화면 관찰·상태 기록 |
| X는 게임을 멈추거나 종료하지 않음 | 실제 제어창 X 및 Alt+F4로 숨김 후 동일 PID·세션·SCP 활동과 시간 진행 확인 | 조작·native 표시·상태 기록 |
| 트레이로 다시 열 수 있음 | 실제 트레이 아이콘 클릭과 Show controls 메뉴 조작으로 같은 제어창을 열고 숨김 반복 | OS UI 조작 결과. 콜백 직접 호출과 구분 |
| 봉쇄와 창 표시 독립 | 숨김/표시 각각에서 봉쇄·재개, 봉쇄 중 제어창 열기·닫기, 재개 후 제어창 자동 표시 없음과 게임 진행 복원 확인 | SystemSmoke 결과·실제 창 상태 |
| 트레이 실패 시 접근 가능 | 트레이 실패를 통제된 진단 경계에서 주입하여 제어창·종료 버튼 유지 확인 | 실패 주입 결과. 실제 Shell 장애와 구분 |
| 완전 종료·재실행 | 실제 트레이 Exit 후 제품 PID·소유 worker 종료, 창/아이콘 제거·자원 해제와 재실행 시 재등록 확인 | 프로세스 종료 코드·로그·상태 기록 |
| 로컬 EXE에서도 같은 동작 | 에디터 없이 내보낸 일반 EXE를 실행해 시작·숨김·복귀·봉쇄·종료 및 아이콘 worker 로딩 확인 | export 로그·실행 경로·의존 파일·관찰 결과 |
| 기록 일치 | 문서 검사기와 변경 문서 내용 검토 | 검사 결과·미검증 범위 |
| 봉쇄 연출의 의미 명확화 | 실제 배너 텍스트·잠금 아이콘·1.5초 표시/종료 확인, SCP는 즉시 숨김·시계 정지 | run-025 결과·lockdown-banner.png |

- native 검증은 기존 LG 모니터 ID `GSM5BDE`의 존재를 실행 시 확인한다. 모니터 배치·DPI·실제 도구 버전을 기록하며 주 모니터 설정은 바꾸지 않는다.
- 각 자동 smoke는 최대 60초, UI 시나리오는 실행당 최대 5분으로 제한한다. 사용자 입력과 충돌하는 합성 입력 시험은 기존 지침을 따르고 별도 구간으로 분리한다.
- 내보내기는 사용자 원요청의 “빌드에서 실행” 동작을 확인하기 위한 로컬 Windows x64 검증이다. 준비·빌드·내보내기는 최대 20분, 실행 검증은 최대 5분을 기준으로 중단 원인과 결과를 기록한다. 외부 업로드·설치·서명은 수행하지 않는다.
- 내보내기 템플릿/SDK 준비 여부를 먼저 확인한다. 로컬 EXE 확인을 못 한 경우 에디터 실행 성공으로 대체하지 않고 미완료로 기록한다.
- 장시간 CPU/메모리, Explorer 재시작 복구, 다른 PC·혼합 DPI 전체 지원은 이번 완료 판정에 포함하지 않으며 검증했다고 주장하지 않는다.

## 현재 상황과 다음 행동

- 현재: completed. 트레이 상주·숨김/종료 분리·배포 리소스 로딩·봉쇄 배너 구현과 범위 검증 완료.
- 기존 PLAN-008 증거는 트레이 생성·등록 콜백을 확인했지만 OS 트레이 메뉴의 실제 클릭·배포용 EXE는 미검증이다. 이번 증거로 재사용해 완료 판정하지 않는다.
- 다음 행동: 로컬 산출물의 `SiteDesktop.exe`로 실행한다. EXE와 PCK·data·IconWorker 폴더를 함께 유지한다. 검증 프로세스는 종료했다.
- 한계: Godot 부트 중 투명한 최소 창 HWND 자체는 존재하며 Windows가1px 요청을64px로 보정한다. 초기 창 부재를 절대 보장하지 않는다. 최종 관찰에서는 제어창 크기 노출·전경 포커스 획득이 없었다. 상세한 표본 한계와 실패 판정 변경 이유는 실행 색인을 따른다.

## 검증 결과

| 날짜 | 환경·대상·방법 | 실제 결과·판정 | 증거·미검증 |
| --- | --- | --- | --- |
| 2026-11-25 | 저장소 코드·문서 읽기, 작성 전 `node scripts/validate-agent-docs.mjs` | PASS: PLAN 14개, active 0개, 문서 구조·메타데이터·로컬 링크 정상 | 구현·빌드·게임 실행·내보내기 미실행 |
| 2026-11-25 | PLAN·목록 작성 후 같은 문서 검사기와 내용 검토 | PASS: PLAN 15개, active 0개, 로컬 링크 1760개 확인 | 제품 변경 없음. 기능 검증은 구현 단계에서 수행 |
| 2026-11-25 | 최종 C# Build, EnvironmentSmoke, SystemSmoke headless/native | 경고0/오류0, 환경 종료 정상, headless18개·Windows39개 PASS | run-024/025. 네이티브 신호/콜백 시험과 실제 UI 조작 구분 |
| 2026-11-25 | 리소스 로딩 변경의 아트 회귀 | Zombie headless109개, Doctor native158개, Zombie native717개 PASS | run-006/008/009. 아트 원본 변경 없음 |
| 2026-11-25 | 배포 EXE의 실제 UI·프로세스 확인 | 실제 X는 도구로 검증, Show controls/Alt+F4/Lockdown/Resume/Exit는 사용자 “세 단계 모두 정상이에요” 확인, PID4976 종료·정리 로그 확인 | run-007. 최종 시작 보완/배너는 run-023/025 별도 검증 |
| 2026-11-25 | 최종 내보내기·EXE 재실행 관찰 | export 성공, 236표본·제어창204표본·개체589표본·예상 밖 제어창/포커스0·종료 코드0 | run-022/023. 1px/64px 투명 부트 표면 허용; 일반 제어창 숨김 확인 |
| 2026-11-25 | 완료 기록 후 문서 검사·diff 검토 | PASS: PLAN15개·active0개·로컬 링크1799개, diff 공백 오류 없음 | 기존 작업 파일의 줄바꿈 경고는 별도. 최종 배너 PNG는 run-015와 SHA256 일치 |

## 관련 결정과 상태 전환

- 관련 결정: TECH-014를 기반으로 제어창 숨김과 게임 봉쇄의 책임을 분리한다. 구현 시 해당 결정에 변경 이유·검증 링크를 반영한다.
- 2026-11-25: proposed 생성. 사용자의 최신 “PLAN을 작성해줘 먼저” 지시를 반영하여 계획 작성 단계에서 멈춘다. 직전 구현 요청을 근거로 이번 턴에 자동 활성화하지 않는다.
- 2026-11-25 proposed → active: 사용자 “PLAN 을 진행해줘.”로 본 PLAN 범위 실행 승인. 선행 완료·active 0개를 확인했다.
- 2026-11-25 active 범위 보완: 사용자 봉쇄 가로줄 피드백에 따라 기존 셔터를 명확한 상태 배너로 변경. 아트 파일 제작 없이 기존 장식 창 안에서 구현했다.
- 2026-11-25 active → completed: 실제 EXE UI 확인·종료, 최종 Windows39개·headless18개, 배포 시작 재검증·배너 렌더 및 문서/증거를 확인했다. 초반 로더/시작 검증 실패는 실행 색인에 보존했다.
