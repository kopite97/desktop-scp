---
id: PLAN-016
type: development
status: completed
title: SCP별 활성화 설정과 확장 가능한 수명 관리
updated: 2026-11-25
---

# PLAN-016 SCP별 활성화 설정과 확장 가능한 수명 관리

작성·실행·상태 전환은 [PLAN 관리 지침](plan.md)을 따른다.

## 목표와 범위

- 목표: 제어창에서 SCP-999·049·173을 각각 켜고 끄며, 선택을 재실행 후에도 유지한다. 신규 SCP는 등록 정보와 개별 모듈을 추가하는 방식으로 같은 설정·봉쇄·종료 경로에 참여한다.
- 요청 근거: 2026-11-25 사용자 “각 SCP를 활성화/비활성화 할 수 있는 옵션”과 “확장이 쉬운 구조” 검토 요청 후 “해당 검토사항을 바탕으로 PLAN을 작성해줘.”
- 실행 승인: 2026-11-25 사용자 “PLAN을 진행해줘.”에 따라 본 PLAN의 정책·구현·검증 범위를 활성화했다.
- 포함: 등록 목록, 활성 상태 관리, SCP별 수명 연결, 049와 공용 복제 오브젝트 분리, 설정 UI·저장, 지표·스트레스·입력·외부 효과 연계, 회귀와 로컬 EXE 검증.
- 제외: 새 SCP 제작, SCP-049-2 독립 스위치, 동적 플러그인/핫 로딩, 범용 ECS·DI 프레임워크, 게임 진행 저장, 자동 실행·설치·서명·외부 배포, 새 아트·조명·밸런스 변경, 장시간 성능 측정.

## 기준 문서와 선행 조건

- 기획: [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md), [시스템 반응](../game-design/system-reactions.md), [실제 아이콘 복제](../game-design/desktop-icon-clones.md), [좀비 표현 계약](../game-design/icon-zombie-presentation.md).
- 선행: [PLAN-006](PLAN-006-scp-049.md)·[PLAN-007](PLAN-007-scp-173.md)의 상호작용, [PLAN-014](PLAN-014-zombie-art-integration.md)의 좀비/효과 수명, [PLAN-015](PLAN-015-tray-residency.md)의 트레이 상주. 모두 completed이며 기존 증거는 보존한다.
- 지침: [작업 절차](../agents/workflow.md), [구조](../agents/project-structure.md), [아키텍처](../agents/architecture.md), [Windows 연동](../agents/windows-integration.md), [Godot 도구](../agents/godot-tools.md), [하네스](../agents/harness.md), [검증](../agents/verification.md).
- 관련 결정: [TECH-011](../decisions/TECH-011-plague-objects-and-reservations.md), [TECH-012](../decisions/TECH-012-overlay-stacking.md), [TECH-014](../decisions/TECH-014-system-options-and-lockdown.md), [TECH-015](../decisions/TECH-015-zombie-presentation-lifetime.md).
- 실행 전 현재 파일 변경·도구·active 수를 재확인한다. 기존 진단의 생성 플래그와 제품의 사용자 활성화 설정을 구분하고 진단 설정은 격리한다.

## 현재 구조에서 해결할 문제

- Main이 세 SCP 생성·상태 표시·봉쇄·종료를 직접 열거한다. 현재 EnablePlague/EnableStatue는 생성 시 진단용 선택이며 사용자 설정이 아니다.
- StatueController/StatueState가 PlagueController/PlagueState에 의존한다. 현재는 049가 없으면 173도 생성되지 않고 공용 대상의 갱신도 049 수명에 묶인다.
- 봉쇄 해제의 일괄 SetVisible(true)는 사용자가 꺼 둔 SCP까지 다시 켤 수 있다.
- 표시만 숨기면 공통 스트레스 완화·시스템 지표 반응·고스트레스 외부 효과가 비활성 SCP에도 적용될 수 있다.

## 승인된 제품 동작

2026-11-25 실행 승인 범위로 구현했다. 현재 제품 규칙 원본은 [SCP 활성화](../game-design/scp-activation.md), 기술 책임은 [TECH-016](../decisions/TECH-016-scp-activation-and-shared-world.md)이다.

