---
id: PLAN-011
type: development
status: completed
title: 개체 겹침과 드래그 중 네이티브 창 깜빡임 해결
updated: 2026-11-24
---

# PLAN-011 개체 겹침과 드래그 중 네이티브 창 깜빡임 해결

작성·실행·상태 전환은 [PLAN 관리 지침](plan.md)을 따른다.

## 목표와 범위

- 목표: LG 모니터에서 SCP-999를 드래그해 SCP-049와 겹칠 때 빠르게 깜빡이는 증상을 해결하고, 개체 간 표시 순서와 입력 대상을 안정적으로 유지한다.
- 포함: 수정 전 재현·원인 검증, 제품 오버레이 창의 공통 순서 관리, 드래그 우선순위, 효과 창의 상대 순서, 생성·삭제·숨김·복원 시 재등록, 실제 Windows 회귀 검증.
- 제외: 신규 SCP 행동, 정식 아트 제작·교체, 조명·색감 조정, 전체 화면 단일 창으로의 렌더링 구조 교체, 배포용 내보내기·고부하 측정.
- 요청 근거: 2026-11-24 사용자가 SCP-999를 마우스로 옮겨 역병의사와 겹칠 때 빠르게 깜빡인다고 보고하고, “해당 증상을 해결하기 위한 PLAN을 작성해줘.”라고 요청했다. 이후 같은 날 “PLAN-011을 승인할게 작업 시작해줘.”로 실행을 승인했다.

## 기준 문서와 선행 조건

- 기준: [기획 원본](../../SCP_site-desktop.md) 3절의 SCP 상호작용·5절의 클릭 투과, [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md) Phase 1·3 공통 후속 수정.
- 선행: [PLAN-003](PLAN-003-overlay-validation.md), [PLAN-005](PLAN-005-scp-999.md), [PLAN-006](PLAN-006-scp-049.md), [PLAN-010](PLAN-010-scp-999-trail-integration.md)의 현재 구현과 증거를 보존한다.
- 적용 지침: [아키텍처](../agents/architecture.md), [Windows 연동](../agents/windows-integration.md), [실행 하네스](../agents/harness.md), [검증](../agents/verification.md), [Godot 도구](../agents/godot-tools.md), [프로젝트 구조](../agents/project-structure.md).
- 기존 결정: [TECH-003](../decisions/TECH-003-overlay-islands.md)의 개체별 네이티브 창·물리 좌표·부분 입력 투과를 유지한다.
- 실행 선행 조건: 해당 PLAN 실행 승인 후 유일한 active로 전환한다. 현재 실행 중인 미리보기와 진단 프로세스를 구별하고 중복 실행을 차단한다.
- 아트/조명 연동: 해당 없음. 현재 승인된 SCP-999·반응·점액 리소스와 SCP-049 임시 표현을 재사용한다.

## 관찰 사실과 원인 가설

- 사용자 관찰: SCP-999와 역병의사가 겹치며, 특히 마우스로 SCP-999를 이동할 때 빠른 깜빡임이 보인다.
- 코드 확인: [Scp999Controller](../../GodotProject/Scripts/Overlay/Scp999Controller.cs)의 RefreshPlatforms와 [PlagueController](../../GodotProject/Scripts/Overlay/PlagueController.cs)의 Refresh가 각각 약 50ms마다 자기 창을 앞으로 올린다. 반응 효과도 Publish에서 반복해서 올라온다.
- 코드 확인: [WindowsDesktop](../../GodotProject/Scripts/Platform/Windows/WindowsDesktop.cs)의 RaiseWithoutFocus는 SetWindowPos와 HWND_TOPMOST를 사용해 창 순서를 변경한다. 점액은 별도로 PlaceBehind를 호출한다.
- 유력 가설: 독립 컨트롤러의 순서 변경이 충돌하며, 드래그 중 위치 변경과 화면 합성이 겹쳐 앞뒤 교대가 시각적으로 드러난다.
- 계획 작성 당시에는 인과관계가 미검증이었다. 실행 후 수정 전 반복 HWND 순서 교대와 수정 후 안정화를 측정했고, 실제 겹침 입력·화면까지 검증했다. 구체 결과는 아래 실행 기록을 따른다.
- 재현되지 않거나 순서 안정화 후에도 남으면 창 위치 갱신·투명 합성·핸들 재생성 경로를 조사한다. 구조 전면 교체가 필요하면 범위를 먼저 보완한다.

