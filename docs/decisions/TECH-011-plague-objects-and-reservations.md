---
id: TECH-011
status: accepted
title: 가짜 데이터 수명과 SCP-049 예약·치료 상태
updated: 2026-11-24
---

# TECH-011 가짜 데이터 수명과 SCP-049 예약·치료 상태

- 근거: 2026-11-24 사용자 [PLAN-006](../plans/PLAN-006-scp-049.md) 실행/LG 작업/별도 ART 계획 생성 요청.
- [기획](../../SCP_site-desktop.md)3·4절, [TECH-008](TECH-008-companion-state-and-input.md), [TECH-009](TECH-009-window-platform-physics.md). 기존 선택을 확장하며 대체하지 않는다.

## 문제와 제약

가짜 파일이 실제 OS 파일과 섞이지 않아야 하고, 삭제/만료/동시 선택/발판 이동 중 치료가 중복 변이로 이어지지 않아야 한다. 원본 아트 제작과 기능 개발을 분리하고 기존 SCP-999와 같은 스트레스/물리를 사용한다.

## 결정

- [PlagueState](../../GodotProject/Scripts/Simulation/PlagueState.cs)가 데이터 ID/종류/예약/치료/변이/생성·수명 시간을 소유한다. ID는 제거 후 재사용하지 않는다. 상태에는 파일 경로/파일 접근 기능을 두지 않는다.
- TryClaim은 단일 소유자 예약, CompleteTreatment는 같은 예약·상태·충분한 시간·거리·같은 접지면·좀비 용량을 다시 확인한다. 성공하면 같은 논리 객체를 Zombie로 바꾸고 수명/변이 효과를 시작한다. 실패/삭제/물리 중단은 예약을 해제한다.
- 의사·데이터·좀비 위치는 각 PlatformWorld가 소유한다. [PlagueController](../../GodotProject/Scripts/Overlay/PlagueController.cs)는 동일한 창 관찰값을 전달하고 입력을 공통 명령으로 바꾼다. 별도 테스트용 AI/물리 복제는 없다.
- 탐색은 같은 화면의 가장 가까운 미예약 데이터, 공통 탄도 도약/아래 발판 가장자리 낙하,12초 제한과2초 재시도다. 좀비는 오른쪽 하단 제한 구간에 모여 왕복한다.
- Main이 두 컨트롤러와 StressState를 소유한다. scp049 등록/위치 갱신/종료 제거를 하며 SCP-999의 기존 Relieve가 같은 값을 감소시킨다. 숨김은 모든 게임 시간을 정지하고 복원 때 최신 지지를 확인한다.
- [PlagueActorWindow](../../GodotProject/Scripts/Overlay/PlagueActorWindow.cs)는 임시 도형·실루엣 입력을 표시한다. 원본 아트/최종 디자인은 별도 ART-004~007. 생성/치료/좀비 정책은 [PlagueSettings](../../GodotProject/Scripts/Simulation/PlagueSettings.cs)의 조정 가능한 시험값이다.
- 진단은 소유 창/별도 수신기만 사용한다. native 입력 전에 별도 미리보기 프로세스가 없는지 검사하고 HWND/foreground를 확인한다. Win32 CloseMainWindow 반환만으로 미리보기 종료를 판정하지 않는다.

## 대안과 선택 이유

- OS 임시 파일이나 바탕화면 파일로 구현하지 않는다. 기획의 데이터는 내부 오브젝트이며 삭제/변이 권한과 무관하다.
- 치료 완료마다 새 Zombie를 무조건 추가하면 오래된 콜백이나 중복 소유자가 복제할 수 있어, 예약 검증 후 같은 객체의 종류를 원자적으로 전환한다.
- 전역 최단 경로/영속 저장/범용 Entity 프레임워크는 도입하지 않는다. 현재 수십 개 이하의 bounded 상태·공통 물리와 직접 명령으로 범위를 구현했다.
- 정식 그림을 코드 도형으로 대체 완료 처리하지 않는다. 임시 표현으로 기능을 검증하고 제작 리비전/시각 검수는 ART로 분리한다.

## 영향과 검증

- [최종 LG71개](../plans/evidence/PLAN-006/run-026/README.md), headless43개, 기존 SCP-999 native111개 회귀, 환경/오버레이/Import 확인.
- 목표 소실·중복 예약·조기 완료/재완료 거부·상한/수명·숨김·모니터 복구·실제 발판/입력·공통 스트레스·종료를 포함한다.
- 동일 화면의 제한된 탐색, 사각 DWM 가림/관찰 주기와 입력 시험의 외부 커서 간섭, 정식 아트/혼합 DPI/장기 비용/배포 미검증 한계를 기록했다. 제품 수치나 최종 아트 승인으로 확정하지 않는다.

## 후속 적용

[TECH-016](TECH-016-scp-activation-and-shared-world.md)이 공용 대상 소유권·SCP별 활동·봉쇄 연결을 확장한다. 이 문서의 기존 검증은 당시 구현의 증거로 보존한다.
