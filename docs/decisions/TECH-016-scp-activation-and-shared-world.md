---
id: TECH-016
status: accepted
title: SCP 등록·활성 상태와 공용 오브젝트 소유권 분리
updated: 2026-11-25
---

# TECH-016 SCP 등록·활성 상태와 공용 오브젝트 소유권 분리

근거: 사용자 승인 [PLAN-016](../plans/PLAN-016-scp-activation.md), [제품 규칙](../game-design/scp-activation.md).

## 문제와 제약

- Main/SystemController가 SCP를 직접 열거하고 173이 049 컨트롤러와 대상 상태에 의존했다. 일괄 표시 복원은 개별 OFF를 덮어쓰며 표시만 숨기면 스트레스와 외부 효과가 계속될 수 있었다.

## 결정

- ScpCatalog의 안정된 ID·표시 정보·기본값·생성 함수·공용 대상 사용 여부로 모듈을 등록한다. ScpActivationManager가 사용자 Enabled와 실제 Active를 소유한다. 전체 봉쇄 원본은 기존 SystemOptions에 유지한다.
- IScpModule은 수명·상태·지표 반응·리셋·종료에만 사용한다. AI/물리/표현은 기존 컨트롤러가 소유하며 Main의 조립은 카탈로그로 모은다. 중복 ID는 생성 전에 거부한다.
- DesktopObjectState/Controller가 공용 대상의 ID·예약·생성·수명·물리·창·아이콘 참조를 소유한다. 049의 치료와 173의 파손은 이 상태에 명령한다. 173은 049 컨트롤러를 알지 않는다.
- PlagueState/Controller는 049 상태·본체만 소유한다. 기존 진단용 Spawn/Collect/Tick/Publish는 공용 소유자에 위임한다. 제품 자동 진행에서는 공용 대상과 의사를 각 소유자가 한 번씩 갱신한다. 별도 진단용 게임 시뮬레이션은 없다.
- 좀비 OwnerId는 기존 런타임 scp049, 설정 ID는 scp-049다. 등록에서 이를 연결한다. 공용 Clock·좀비 ZombieClock·의사 Clock을 분리해 독립 정지 후 애니메이션이나 수명이 건너뛰지 않게 한다.
- 전환 이유를 구별하여 전체 봉쇄는 치료 진행 보존, 개별 OFF는 예약/행동/일시 표현 정리를 적용한다. 스트레스는 값과 Active를 분리한다.
- 외부 효과의 시작 소유자를 SystemController가 보관하고 활동 자격을 잃으면 WindowsWindowShake.Cancel로 취소한다. 매 이동의 옵션·대상 유효성 검사는 기존 Windows 경계에 남는다.
- ScpSettingsStore는 파일 입출력, ScpOptionsPanel은 동적 체크·상태를 담당한다. UI와 진단은 같은 SetEnabled 경로를 쓰며 진단 저장 경로는 격리한다.
- 정상 저장은 같은 디렉터리 임시 파일 후 교체한다. 손상/상위 버전 원본은 보호하고 별도 복구 파일을 사용한다. 미래 ID는 실행하지 않으면서 키는 보존한다.

## 대안과 선택 이유

- 숨김만 추가하면 게임 상태·외부 효과가 멈추지 않는다. 생성 플래그만 바꾸면 재실행이 필요하고 173 의존성이 남는다.
- ON/OFF마다 재생성하는 대신 세션 인스턴스를 유지해 상태 보존과 구독·창 정리를 명시적으로 처리한다.
- 범용 플러그인 로더·ECS·모든 클래스의 인터페이스화는 도입하지 않는다. 새 상호작용에는 해당 서비스 확장이 필요할 수 있다.

## 영향과 검증

- [TECH-011](TECH-011-plague-objects-and-reservations.md)의 공용 대상 소유권을 분리한다. 단일 예약·원자적 변이·원본 파일 불변은 유지한다.
- [TECH-014](TECH-014-system-options-and-lockdown.md)의 일괄 봉쇄 연결을 등록 기반으로 교체한다. 트레이/제어창/종료 규칙은 유지한다.
- [TECH-015](TECH-015-zombie-presentation-lifetime.md)의 표시 전용 수명·텍스처 참조 보존을 유지하며 개별 OFF 취소를 추가한다. 과거 증거는 당시 사실로 보존한다.
- 실제 검증과 남은 항목은 PLAN-016에서 관리한다. 기술 문서 작성 자체를 기능 검증 통과로 간주하지 않는다.