| 사용자 설정 | 전체 봉쇄 | 실제 동작·제어창 상태 |
| --- | --- | --- |
| ON | 해제 | 활동. 체크 ON, 상태 “활성” |
| ON | 봉쇄 | 표시·게임 갱신·입력 중단. 체크 ON, 상태 “봉쇄 중” |
| OFF | 어느 상태든 | 비활성. 체크 OFF, 상태 “비활성” |

- 처음 실행할 때 기존 세 SCP 기본값은 ON이다. 제어창의 SCP 목록은 이름·설명·체크박스·실제 상태를 등록 정보로 생성한다.
- 체크 변경은 즉시 공통 명령으로 적용한다. 봉쇄 중 변경도 설정에 반영하되, ON으로 바꾼 SCP는 해제 전까지 나타나지 않는다.
- 제어창 숨김은 설정·게임 진행과 독립적이다. 모두 OFF여도 앱·트레이·제어창 접근은 유지하며, 트레이 Exit만 정상 완전 종료 경로로 유지한다.
- 개별 OFF 시 본체와 소유 효과를 숨기고 AI·물리·게임 시간·입력 감시·지표 반응을 멈춘다. 해당 SCP의 스트레스는 증가·감소 모두 멈추고 다른 SCP의 상호작용 대상에서도 제외한다.
- 같은 세션의 위치·스트레스·개체 상태는 보존한다. 드래그·대상 예약·진행 중 치료/공격은 취소하고 일시 효과를 정리한다. 재활성화는 유효한 대기/탐색 상태에서 시작한다.
- 전체 봉쇄만 한 경우에는 기존 치료 진행 보존 규칙을 유지한다. 봉쇄 중 개별 OFF를 선택하면 해당 SCP에는 개별 비활성화의 취소 규칙을 적용한다.
- 재활성화·봉쇄 해제 시 최신 모니터·발판·대상 유효성을 확인하며 중단 시간의 이동·수명·스트레스를 한꺼번에 따라잡지 않는다.
- 049가 만든 기존 좀비는 이번 범위에서 049에 종속한다. 049 OFF 시 함께 숨김·시간 정지, ON 시 남은 수명으로 복귀한다. 치료/변이 오라와 소멸 잔상은 OFF 시 정리하여 지연 재생하지 않는다.
- 일반 복제 아이콘은 049·173 중 실제 활동하는 소비자가 있으면 유지·갱신한다. 049 OFF/173 ON에서도 생성·드래그·파손이 동작한다. 소비자가 없거나 전체 봉쇄이면 숨김·시간 정지·새 아이콘 취득을 중단한다.
- Spawn icon clone 버튼은 049 존재 여부 대신 공용 오브젝트의 활동 가능 여부를 따른다. 999만 활성인 경우 공용 대상은 정지한다.
- 비활성 SCP가 발생시킨 대기/진행 중 외부 창 효과는 취소한다. 다른 활성 SCP의 유효한 요청은 기존 옵션·쿨다운·대상 검사 규칙을 따른다. 강한 연출 기본 OFF는 유지한다.

### 설정 저장

- 안정된 설정 ID `scp-999`, `scp-049`, `scp-173`으로 ON/OFF만 저장한다. 표시 이름·목록 순서·클래스 이름을 저장 키로 쓰지 않는다.
- 기존 스트레스/예약 ID와 저장 ID가 다르면 명시적으로 연결하며, 기존 런타임 ID를 무리하게 일괄 변경하지 않는다.
- 사용자 설정은 Godot `user://scp-settings.json`에 버전과 함께 저장하고 첫 개체 표시 전에 읽는다. 시작 때 OFF인 SCP가 잠깐 나타나지 않게 한다.
- 신규 SCP의 누락 키는 해당 등록의 기본값을 사용한다. 알 수 없는 ID는 실행하지 않는다. 지원 버전의 알 수 없는 키는 가능한 한 보존한다.
- 잘못된 값은 해당 항목 기본값, 읽을 수 없는 파일은 기본값으로 복구하고 상태를 안내한다. 지원하지 않는 상위 버전/손상 원본을 자동 덮어쓰지 않는 복구 경로를 둔다.
- 정상 저장은 임시 파일 후 교체 방식으로 이전 정상 설정을 보호한다. 저장 실패 시 세션 선택은 유지하고 제어창에 저장 실패를 알린다.
- 위치·스트레스·좀비·치료 진행·봉쇄·강한 연출은 저장하지 않는다. 검증에서는 별도 저장 경로를 주입해 실제 사용자 설정을 변경하지 않는다.