## 적용 구현 계약

- Main이 공통 오버레이 순서 관리자를 소유한다. 각 컨트롤러는 창 등록·해제, 표시 상태, 소유 개체/효과 관계, 드래그 상태를 전달하고 개별적으로 최상단 경쟁을 하지 않는다.
- 창 발판의 50ms 관찰은 유지하되, 환경 관찰과 창 순서 변경을 분리한다. 캐릭터 좌표만 바뀌는 프레임에는 불필요한 순서 변경을 요청하지 않는다.
- 평상시에는 안정된 기본 순서를 사용한다. 기본 순서는 반응 → 049 → 데이터/좀비 → 999 → 점액이며 [TECH-012](../decisions/TECH-012-overlay-stacking.md)에 명시했다. 타이머·AI 이동·관찰 주기에 따라 상대 순서가 교대하지 않아야 한다.
- 드래그 중인 개체를 다른 일반 개체보다 앞에 둔다. SCP-999·SCP-049·가짜 데이터·좀비에 같은 규칙을 적용하고, 종료·취소 시 기본 순서로 한 번 복귀한다.
- SCP-999 반응은 본체 위, 점액은 본체 뒤의 관계를 유지한다. 장식 효과의 클릭 투과를 보존하고, 효과 생성이 드래그 대상보다 다른 개체를 앞으로 올리지 않도록 전체 순서를 계산한다.
- 등록·표시·드래그 우선순위·핸들 변경 또는 실제 순서 이탈 때 필요한 변경만 공통 경로에서 적용한다. 단순히 이전 요청값만 캐시해 실제 OS 순서 이탈을 놓치지 않는다.
- 숨겨지거나 해제된 창의 핸들을 사용하지 않는다. 복원 시 새 핸들을 반영하고, 종료 시 등록과 이벤트를 정리한다. Windows 호출은 기존 플랫폼 경계에 둔다.
- 포커스를 강제로 가져오지 않으며, 앞에 보이는 개체의 입력과 빈 영역의 뒤쪽 작업 창 입력이 일치해야 한다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 제품 코드·진단과 실제 증거는 아래에 생성했다.

| 산출물 | 경로 | 상태 |
| --- | --- | --- |
| PLAN·목록·로드맵 | `docs/plans/PLAN-011-overlay-stacking.md`, `docs/plans/README.md`, `docs/game-design/roadmap.md` | 이번 작성·갱신 |
| 공통 순서 관리자 | `GodotProject/Scripts/Overlay/OverlayStackCoordinator.cs` | 생성 |
| 상태·창 연결 | `GodotProject/Scripts/App/Main.cs`, `GodotProject/Scripts/Overlay/Scp999Controller.cs`, `GodotProject/Scripts/Overlay/PlagueController.cs` | 변경 |
| 네이티브 순서 적용·조회 | `GodotProject/Scripts/Platform/Windows/WindowsDesktop.cs` | 변경 |
| 재현·회귀 진단 | `GodotProject/Scripts/Diagnostics/OverlayStackSmoke.cs`, `GodotProject/Scripts/Diagnostics/OverlayStackSmoke.Input.cs`, `GodotProject/Scenes/Diagnostics/OverlayStackSmoke.tscn`, `scripts/godot.ps1` | 생성·변경 |
| 기존 입력 진단 | `GodotProject/Scripts/Diagnostics/NativeInputFixture.cs`, `GodotProject/Scripts/Diagnostics/Scp999Smoke.cs`, `GodotProject/Scripts/Diagnostics/Scp049Smoke.Native.cs` | 재사용, 049 수신기를 일반 창으로 변경 |
| 공통 순서 결정 | `docs/decisions/TECH-012-overlay-stacking.md`, `docs/decisions/README.md` | TECH-012 생성·목록 갱신 |
| 실행 증거 | `docs/plans/evidence/PLAN-011/run-NNN/README.md`, `results.json`, `execution.log`, `stack-trace.json`, `monitor.json` | run-001부터 증가, 이전 실행 보존 |
| 실제 시각 증거 | run-001/006/007의 `frames/` 연속 PNG | 생성, frame 번호로 trace와 대응. 별도 MP4는 미생성 |

