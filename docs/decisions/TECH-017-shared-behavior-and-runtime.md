---
id: TECH-017
status: accepted
title: 개체별 공통 FSM과 환경·이동·표현 책임 분리
updated: 2026-11-26
---

# TECH-017 개체별 공통 FSM과 환경·이동·표현 책임 분리

- 결정일·근거: 2026-11-25 사용자 [PLAN-017](../plans/PLAN-017-shared-behavior-and-decoupling.md) 실행 승인.
- 기획: [행동 계약](../game-design/scp-behavior.md), [활성화](../game-design/scp-activation.md).
- 적용: 구현 및 관련 회귀 완료. 실제 Windows/EXE의 통과 범위와 초기 부트 포커스 제한은 PLAN에서 관리한다.

## 문제와 제약

행동·물리·표현의 혼합, 반복 환경 관찰, 049 정체와 999 배회 부재를 해결한다. 기존 트레이·개별 활성화·예약·물리·승인 아트를 보존한다.

## 결정

- 공통 StateMachine·IdleState·MovingState를 조합하되 개체마다 인스턴스를 소유한다. 변경 가능한 현재 상태/타이머/예약은 공유하지 않는다. 전환 요청은 갱신 경계에서 적용하고 기록은 개체당 최근64개로 제한한다.
- 049/999/173/좀비 정책은 Behavior의 개체별 디렉터리에 둔다. 공통 상태는 SCP ID를 모르며 173에는 일반 보행을 강제하지 않는다.
- PlatformWorld가 위치·충돌을 소유한다. 실제 보행 거리/속도와 발판 운반을 분리하고 동일 관찰의 Revision 증가를 제거한다. PlatformNavigator는 기존 점프 착지 검사와 하강 연결을 이용하는 제한된 발판 그래프다.
- WindowsDesktopEnvironmentSource가 관찰을 모으고 ScpRuntime이 등록 순서로 월드/개체를 갱신한다. 같은 프레임 요청은 동일 관찰을 사용한다. 정책 코드는 네이티브 호출을 하지 않는다.
- CompanionAnimation이 클립 시간·준비 신호를 소유한다. 공용 치료 표시는 TreatmentPresentationState를 읽고 Doctor 상태 전체를 참조하지 않는다.
- DesktopObjectSettings/ZombieSettings와 행동 모듈을 분리한다. 최종 예약/파손/변이는 공용 월드 명령 경계에서 검사한다. 생성 문맥은 Main 대신 필요한 서비스와 조립 함수를 받는다.
- 개별 OFF/봉쇄/복귀 이유를 명시한다. 기존 봉쇄 보존·개별 OFF 취소 계약은 TECH-016을 유지한다. 네이티브 창은 ActorWindowLifetime과 기존 단일 스택 소유자를 사용한다.
- 제품 컨트롤러가 공용 월드를 암묵적으로 Tick/Publish하는 진단 호환 경로를 제거한다. Diagnostics의 시나리오 조정은 동일 제품 명령을 각 소유자에게 한 번씩 전달한다.

## 대안과 선택 이유

하나의 전역 현재 상태는 동시 행동을 막는다. 거대한 SCP 상속 클래스는 특수 행동마다 예외를 늘린다. 작은 상태 조합과 교체 가능한 환경 경계를 선택하며 DI/ECS/일괄 인터페이스화는 도입하지 않는다.

## 영향과 검증

TECH-009의 물리, TECH-011의 예약, TECH-015의 표시 수명, TECH-016의 활성화를 유지하면서 행동·관찰·조립 책임을 보완한다. 공통화 자체를 CPU 개선으로 간주하지 않는다. 로직·실제 입력·비용·패키징을 구분하고 실제 결과와 한계는 PLAN-017에 기록한다.