## 구조와 책임

| 구성 | 단일 책임·연결 계약 |
| --- | --- |
| SCP 등록 목록 | 안정된 ID, 표시 정보, 기본값, 모듈 생성 함수, 사용하는 공용 서비스 정의. 중복 ID는 초기화 시 거부 |
| 활성화 관리자 | 사용자 Enabled의 원본과 실제 활동 상태 계산. 전체 봉쇄 원본은 기존 SystemOptions에 유지하고 변경을 전달받음. UI·하네스는 같은 명령 사용 |
| SCP 모듈 | 초기화·활동 전환·종료와 상태 조회. 전환 이유를 받아 개별 OFF의 취소와 봉쇄의 보존을 구별. AI·표현·입력 세부는 각 SCP가 소유 |
| 공용 오브젝트 | 복제 아이콘의 ID·예약·생성·물리·수명·표현, 소유자별 좀비의 갱신/노출 조건. 049는 치료/변이 명령, 173은 예약/파손 명령으로 접근 |
| 공용 환경·효과 | 모니터·창 발판·스트레스·아이콘 캐시·겹침 순서 재사용. 효과 요청에 소유 SCP ID를 연결하고 실행 직전 활동 가능 여부 재검사 |
| 제어창·저장 | 등록 목록으로 UI를 만들고 관리자 명령/상태를 사용. 파일 입출력은 별도 저장 책임으로 분리 |

- 모듈 인스턴스는 기본적으로 세션당 하나를 유지하고 ON/OFF마다 재생성하지 않는다. OS 감시·표현 창·구독의 중단/복귀/최종 정리 책임을 명시한다.
- SCP 수명과 선택된 시스템 반응만 작은 계약으로 연결한다. 모든 AI를 공통 상속 계층으로 합치거나 모든 서비스에 인터페이스를 추가하지 않는다.
- 공용 대상 상태·시계·표현은 각각 한 소유자만 갱신한다. 049와 173이 같은 프레임에 중복 Tick/Publish하지 않으며 173에서 049 컨트롤러를 직접 호출하지 않는다.
- 049 치료·좀비 변이와 173 파손의 예약 검사는 같은 공용 명령에 남긴다. 소유 SCP 비활성화 시 예약을 한 번만 해제하고 늦은 콜백이 변이·효과를 만들지 못하게 한다.
- 스트레스 값 보관과 활동 대상 여부를 분리한다. 비활성 상태를 지워 재등록하면서 값이 초기화되거나 중복 등록되는 방식을 피한다.
- Main의 조립·종료, SystemController의 봉쇄·반응, UI 상태를 등록/모듈 경로로 옮긴다. 999 표시 여부로 다른 SCP의 상태 표시를 결정하는 조건도 제거한다.
- 새 독립 SCP 추가는 모듈과 등록 항목으로 가능해야 한다. 새로운 상호작용 종류 자체는 해당 공용 서비스 확장이 필요할 수 있으며 이를 무수정 보장 대상으로 삼지 않는다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 아래 제품·문서·검증 산출물을 생성·변경했다. C# 원본의 Godot `.uid`도 보존한다.

