---
id: PLAN-007
type: development
status: completed
title: SCP-173 시선·이동·대상 파손
updated: 2026-11-25
---

# PLAN-007 SCP-173 시선·이동·대상 파손

## 목표와 범위

- 목표·포함: 커서 시선 판정·정지, 관찰 해제 시간, 모니터 전환, 가짜 대상 순간이동·파손, 스트레스와 999 상호작용.
- 제외: 실제 외부 창 흔들기·정식 점프스케어 아트.
- 요청 근거: 2026-11-23 하네스 구축 요청에 따라 후속 후보로 작성했다. 2026-11-25 사용자 “Plan을 차례대로 진행해줘”로 PLAN-012 → PLAN-007 → PLAN-008 순차 실행 승인. 선행 완료 전까지 proposed로 대기한다.

## 기준 문서와 선행 조건

- [기획 원본](../../SCP_site-desktop.md), [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md).
- 선행: [PLAN-006](PLAN-006-scp-049.md), [PLAN-012](PLAN-012-desktop-icon-clones.md)의 런타임 아이콘 복제와 게임 연결.
- 대상 외형·원본 불변 규칙은 [아이콘 복제 기획](../game-design/desktop-icon-clones.md)을 따른다. 173은 게임 복제본을 접거나 파손한다. 아이콘별 파손 아트를 그리지 않고 취득 텍스처 분할/변형을 검토한다. 실제 Windows 항목에는 영향을 주지 않는다.
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](../agents/plan-management.md), [관련 구현 지침](../agents/harness.md), [검증](../agents/verification.md).
- 구현 시험값: 관찰 반경180px(경계 포함), 같은 화면에서 연속8초 미관찰 시 공격, 스트레스70 이상이면4초. 커서가 다른 모니터면 시간 대기 없이 공격 후보가 되되 공격/실패 재시도 간격2초를 지킨다. 잘못된 커서/화면은 관찰 상태로 처리한다. 파손 표시는0.8초. 확정 밸런스가 아닌 조정 가능한 프로토타입 값이다.
- 173은 같은 화면의 가장 가까운 미예약·접지·미파손 데이터를 선택한다. 관찰 중 자율 이동/공격은 정지하고 사용자의 드래그·외부 발판 소실에 따른 공통 물리는 유지한다. 실제 원본을 수정하지 않는다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 제품과 검증 파일은 생성되어 아래 최종 증거에 연결한다.

- 제품: `GodotProject/Scripts/Simulation/StatueState.cs`, `Overlay/StatueController.cs` 및 Main/PlagueState/PlagueActorWindow 연결. 재현 입력은 `GodotProject/Scripts/Diagnostics/StatueSmoke.cs`에 고정하고 결과 JSON에 검사명을 남긴다. 별도 scenario-inputs.json은 생성하지 않았다.
- 작업 기록: `docs/plans/PLAN-007-scp-173.md` 갱신.
- 검증 색인: `docs/plans/evidence/PLAN-007/run-003/README.md`.
- 검증 결과: `docs/plans/evidence/PLAN-007/run-001/results.json`.
- 실행 로그: `docs/plans/evidence/PLAN-007/run-001/execution.log`.
- 검증 회차는 run-001부터 증가시키며 기존 증거를 덮어쓰지 않는다. 선택 자료가 미생성인 경우 이유를 기록한다.
- 여러 기능에 영향을 주는 기술 선택이 생기면 다음 미사용 TECH ID로 기록한다.

## 작업 목록

- [x] 실행 요청·선행 결과 확인, 미정 규칙·제품 파일 경로 확정
- [x] 범위의 제품 동작과 필요한 실행 하네스 구현
- [x] 아래 필수 검증·실제 결과·미검증 한계 기록

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 관찰 중 정지하고 해제 조건에 따라 행동함 | 거리·시간 임계값 전후와 모니터 변경 시나리오 | 해당 실행 README·결과·로그 |
| 복제본 이동·파손이 공통 상태에 반영되고 실제 원본은 유지됨 | 취득 실패로 대상 없음·게임 회수·049와 예약 경합·여러 아이콘/알파의 파손 및 원본 불변 검증 | 해당 실행 README·결과·로그 |
| 999 스트레스 감소와 실제 표시가 연결됨 | 세 개체 공통 상태와 Godot 화면의 정상 상호작용 검증 | 해당 실행 README·결과·로그 |

## 현재 상황과 다음 행동

- 2026-11-25 completed. StatueState/StatueController/Main, PlagueState 파손 전환과 아이콘 분할 표시를 연결했다.
- 후속: 같은 순차 실행 승인으로 PLAN-008도 완료. 173은 임시 코드 도형이며 정식 아트/효과음 적용 완료로 보고하지 않는다.

## 검증 결과

- [최종 native run-003](evidence/PLAN-007/run-003/README.md), [규칙 run-001](evidence/PLAN-007/run-001/results.json). 관찰·시간/거리 경계·예약 경합·공통 스트레스·실제 아이콘 분할/정리·LG 좌표·실제 커서 연결 PASS. 빌드 경고/오류0.
- 실제 마우스 드래그·정식 아트·효과음·외부 창 연출은 이번 검사 범위 밖이다. 다른 모니터 판정은 실제 화면 목록과 재현 커서 입력으로 검사했다.

## 관련 결정과 상태 전환

- 관련 기술 결정: 실행 단계에서 필요한 경우 추가한다.
- 2026-11-23: proposed 생성. 후속 작업을 proposed로 작성하라는 사용자 지시를 따른다.
- 2026-11-25: 사용자 방향 변경에 따라 PLAN-012를 선행으로 추가하고 파손 대상을 런타임 아이콘 복제본으로 수정. proposed 유지.


- 2026-11-25 proposed → active: PLAN-012 완료 후 사용자 “Plan을 차례대로 진행해줘” 승인에 따라 실행.

- 2026-11-25 active → completed: 규칙 및 실제 LG 제품 검증을 마쳤다. 후속 PLAN-008은 기존 순차 실행 승인으로 진행한다.
