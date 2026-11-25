# 기획 MAP

현재 제품 기획 원본은 [SCP: Site-Desktop 프로토타입 기획서](../../SCP_site-desktop.md)다. 2026-11-25 사용자 방향 변경의 상세 원본은 [실제 아이콘 복제 기획](desktop-icon-clones.md)에 분리했으며 고정 .tmp/쓰레기 외형을 대체한다.
원본의 API 이름·예시 수치는 구현 검증이나 확정된 세부 수치로 간주하지 않는다.

| 찾는 내용 | 기준 위치 |
| --- | --- |
| 장르·개발 환경·프로토타입 범위 | 원본 1절 |
| 아트 방향 | 원본 2절 |
| 실제 바탕화면 아이콘 취득·복제본 외형·원본 불변 | [아이콘 복제 기획](desktop-icon-clones.md) |
| 좀비 발 부착·치료/변이 효과·소멸 애니메이션의 후속 연결 | [아이콘 좀비 연결 계약](icon-zombie-presentation.md) |
| SCP-999·173·049 동작 | 원본 3절 |
| 창 물리·OS 환경 반응 | 원본 4절 |
| 클릭 투과·강한 연출·봉쇄 | 원본 5절 |
| 원본 개발 단계 | 원본 6절 |
| 하네스 선행 작업과 실행 단위 | [개발 로드맵](roadmap.md) |
| SCP별 ON/OFF·설정 저장·공용 대상 활동 | [SCP 활성화](scp-activation.md) |
| 실제 작업 상태 | [PLAN 목록](../plans/README.md) |

## 기획 유지

- 초기에는 원본을 보존하고 연결한다. 주제별 분리가 필요하면 내용을 이관하고 원본의 해당 내용을 링크로 대체한다.
- 동일한 제품 규칙을 여러 문서에서 따로 수정하지 않는다.
- 기술 결정은 구현 방법을 기록한다. 제품 동작·수치·범위를 조용히 확정하지 않는다.
- 예시값·미정·제안·확정값을 구분한다. 미정 항목은 관련 PLAN 활성화 시 확인한다.
- 원본과 작업 지침이 충돌하면 충돌을 기록하고 제품 동작을 임의로 바꾸지 않는다.

## 구현 전에 구체화할 항목

| 항목 | 확인할 내용 | 관련 PLAN |
| --- | --- | --- |
| 좌표·창 발판 | 배율, 가려짐, 최대화, 모니터 변경, 작업 영역 | [PLAN-003](../plans/PLAN-003-overlay-validation.md), [PLAN-004](../plans/PLAN-004-window-platforms.md) |
| 개체 수치 | 거리·속도·스트레스·시간·재사용 간격 | [PLAN-005](../plans/PLAN-005-scp-999.md)~[PLAN-007](../plans/PLAN-007-scp-173.md) |
| SCP-173 조건 | 다른 모니터 이동 시 즉시 행동인지, 타이머 관계 | [PLAN-007](../plans/PLAN-007-scp-173.md) |
| 지표 신호 | 마스터 볼륨 또는 실제 출력 레벨, CPU/RAM 반응 | [PLAN-008](../plans/PLAN-008-system-options.md) |
| 봉쇄 연출 | 즉시 숨김과 경보·셔터의 순서, 해제·시간 처리 | [PLAN-003](../plans/PLAN-003-overlay-validation.md), [PLAN-008](../plans/PLAN-008-system-options.md) |
| 외부 창 연출 | 대상 선정·취소·복원·사용자 이동과 충돌 | [PLAN-008](../plans/PLAN-008-system-options.md) |
| 비기능 기준 | 지원 Windows 환경·성능 목표·종료/재실행 기준 | [PLAN-002](../plans/PLAN-002-development-environment.md), [PLAN-008](../plans/PLAN-008-system-options.md) |

이 목록은 검토 사항이며 새로운 제품 기능이나 확정 수치를 추가하지 않는다.

시스템 지표·봉쇄·외부 창 효과의 현재 프로토타입 규칙은 [시스템 반응](system-reactions.md)에 기록한다.
