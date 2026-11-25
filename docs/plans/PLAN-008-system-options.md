---
id: PLAN-008
type: development
status: completed
title: 시스템 지표·옵션·봉쇄 통합
updated: 2026-11-25
---

# PLAN-008 시스템 지표·옵션·봉쇄 통합

## 목표와 범위

- 목표·포함: 시스템 지표 관찰·개체 반응, 강한 연출 옵션, 외부 창 효과 취소·복원, 트레이·단축키·정식 봉쇄/복귀.
- 제외: 실제 파일 변경·성장/해금·미요청 고부하 측정·배포.
- 요청 근거: 2026-11-23 하네스 구축 요청에 따라 후속 후보로 작성했다. 2026-11-25 사용자 “Plan을 차례대로 진행해줘”로 PLAN-012 → PLAN-007 → PLAN-008 순차 실행 승인. 선행 완료 전까지 proposed로 대기한다.

## 기준 문서와 선행 조건

- [기획 원본](../../SCP_site-desktop.md), [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md).
- 선행: [PLAN-007](PLAN-007-scp-173.md), [PLAN-012](PLAN-012-desktop-icon-clones.md).
- [아이콘 복제 기획](../game-design/desktop-icon-clones.md)의 읽기 전용 취득은 강한 연출 OFF에서도 사용할 수 있다. 봉쇄는 새 스폰·취득 결과 반영을 멈추고, 해제 시 최신 세대만 반영하며 종료 시 서비스/캐시를 정리한다.
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](../agents/plan-management.md), [관련 구현 지침](../agents/windows-integration.md), [검증](../agents/verification.md).
- 구현 규칙: [시스템 반응](../game-design/system-reactions.md). 단일 비동기 수집/1초 간격/5초 만료, 효과1.2초/간격20초로 상한을 둔다. LG 실환경·모의 고부하를 분리한다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 제품 파일은 구현되었으며 회차별 검증 증거를 아래에 연결한다.

- 제품: `GodotProject/Scripts/App/SystemController.cs`, `SiteTray.cs`, `Simulation/SystemOptions.cs`, `Platform/Windows/WindowsSystemMetrics.cs`, `LockdownHotkey.cs`, `WindowsWindowShake.cs`, `Overlay/SystemCueWindow.cs`를 생성하고 Main·999·049·173·공통 물리를 연결한다.
- 진단: `GodotProject/Scripts/Diagnostics/SystemSmoke.cs`, 대응 scene 및 scripts/godot.ps1 SystemSmoke 액션. 실제 입력은 -NativeInput 옵션으로 분리한다.
- 작업 기록: `docs/plans/PLAN-008-system-options.md` 갱신.
- 검증 색인: `docs/plans/evidence/PLAN-008/run-001/README.md`.
- 검증 결과: `docs/plans/evidence/PLAN-008/run-001/results.json`.
- 실행 로그: `docs/plans/evidence/PLAN-008/run-001/execution.log`.
- 검증 회차는 run-001부터 증가시키며 기존 증거를 덮어쓰지 않는다. 선택 자료가 미생성인 경우 이유를 기록한다.
- 여러 기능에 영향을 주는 기술 선택이 생기면 다음 미사용 TECH ID로 기록한다.

## 작업 목록

- [x] 실행 요청·선행 결과 확인, 미정 규칙·제품 파일 경로 확정
- [x] 범위의 제품 동작과 필요한 실행 하네스 구현
- [x] 아래 필수 검증·실제 결과·미검증 한계 기록

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 지표 변화가 정해진 개체 반응으로 연결됨 | 모의 지표와 실제 수집 연결 확인, 실제 고부하 발생과 분리 | 해당 실행 README·결과·로그 |
| OFF·봉쇄 시 외부 창 효과가 중단됨 | 모의 효과 호출 기록과 테스트 소유 창의 실제 이동·취소·복원 확인 | 해당 실행 README·결과·로그 |
| 트레이·단축키·UI 봉쇄가 같은 상태를 제어함 | 중단·복귀·종료·재시작, 예약 효과 취소, 아이콘 취득 중 늦은 결과 폐기와 캐시 수명 확인 | 해당 실행 README·결과·로그 |
| 프로토타입 범위의 실제 환경 검증이 기록됨 | 세 개체·창 물리·입력·옵션 통합과 미지원/미검증 조건 정리 | 해당 실행 README·결과·로그 |

## 현재 상황과 다음 행동

- 현재: 2026-11-25 completed. 시스템 지표·반응·강한 연출·트레이/단축키/봉쇄 및 LG 실제 입력 검증을 완료했다.
- 다음 행동: 승인된 순차 개발 PLAN-012 → 007 → 008 완료. 남은 정식 아트 제작/통합과 배포는 별도 범위다.
- [LG 최종 미리보기](evidence/PLAN-008/run-010/README.md)를 실행해 두었다. 강한 연출 OFF, 봉쇄/복귀 Ctrl+Shift+L.
- 실행 승인: PLAN-012 → 007 → 008 순차 실행 요청을 유지한다.

## 검증 결과

- [실행별 증거 색인·한계](evidence/PLAN-008/run-001/README.md). run-009 최종 LG28개 검사와 화면 확인 PASS. 지표 수집 최대2.8083ms(짧은 표본).
- 실제 과부하·장기 성능·트레이 OS 메뉴 자동 클릭·경보 수동 청감·혼합 DPI/탈착·배포는 미검증. 일반 전경 창만 지원하고 비정상 강제 종료 복원은 보장하지 않는다.

- Build 경고/오류0. run-001 headless 18개, run-003 LG native 27개, run-004 실제 입력 포함 43개 검사 통과. run-002의 메인 창 Hide 오류는 Windows ShowWindow 경로로 수정했다.
- run-005 SCP-049, run-006 SCP-173, run-007 공통 발판 headless 회귀 통과.
- 수치·신호·봉쇄 순서·취소 규칙은 [시스템 반응](../game-design/system-reactions.md), 기술 소유자는 [TECH-014](../decisions/TECH-014-system-options-and-lockdown.md).

## 관련 결정과 상태 전환

- 관련 기술 결정: [TECH-014](../decisions/TECH-014-system-options-and-lockdown.md).
- 2026-11-23: proposed 생성. 후속 작업을 proposed로 작성하라는 사용자 지시를 따른다.
- 2026-11-25: 런타임 아이콘 서비스와 복제본의 봉쇄/복귀/종료 검증 추가. proposed 유지.


- 2026-11-25 proposed → active: PLAN-007 완료 후 사용자 순차 실행 승인에 따라 진행한다.

- 2026-11-25 active → completed: 모의/실제 지표, 제품 반응, LG 소유 창 이동·취소·복원, 실제 글로벌 단축키/클릭, 기존 물리/049/173 회귀 및 자원 해제 증거를 확인했다.