## 작업 목록

- [x] 증상·확인된 코드·원인 가설을 분리하고 범위·예정 경로 작성
- [x] LG에서 수정 전 정지 겹침·드래그 겹침을 재현하고 실제 순서/호출/화면 기록
- [x] 기본 순서·드래그·효과·창 수명 계약 확정 및 TECH 작성
- [x] 공통 순서 관리자 통합 및 컨트롤러의 독립적인 반복 올리기 제거
- [x] 실제 네이티브 겹침·입력·숨김/복원·창 생성/제거 회귀 검증
- [x] 필수 결과와 한계 기록, PLAN·목록·관련 지침 갱신

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 원인 설명에 근거가 있음 | 수정 전 동일 두 개체의 정지/이동 상태, 순서 변경 요청과 실제 상대 순서를 시간별 대조. 재현 실패는 별도 기록 | before 화면·stack-trace·README |
| 겹침 중 빠른 앞뒤 교대·깜빡임 해소 | LG GSM5BDE의 실제 Godot 창에서 999를 049 위로 느리게/빠르게 왕복 각 10초, 겹친 채 정지 10초, 드래그 시작·해제 5회. 역으로 049와 데이터/좀비도 드래그해 비교 | after 연속 화면·실제 순서 추적·results |
| 순서가 결정적이며 불필요한 변경 없음 | 우선순위·창 집합이 고정된 구간에서 의도하지 않은 상대 순서 반전 0회, 실제 이탈 없는 안정 구간의 반복 순서 적용 0회. 드래그/생성/복원에 따른 의도된 전환은 구분 | 호출 수·실제 창 순서·이벤트 trace |
| 본체·효과와 입력 계약 유지 | 겹침 앞쪽 개체 입력, 드래그 지속/해제, 999 쓰다듬기·반응·점액, 데이터 회수, 빈 영역·장식 효과 아래 InputProbe 입력 수신 확인 | 실제 입력 진단·수신 결과 |
| 창 수명·복원 정상 | 겹친 상태에서 숨김/복원, 드래그 중 숨김, 데이터/효과 생성·만료, 초기화·종료 후 핸들 오류·남은 입력 영역 없음 확인 | execution.log·results |
| 기존 기능 회귀 없음 | C# 빌드, 변경 영향에 해당하는 Scp999Smoke·Scp049Smoke와 실제 입력 검사, 문서 검사기·diff 검사 | 명령·종료 코드·실제 검사 수·결과 |

- 실제 시험은 LG FULL HD / GSM5BDE를 선택하고 당시 물리 경계·작업 영역·배율을 기록한다. 기존 음수 원점 변환을 보존한다.
- 소유한 테스트 배경과 제품 창만 캡처·조작한다. 중복 미리보기 방지와 포커스/입력 수신자 확인을 기존 하네스로 수행하며 사용자 커서 개입은 환경 간섭으로 구분한다.
- 실제 입력 구간의 필요 시간을 미리 알리고 완료 후 커서·포커스를 복원한다. 과거의 20초 마우스 중지 응답을 이번 시험에 재사용하지 않는다.
- 단일 스크린샷·headless 통과·컴파일만으로 깜빡임 해결을 판정하지 않는다. 실제 화면과 native 입력 검증이 남으면 completed로 전환하지 않는다.
- 혼합 DPI·물리 모니터 제거는 기존 미검증 한계를 보존한다. 이번 변경으로 좌표/모니터 경로를 수정하는 경우에만 해당 검증 범위를 추가한다.

## 현재 상황과 다음 행동