| 산출물 | 경로 | 상태 |
| --- | --- | --- |
| 계획·목록·로드맵 | [본 PLAN](PLAN-016-scp-activation.md), [PLAN 목록](README.md), [로드맵](../game-design/roadmap.md) | 이번 작성·후보 추가 |
| 등록·모듈·활성화 조립 | `GodotProject/Scripts/App/Scps/ScpCatalog.cs`, `IScpModule.cs`, `ScpActivationManager.cs`, `Scp999Module.cs`, `Scp049Module.cs`, `Scp173Module.cs` | 신규, 같은 디렉터리 기준 |
| 저장·동적 UI | `GodotProject/Scripts/App/ScpSettingsStore.cs`, `ScpOptionsPanel.cs` | 신규 |
| 진입·봉쇄·제어창 | `GodotProject/Scripts/App/Main.cs`, `SystemController.cs`, `GodotProject/Scenes/App/Main.tscn` | 변경 |
| 공용 대상 상태·표현 | `GodotProject/Scripts/Simulation/DesktopObjectState.cs`, `GodotProject/Scripts/Overlay/DesktopObjectController.cs` | 기존 Plague 책임에서 분리·신규 |
| 개별 상태·표현·스트레스 | `GodotProject/Scripts/Simulation/PlagueState.cs`, `StatueState.cs`, `StressState.cs`, `GodotProject/Scripts/Companions/Scp999State.cs`, `GodotProject/Scripts/Overlay/PlagueController.cs`, `StatueController.cs`, `Scp999Controller.cs`, `PlaguePresentation.cs`, `DesktopIconAppearance.cs` | 변경, 관련 표현 창은 필요 시 연결 수정 |
| OS 효과 경계 | `GodotProject/Scripts/Platform/Windows/WindowsWindowShake.cs` | 수정 없이 기존 취소·복원 재사용. 소유자 연결은 SystemController에서 수행 |
| 활성화 진단·실행 진입 | `GodotProject/Scripts/Diagnostics/ScpActivationSmoke.cs`, `GodotProject/Scenes/Diagnostics/ScpActivationSmoke.tscn`, `scripts/godot.ps1` | 진단 신규·스크립트 액션 추가 |
| 회귀 진단 | `GodotProject/Scripts/Diagnostics/SystemSmoke.cs`, `Scp049Smoke.cs`, `StatueSmoke.cs`, `DesktopIconSmoke.cs`, `ZombieArtSmoke.cs` | 공용 상태 분리에 따라 변경, 실제 제품 경로 유지 |
| 제품 규칙·구조 기록 | `docs/game-design/scp-activation.md`, `docs/decisions/TECH-016-scp-activation-and-shared-world.md` | 신규·TECH-016 accepted |
| 관련 문서 | `docs/game-design/README.md`, `system-reactions.md`, `docs/decisions/README.md`, 관련 TECH, `docs/agents/architecture.md`, `project-structure.md`, `harness.md`, `godot-tools.md` | 구현 후 실제 책임·실행법에 맞게 변경 |
| 실행 증거 | `docs/plans/evidence/PLAN-016/run-001/README.md`, 각 `run-NNN/results.json`, `build.log`, `smoke.log`, `export.log`, 필요한 `controls.png` | run-001~027 생성(009 미생성), [실행 색인](evidence/PLAN-016/README.md) 참조 |
| 로컬 EXE | `.artifacts/PLAN-016/windows-x64/SiteDesktop.exe`와 PCK·data·IconWorker 의존 파일 | 기존 export 스크립트/프리셋 재사용, 원본 관리 제외 |

사용자 설정은 `user://`에만 저장하며 저장소 산출물이 아니다. 아트 제작/조명 작업은 없고 현재 승인 리소스를 재사용한다. 실행 증거는 실제 실행에만 생성했다. 시작 관찰 스크립트는 SettingsFile/ExpectedActors를 추가했고 NativeInputFixture에는 합성 입력 없는 FocusWithoutInput을 추가했다.

## 작업 목록

