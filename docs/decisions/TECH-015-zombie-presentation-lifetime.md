---
id: TECH-015
status: accepted
title: 좀비 표시 수명과 승인 발·효과의 게임 시계 연결
updated: 2026-11-25
---

# TECH-015 좀비 표시 수명과 승인 발·효과의 게임 시계 연결

- 결정일: 2026-11-25.
- 결정 주체·위임 근거: 사용자 승인 [PLAN-014](../plans/PLAN-014-zombie-art-integration.md)의 구현 범위에서 선택.
- 기준 기획: [아이콘 좀비 표현 계약](../game-design/icon-zombie-presentation.md).
- 적용: `GodotProject/Scripts/Simulation/ZombieAnimation.cs`, `PlagueState.cs`와 `Scripts/Overlay/PlaguePresentation.cs`, `PlagueEffectWindow.cs`, `ZombieSprite.cs`, `PlagueController.cs`, `DesktopIconAppearance.cs`.
- 대체 관계: TECH-011의 게임 상태, TECH-012의 창 순서, TECH-013의 캐시, TECH-014의 봉쇄 책임을 유지하며 표시 수명과 캐시 참조 범위를 확장한다. 기존 결정을 전체 대체하지 않는다.

## 문제와 제약

게임 객체를 즉시 제거하더라도 승인된 collect/expire의360/660ms 표현은 남아야 한다. 게임 삭제를 늦추면 중복 회수·예약·카운트에 영향을 주고, 텍스처를 먼저 해제하면 카탈로그 갱신 중 잔상이 사라진다.
발과 효과는 게임 성공·물리·봉쇄와 같은 시간을 따라야 하며 창의 Publish나 순서 갱신 횟수로 빨라져서는 안 된다.

## 결정

- PlagueState는 제거 이유와 마지막 위치·방향·외형 ID·발 자세의 스냅샷을 사건으로 발행한다. 논리 삭제·보상은 즉시 한 번만 수행한다.
- 컨트롤러 소유 PlaguePresentation은 좀비 회수/만료만 비상호작용 표시 객체로 유지한다. 살아 있는 객체와 잔상의 외형 ID 합집합을 캐시 Retain에 전달한다. 정상 데이터 제거·파손·종료는 좀비 소멸을 만들지 않는다.
- ZombieAnimation과 효과/소멸은 PlagueState.Clock에 연결한다. Publish는 시계를 전진시키지 않는다. 실제 Treating 진입 시각과 MutationAge를 사용하며 성공 때 spawn과 transform을 함께 시작한다.
- 봉쇄는 표시와 시계를 함께 멈춘다. 모니터 교체는 오래된 위치의 잔상을 정리하고, 종료는 재생 완료를 기다리지 않고 모든 표시·참조를 해제한다.
- 발/효과 텍스처와 알파 마스크는 컨트롤러의 PlagueArt가 공유한다. alpha16 아이콘/발 합집합을 입력에 사용하고, 효과와 잔상 창은 외부 프로세스로 완전 클릭 투과한다. 공통 창 순서 관리자에 등록한다.
- 승인 계약의 로컬 아이콘 아래끝y56·발바닥y64를 물리 foot에 맞춰 좀비 창 원점을 `foot-(32,64)`로 둔다. 일반 아이콘은 기존 `foot-(32,56)`을 유지한다. transform은 `foot-(48,80)`, 일반 아이콘의 treat는 표시 기준점 `foot+(0,8)`에서 같은 피벗을 뺀다. 아트/로고는 재표본화하지 않는다.

## 대안과 선택 이유

- 게임 객체를 소멸 종료까지 유지하는 방식은 상호작용·예약 정리를 복잡하게 하므로 채택하지 않았다.
- 영구 텍스처 보관 대신 실제 생존/표시 참조로 제한한다. 카탈로그에서 빠진 아이콘도 잔상 종료까지 유지하면서 종료 뒤 해제할 수 있다.
- 창별 타이머 대신 게임 시계를 사용해 봉쇄 복귀와 반복 Publish에서도 재생 속도를 보존한다.
- 아이콘 창을96px로 키우는 대신 별도 장식 창을 사용해 기존64px 입력 좌표와 효과의 작업 영역 마스크를 분리한다.

## 영향과 검증

- 승인 JSON의1기반 태그와 rect/cell을 공통 애니메이션 로더에서 변환한다. 기존0기반 frame/canvas 계약도 유지한다.
- 제품 시각 피드백으로 r001 move의 접지 이동 방향을 보정했다. ZombieAnimation만 move를16→9로 매핑하고 원본별 ms를 유지한다. 양발·양방향의 실제 시트 접지 픽셀 검사로 역보행 회귀를 검출한다. 원본 PNG/JSON은 변경하지 않았다.
- [PLAN-014 실행 기록](../plans/evidence/PLAN-014/README.md)에 해시·실제 RGBA/알파·게임 사건·캐시·LG 외부 입력·겹침·봉쇄·짧은 비용 결과를 기록한다.
- 혼합 DPI·실제 모니터 탈착·GPU 및 장기 성능 측정은 이번 실행 증거에 포함하지 않는다.

## 후속 적용

[TECH-016](TECH-016-scp-activation-and-shared-world.md)이 공용 대상 소유권·SCP별 활동·봉쇄 연결을 확장한다. 이 문서의 기존 검증은 당시 구현의 증거로 보존한다.