- 완료: Main 소유 공통 관리자, 드래그 우선순위, 효과 상대 순서, 창 수명/복원 및 실제 순서 이탈 복구를 적용했다.
- 수정 전 run-001에서 각 3초 구간에 120회 상대 순서 반전을 기록했다. 최종 run-006의 정지·느린·빠른 드래그 각 10초에서는 반전과 안정 구간의 반복 배치 적용이 모두 0회다.
- 실제 입력은 사용자 마우스 중지 응답 후 별도로 수행했다. run-007의 네 종류 개체 겹침 드래그와 run-004/008의 기존 입력 회귀가 통과했다.
- 제품 수정은 완료했으며 LG에서 [최종 미리보기](evidence/PLAN-011/run-010/README.md)를 실행했다. 추가 실행 승인 요청이나 아트 제작 대기는 없다.
- 한계: HWND 순서 변화 수는 화면 깜빡임 빈도가 아니다. 캡처는 합성된 화면의 표본이며 실제 순서 추적·입력 수신 검사와 함께 판정했다. 혼합 DPI 실기기·핫플러그·장시간 성능은 미검증이다.

## 검증 결과

| 날짜 | 환경·대상·방법 | 실제 결과·판정 | 증거·미검증 |
| --- | --- | --- | --- |
| 2026-11-24 | 계획 작성 당시 컨트롤러·WindowsDesktop 정적 조사 | 반복 순서 변경 경로 확인 | 위 소스 링크. 실제 깜빡임 인과관계·수정 효과 미검증 |
| 2026-11-24 | PLAN 작성 후 `node scripts/validate-agent-docs.mjs`, `git diff --check` | PASS: 개발 PLAN 11개·active 0개, ART 7개·active 0개, TECH 11개, 로컬 링크 1297개. diff 공백 오류 없음 | 제품 빌드·실행 시험은 계획 작성 범위에서 미실행 |
| 2026-11-24 | LG 수정 전 3개 구간 각 3초 관찰 | 각 구간 상대 순서 반전 120회, 화면 깜빡임 횟수로는 해석하지 않음 | [run-001](evidence/PLAN-011/run-001/README.md) |
| 2026-11-24 | 중간 구현·진단 실패 보존 | 과도한 외부 창 비교, 잘못된 반응 생성 경로, 미반영 수신기 설정 수정 | [run-002](evidence/PLAN-011/run-002/README.md), [run-003](evidence/PLAN-011/run-003/README.md), [run-005](evidence/PLAN-011/run-005/README.md) |
| 2026-11-24 | LG 30초 실제 창 순서·효과·수명 검사 | 25개 PASS, 각 안정 구간 순서 반전/반복 적용 0회 | [run-006](evidence/PLAN-011/run-006/README.md) |
| 2026-11-24 | 실제 겹침 오른쪽 드래그·클릭/숨김 투과 | 197개 PASS, 999·049·데이터·좀비 각각 18단계 | [run-007](evidence/PLAN-011/run-007/README.md) |
| 2026-11-24 | 기존 SCP-999/049 native + 입력 회귀 | 각각 111개/71개 PASS | [run-004](evidence/PLAN-011/run-004/README.md), [run-008](evidence/PLAN-011/run-008/README.md) |
| 2026-11-24 | 최종 Build·Import·Headless Main 수명 | PASS, 빌드 경고/오류 0개 | [run-009](evidence/PLAN-011/run-009/README.md) |
| 2026-11-24 | 완료 문서 검사·diff 검사 | PASS: PLAN 11개·active 0개, ART 7개·active 0개, TECH 12개, 로컬 링크 1355개. 공백 오류 없음 | 실행/증거 링크·목록 일치 확인 |

## 관련 결정과 상태 전환

- 기술 결정: [TECH-012](../decisions/TECH-012-overlay-stacking.md) accepted, 실제 코드와 실기기 검증에 적용.
- 활성화·재개 승인: 2026-11-24 사용자가 “PLAN-011을 승인할게 작업 시작해줘.”로 이 PLAN의 구현·검증을 승인했다.
- 2026-11-24: 신규 → proposed. 사용자 증상 보고 및 PLAN 작성 요청에 따라 완료된 PLAN-005/006을 재활성화하지 않고 후속 결함 수정으로 분리했다.
- 2026-11-24: proposed → active. 위 실행 승인에 따라 LG에서 재현·공통 순서 관리·회귀 검증을 시작했다.
- 2026-11-24: active → completed. 사용자 승인 범위의 공통 창 순서 수정, LG 실제 겹침/입력·수명·기존 999/049 회귀 및 문서 마감을 완료했다.
