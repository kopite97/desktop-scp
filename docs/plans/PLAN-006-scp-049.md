---
id: PLAN-006
type: development
status: completed
title: 가짜 데이터·SCP-049·좀비 수명
updated: 2026-11-25
---

# PLAN-006 가짜 데이터·SCP-049·좀비 수명

## 목표와 범위

- 목표·포함: 게임 내부 가짜 데이터 생성, SCP-049 대상 탐색·치료, SCP-049-2 변이·배회·수명 또는 회수.
- 제외: 실제 파일 정리·삭제·시스템 지표 가속·정식 아트 제작.
- 요청 근거: 2026-11-23 하네스 구축 요청에 따라 후속 후보로 작성했다. 2026-11-24 사용자가 “이제 Plan-006을 진행해줘. 이번에도 LG 모니터에서 작업해줘. 필요한 아트는 따로 Art전용 Plan을 생성해”로 구현과 ART 계획 생성을 요청했다.

## 기준 문서와 선행 조건

- [기획 원본](../../SCP_site-desktop.md), [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md).
- 선행: [PLAN-005](PLAN-005-scp-999.md).
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](../agents/plan-management.md), [관련 구현 지침](../agents/harness.md), [검증](../agents/verification.md).
- 활성화 전: 대상 생성 주기·최대 수·치료 시간·중복 대상 처리·좀비 소멸/회수 규칙을 정한다. 명시되지 않은 선택을 기획 확정값으로 복사하지 않는다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 최종 파일·증거는 아래와 같다. 최초 예정 경로와 구분해 실제 생성 위치를 기록한다.

- 제품: [PlagueState](../../GodotProject/Scripts/Simulation/PlagueState.cs)·[설정](../../GodotProject/Scripts/Simulation/PlagueSettings.cs)·[컨트롤러](../../GodotProject/Scripts/Overlay/PlagueController.cs)·[임시 표시/입력](../../GodotProject/Scripts/Overlay/PlagueActorWindow.cs), Main에 통합.
- 검증: Scp049Smoke.cs/Scp049Smoke.Native.cs·씬, godot.ps1 Scp049Smoke 액션. [최종 증거](evidence/PLAN-006/run-026/README.md), [결과](evidence/PLAN-006/run-026/results.json), [로그](evidence/PLAN-006/run-026/execution.log). run-001은 중간 미리보기다.
- 작업 기록: `docs/plans/PLAN-006-scp-049.md` 갱신.
- 검증 색인: `docs/plans/evidence/PLAN-006/run-026/README.md`.
- 검증 결과: `docs/plans/evidence/PLAN-006/run-026/results.json`.
- 실행 로그: `docs/plans/evidence/PLAN-006/run-026/execution.log`.
- 검증 회차는 run-001부터 증가시키며 기존 증거를 덮어쓰지 않는다. 선택 자료가 미생성인 경우 이유를 기록한다.
- 여러 기능에 영향을 주는 기술 선택이 생기면 다음 미사용 TECH ID로 기록한다.

## 후속 방향 변경

- 2026-11-25 사용자 요청으로 고정 .tmp/쓰레기 외형을 실제 바탕화면 아이콘 복제본으로 변경한다. [PLAN-012](PLAN-012-desktop-icon-clones.md)가 새 취득·캐시·게임 연결을 담당한다.
- 아래 완료 범위·검증·OS 파일 미열람 기록은 당시 구현의 사실로 보존한다. 새 기능 완료를 의미하지 않는다. ART-005는 취소, ART-006은 좀비 부착 요소로 변경, ART-007은 공통 효과로 유지한다.

## 필요한 아트

- [ART-004](../../Art/docs/plans/ART-004-scp-049-character.md), [ART-005](../../Art/docs/plans/ART-005-dummy-data.md), [ART-006](../../Art/docs/plans/ART-006-scp-049-2-zombie.md), [ART-007](../../Art/docs/plans/ART-007-scp-049-mutation.md). 원본 제작은 별도 후보이며 현재 개발에서는 코드 도형을 임시 표현으로 사용한다.

## 구현 계약 — 2026-11-24