- [x] 검토 내용·현재 코드 결합·관련 규칙 확인, proposed PLAN 작성
- [x] 실행 승인·정책 범위·현재 파일 및 도구 상태 확인 후 active 전환
- [x] 제품 동작 원본과 TECH 기록, 공용 오브젝트의 상태·표현·예약 소유권 분리
- [x] 등록 목록·활성화 관리자·세 모듈 연결, 049 없이 173 동작 및 일관된 종료 구현
- [x] 입력·스트레스·지표·외부 효과·소유 좀비의 실제 활동 상태 연계
- [x] 동적 설정 UI·저장·시작 전 복원·실패 안내 구현
- [x] 공통 명령 기반 활성화 진단과 영향받는 기존 회귀 실행
- [x] 실제 Windows UI·입력·표현·자원 수명과 로컬 EXE 저장/재실행 확인
- [x] 결과·미검증 범위 기록, 문서 검사와 완료 기준 판정

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 세 SCP의 독립 활동 | ON/OFF 8개 조합을 제품 상태에서 검증. 특히 049 OFF/173 ON의 복제 대상 생성·예약·파손, 999 OFF에서도 다른 상태 표시 확인 | 조합별 상태·시간·대상 기록 |
| 봉쇄와 설정 분리 | 각 조합에서 봉쇄/재개, 봉쇄 중 ON/OFF, 빠른 반복 명령과 동일 값 재적용. OFF가 재활성화되지 않고 치료 보존/취소 차이 확인 | 공통 명령 결과·시간/예약 검사 |
| 전환 중 상호작용 정리 | 드래그·치료·공격·변이·소멸 중 OFF. 예약 해제, 늦은 완료 거부, 입력 포획·잔상 없음, 다른 SCP의 대상 사용 가능 | 로직 결과·native 관찰 |
| 비활성 스트레스·외부 효과 중단 | 모의 CPU/RAM/소음·999 완화·고스트레스 입력에서 비활성 값 불변. 요청 후 OFF와 진행 중 OFF에서 효과 취소·소유 창 복원 검사 | 모의 기록·소유 테스트 창 결과 |
| 공용 자원과 좀비 정책 | 049 OFF 시 소유 좀비 숨김/시간 정지, 173 사용 중 일반 대상 계속 진행, 소비자 0이면 중단. 아이콘 늦은 결과 폐기·캐시 참조 수명 확인 | 상태/시계·worker·캐시 기록 |
| 설정 복원·오류 처리 | 격리 저장 경로로 정상 재실행, 누락 키·새 등록 기본값·알 수 없는 ID·잘못된 값·손상·상위 버전·쓰기 실패 검사. 시작 OFF 개체의 창 노출 없음 | 설정 fixture·로그·시작 창 관찰 |
| 확장 경계 유지 | 진단 전용 작은 모듈을 등록해 UI·ON/OFF·봉쇄·저장·종료 참여 확인. 제품에 네 번째 SCP를 넣지 않고 개별 switch 추가가 필요 없는지 코드 검토 | 진단 결과·책임 검토 기록 |
| 반복 전환·종료 안정성 | 대표 조합 20회 전환 후 구독·입력 감시·창·stress 등록·worker 중복 및 소유 자원 잔류 검사. 반복 종료 명령의 멱등성 확인 | 전후 소유 자원 수·종료 로그 |
| 기존 게임·트레이 유지 | C# Build, EnvironmentSmoke/SystemSmoke와 상태 분리 영향이 있는 049·173·아이콘·좀비 회귀. 치료/변이/파손/알파 입력·겹침 확인 | 실제 실행별 결과·실패/건너뜀 구분 |
| 실제 조작·배포 경로 | 제어창 체크박스, X 숨김, 봉쇄/재개, 모두 OFF에서 복귀, 트레이 Exit를 Windows에서 조작. 로컬 EXE로 저장 후 재실행하여 초기 OFF 유지 확인 | 실제 UI와 콜백 시험을 구분한 기록·export/실행 로그 |
| 문서 일치 | 제품 정책·TECH·구조·PLAN/목록 갱신 후 `node scripts/validate-agent-docs.mjs`와 내용 검토 | 문서 검사 결과 |

- 자동 smoke는 회차당 최대 120초, native UI 시나리오는 회차당 최대 5분으로 제한한다. 장시간 부하나 성능 무회귀를 주장하지 않는다.
- 실제 Windows 실행은 모니터 ID·배치·DPI·버전을 기록한다. OS 입력은 소유 창/수신기를 사용하며 합성 입력 시간은 하네스 지침을 따른다. 모의 환경 변경으로 모니터 소실·발판 무효화도 검증한다.
- 로컬 Windows x64 내보내기의 목적은 저장/시작/트레이를 포함한 실제 사용 경로 확인이다. 기존 템플릿·SDK와 export 스크립트를 사용하며 준비/내보내기 최대 20분, 실행당 최대 5분이다. 외부 배포는 없다.
- 회귀는 분리한 상태·표현과 실제 영향에 맞춰 선택하고 실행 목록/이유를 기록한다. 실제 클릭 투과·트레이 조작·EXE 확인을 headless 성공으로 대체하지 않는다.

## 현재 상황과 다음 행동

