# 기술 결정 MAP

영향이 큰 기술 선택의 이유·대안·적용 위치·대체 관계를 기록한다.
작업 진행·실제 검증 결과의 원본은 [개발 PLAN](../plans/README.md)과 [제작 ART](../../Art/docs/plans/README.md)다.

## 기록 규칙

- 여러 기능에 영향을 주거나 변경 비용이 큰 선택을 기록한다. 작은 구현 선택은 PLAN에 남긴다.
- [_template.md](_template.md)를 사용해 `TECH-NNN-topic.md`로 작성한다.
- 필수 메타데이터는 id·status·title·updated다. 날짜는 YYYY-MM-DD다.
- 상태는 proposed(검토 중), accepted(채택), superseded(새 결정으로 대체)다. PLAN 상태와 구분한다.
- accepted는 기술 선택의 채택이다. 제품 구현·검증 완료를 뜻하지 않는다. 실제 적용 여부와 위치를 본문에 별도로 적는다.
- 결정 변경 시 새 문서와 이전 문서에 대체 관계를 연결한다. 과거 선택 이유를 지우지 않는다.
- 기준 기획·관련 PLAN·결정·대안·영향·검증 방법을 적는다.
- 제품 동작·수치를 기술 결정으로 확정하지 않는다.
- TECH ID는 PLAN ID와 독립적이다. 목록의 원본은 각 TECH 메타데이터다.

## 결정 목록

| 결정 | 주제 | status |
| --- | --- | --- |
| [TECH-001](TECH-001-document-harness.md) | 문서 역할 분리와 Node 기반 구조 검사 | accepted |
| [TECH-002](TECH-002-development-toolchain.md) | Godot 4.7.2 .NET·기존 SDK·최소 실행/검증 경로 | accepted |
| [TECH-003](TECH-003-overlay-islands.md) | 분리된 네이티브 오버레이·입력 윤곽·물리 좌표 경계 | accepted |
| [TECH-004](TECH-004-art-workspace.md) | 프로젝트 내부 Art 작업 공간과 Aseprite 제작 하네스 | superseded |
| [TECH-005](TECH-005-separate-art-plans.md) | ART 제작 계획과 개발 PLAN의 분리 | accepted |
| [TECH-006](TECH-006-lighting-aware-art.md) | 동적 조명을 고려한 아트 제작과 샘플 검증 | accepted |
| [TECH-007](TECH-007-unlit-outline-and-reactions.md) | 본체 외곽선과 반응 효과의 조명 제외 | accepted |
| [TECH-008](TECH-008-companion-state-and-input.md) | 공통 행동 상태와 활동 입력·장식 효과 창 | accepted |
| [TECH-009](TECH-009-window-platform-physics.md) | DWM 창 관찰과 공통 일방향 발판 물리 | accepted |
| [TECH-010](TECH-010-companion-platform-integration.md) | SCP-999 공통 물리 위치와 지지면 부착 효과 | accepted |
| [TECH-011](TECH-011-plague-objects-and-reservations.md) | 가짜 데이터 수명과 SCP-049 예약·치료 상태 | accepted |
| [TECH-012](TECH-012-overlay-stacking.md) | 단일 제품 창 순서 소유자·드래그 우선순위 | accepted |
| [TECH-013](TECH-013-runtime-icon-appearance.md) | Shell 아이콘 작업 프로세스와 세션 외형 캐시 | accepted |
| [TECH-014](TECH-014-system-options-and-lockdown.md) | 시스템 지표 관찰과 단일 봉쇄·외부 효과 소유자 | accepted |
| [TECH-015](TECH-015-zombie-presentation-lifetime.md) | 좀비 표시 수명과 승인 발·효과의 게임 시계 연결 | accepted |
| [TECH-016](TECH-016-scp-activation-and-shared-world.md) | SCP 등록·활성 상태와 공용 오브젝트 소유권 분리 | accepted |
| [TECH-017](TECH-017-shared-behavior-and-runtime.md) | 개체별 공통 FSM과 환경·이동·표현 책임 분리 | accepted |
| [TECH-018](TECH-018-monitor-portals-and-spawn-policy.md) | 모니터 전이 상태와 중립 생성 위치 정책 | accepted |