- LG GSM5BDE에서 개발/실제 검증. PLAN-005/010 완료 기반을 사용한다. 정식 ART 제작 실행은 별도이며 이번에는 proposed 계획4개를 생성한다.
- 시험값: 가짜 데이터6초 간격/상한6개/수명90초, 의사 속도80px/s/치료거리44px/치료3초/재시도2초/탐색 최대12초, 좀비 속도40px/s/상한8개/수명45초, 변이 효과0.8초. 제품 확정 기획값이 아니다.
- 데이터는 메모리 객체이며 .tmp는 화면의 모양/표기뿐이다. OS 파일 생성/열람/이동/삭제를 하지 않는다. 수명 만료 또는 왼쪽 클릭으로 회수한다.
- 단일 PlagueState가 개체 ID·예약·치료·변이를 소유한다. 대상 하나는 의사 하나만 예약할 수 있고 변이는 소유/거리/접지/용량을 재검사하는 원자 명령이다. 소실/만료/이동/드래그/발판 소실은 치료를 취소한다.
- 의사와 데이터/좀비 위치는 기존 PlatformWorld만 소유한다. 데이터는 선택 화면 바닥에 주기 생성한다. 오른쪽 드래그로 의사/데이터를 발판에 둘 수 있다. 같은 화면에서 도달 가능한 대상부터 접근하며 높은 표면은 공통 도약, 아래 표면은 가장자리 낙하, 제한 시간 초과는 예약 해제 후 재탐색한다.
- 좀비는 화면 하단 오른쪽 구석으로 모여 제한 구간을 왕복한다. 각자의 ID/물리/수명을 갖고 클릭 회수한다. 좀비 상한이면 치료를 시작하지 않아 대상 손실을 막는다.
- Main의 공통 StressState에 scp049를 등록/위치 갱신하며 SCP-999 쓰다듬기로 감소한다. OS 부하 가속·스트레스 자동 증가는 PLAN-008 범위다.
- 숨김은 생성/이동/치료/수명/효과 시간을 동결, 드래그 취소·입력 창 숨김. 복원 때 최신 발판을 관찰하며 사라진 대상/지지를 재검사한다. 종료는 모든 예약/객체/창/스트레스 등록을 정리한다.
- 제품 예정 파일: Scripts/Simulation/PlagueState.cs, PlagueSettings.cs, Scripts/Overlay/PlagueController.cs, PlagueActorWindow.cs. Main에 함께 생성/표시/숨김/종료하고 별도 Scp049Smoke 진단과 godot.ps1 액션을 추가한다.
- 정식 아트 미수령 상태의 임시 표현은 도형으로 구분한다. 기능 검증을 정식 아트나 시각 승인으로 보고하지 않는다.

## 작업 목록

- [x] 실행 요청·선행 결과 확인, 미정 규칙·제품 파일 경로 확정
- [x] 범위의 제품 동작과 필요한 실행 하네스 구현
- [x] 아래 필수 검증·실제 결과·미검증 한계 기록

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 탐색→치료→변이가 연결됨 | 공통 게임 상태·시간 경로로 전체 사이클 확인 | 해당 실행 README·결과·로그 |
| 대상 사라짐·중복 선택·수명이 처리됨 | 중요 예외 시나리오와 실제 화면 상태 대조 | 해당 실행 README·결과·로그 |
| 가짜 파일은 내부 오브젝트로만 존재함 | 제품 파일 접근 경로 검토와 게임 오브젝트 생성·회수 관찰 | 해당 실행 README·결과·로그 |

## 현재 상황과 다음 행동

- 2026-11-24 completed. 가짜 데이터 생성/예약/탐색/치료/좀비 변이/배회/클릭 회수/수명과 공통 스트레스, 실제 창 물리/입력/숨김/복원/종료를 구현하고 LG에서 검증했다.
- ART-004~007은 별도 proposed로 작성했다. 현재 의사/데이터/좀비/오라는 코드 도형 임시 표현이며 정식 제작·리비전 시각 승인·정식 적용은 별도 작업이다. 기능 완료를 아트 완료로 처리하지 않는다.
- 정식 아트가 제출되면 승인된 리비전의 Godot 적용을 별도 개발 PLAN으로 계획한다. 이번 요청은 ART 계획 생성이며 제작 자동 활성화나 PLAN-007 실행으로 확대하지 않는다.
- 후속 전달 기록(2026-11-24): ART-004 Scp049 r001 제작·사용자 시각 승인 완료. [제출본](../../Art/assets/characters/Scp049/revisions/r001/README.md)과 [전달 계약](../../Art/assets/characters/Scp049/handoff.md)을 연결한다. PLAN-006 완료 당시의 임시 도형은 아직 교체하지 않았으며 정식 적용은 별도 개발 범위다.

## 검증 결과

- [run-026 최종](evidence/PLAN-006/run-026/README.md): LG 실제 창/별도 프로세스 입력71개 PASS, 오류 로그0/종료0. 사용자 마우스 사용 중지 응답 후 최종 검사 성공.
- [run-016](evidence/PLAN-006/run-016/results.json) headless43개, [run-020](evidence/PLAN-006/run-020/results.json) 기존 SCP-999 native111개. Build 경고/오류0, Import·환경·Overlay headless10개 회귀 통과.
- [제어 패널](evidence/PLAN-006/run-026/control-panel.png) 전체 버튼 노출, 실제 치료/좀비 화면과 원본 코드 해시 기록. 제품 Plague 경로에 OS 파일 접근 호출이 없음을 검토했다.
- 미리보기 중복·검사 마커·foreground·외부 커서 이동으로 실패한 회차는 최종 README에서 원인/한계를 구분하고 보존했다. 통과 회차만 성공으로 판정했다.
- 정식 ART 제작/시각 승인, 완전한 다중 창 경로 탐색, 실제 혼합 DPI/모니터 물리 분리, 장기 비용/배포는 이번 범위에서 완료하지 않았다.
- 문서 검사 PASS(개발 PLAN10/ART7/TECH11, 각 active0), `git diff --check` PASS. [최종 LG 실행](evidence/PLAN-006/run-027/README.md)을 열어 두었고 중복 미리보기 native 진단 거부 검사도 통과했다.

## 관련 결정과 상태 전환

- 관련 기술 결정: [TECH-011](../decisions/TECH-011-plague-objects-and-reservations.md).
- 2026-11-23: proposed 생성. 후속 작업을 proposed로 작성하라는 사용자 지시를 따른다.


- 2026-11-24: 사용자 PLAN-006 실행/LG 작업/별도 ART 계획 생성 요청으로 proposed → active.

- 2026-11-24: 실제 LG 필수 검증과 별도 ART 계획 작성을 완료해 active → completed.