- 현재: completed. 등록·활성 상태·공용 대상 분리·UI/저장·전환/외부 효과 중단과 범위 검증을 완료했다.
- 결과: SCP-049 없이 SCP-173을 구성할 수 있고, 사용자 OFF는 봉쇄/재실행 후에도 유지한다. 새 독립 모듈의 UI·저장·봉쇄·종료 참여도 진단 등록으로 확인했다.
- 실행 파일: `.artifacts/PLAN-016/windows-x64/SiteDesktop.exe`. PCK·data·IconWorker 폴더를 함께 유지한다. 실제 사용자 설정은 검증 중 변경하지 않았다.
- 다음 행동: 실행 후 트레이 Show controls에서 SCP 활성화 항목을 사용한다. 본 PLAN의 남은 구현 작업은 없다.
- 검증 한계: 시작 관찰은 표본 검사이며 기존 투명 64px 부트 창 한계가 남는다. 기존 트레이 OS 메뉴 클릭은 PLAN-015 증거를 재사용했고 이번 새 실제 UI 조작은 체크박스·저장·X다. 합성 입력 전체 묶음·다른 PC/혼합 DPI 전체·장시간 성능은 재검증하지 않았다. 상세 범위는 [실행 색인](evidence/PLAN-016/README.md)을 따른다.

## 검증 결과

| 날짜 | 환경·대상·방법 | 실제 결과·판정 | 증거·미검증 |
| --- | --- | --- | --- |
| 2026-11-25 | 작성 전 저장소 코드/문서 조회·문서 검사기 | PASS: PLAN 15개, active 0개, 로컬 링크 1799개 | 읽기 전용 검토. 기능·빌드·EXE 검증 미실행 |
| 2026-11-25 | PLAN·목록·로드맵 작성 후 문서 검사·내용 검토 | PASS: PLAN 16개, active 0개, 로컬 링크 1825개. 변경한 기존 문서의 diff 공백 검사 통과 | 문서만 변경. 제품 구현·실행 검증 미실행 |

| 2026-11-25 | C# Build·Import·EnvironmentSmoke | 경고 0/오류 0, 리소스 스캔·환경 초기화/종료 PASS | run-010/021 및 도구 출력. 일반 컴파일과 export 구분 |
| 2026-11-25 | 활성화 headless/native | headless 70개, native 75개 PASS. 후자는 소유 외부 창 효과 취소·복원 포함 | run-027/018. 추가 headless 검사와 실제 Windows 범위 구분 |
| 2026-11-25 | 기존 회귀 | System headless 18/native 39, 049 headless 43, 173 headless 18/native 29, 999 headless 66, Zombie headless 109/native 717, Doctor native 158 PASS | run-001~013/020/026. 정확한 환경·회차는 색인 참조 |
| 2026-11-25 | 실제 UI·설정 저장·X | 173 ON, 049 ON/OFF, 999 ON의 실제 클릭·저장·X 숨김 확인 | run-014 및 ui-verification.json. 입력 감지 시 재관찰 |
| 2026-11-25 | 최종 export·독립 EXE | export PASS, 선택 복원 237표본/표시 개체 391, 모두 OFF 232표본/표시 개체 0, 두 실행 모두 예상 밖 제어창 0/종료 0 | run-022~024. 약 10ms 요청 간격의 관찰 한계 |
| 2026-11-25 | 중간 실패 | Doctor 복원 시 숨김 잔류 수정 후 같은 158개 회귀 PASS. 외부 효과 수신기 전경 거부는 소유 창만 활성화한 재검사 PASS | run-011/017 실패 보존, run-012/018 통과. 초기 컴파일 실패는 도구 출력 기록 |

| 2026-11-25 | 완료 기록 후 문서 검사·diff 공백 검사 | PLAN 16개·active 0개·TECH 16개, 구조/링크/목록 PASS, diff 공백 오류 없음 | 제품 기능 검증과 문서 검사 구분 |

## 관련 결정과 상태 전환

- 기술 결정: 공유 대상 소유권과 활성 상태 책임은 TECH-016에 기록하고 TECH-011/014/015의 후속 적용 관계를 연결했다. 기존 결정의 과거 검증은 보존한다.
- 활성화 승인: 2026-11-25 사용자 “PLAN을 진행해줘.”, 본 PLAN 전체 구현·검증 범위.
- 2026-11-25: proposed 생성. 사용자의 검토 기반 PLAN 작성 요청에 따라 범위·정책 제안·검증 기준을 문서화했다.

- 2026-11-25 proposed → active: 본 PLAN 실행 지시, 선행 완료·active 0개·문서 검사 통과를 확인했다.

- 2026-11-25 active → completed: 사용자 승인 범위 구현, 실제 UI·Windows/EXE와 회귀·실패 수정 증거를 확인했다. 검증 범위와 재사용한 트레이 증거를 위에 명시했다.
