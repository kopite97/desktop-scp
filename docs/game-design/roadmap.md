# 개발 로드맵

기준: [기획 MAP](README.md), [작업 상태](../plans/README.md).

원본의 Phase 1~4 앞에 문서 하네스·환경 확인을 둔다.
아래 완료 기준은 작업 분할을 위한 기준이며, 후속 PLAN 실행 승인이나 검증 완료를 뜻하지 않는다.
최소 숨김·종료 경로를 오버레이 단계에서 확인하고 정식 봉쇄 UI·트레이·연출은 마지막 단계에서 다룬다.

| 단계 | 결과와 완료 기준 | 작업 PLAN | 단계 판정 |
| --- | --- | --- | --- |
| 문서 하네스 | MAP·지침·템플릿·상태/링크 검사와 검사기 회귀 검증 | [PLAN-001](../plans/PLAN-001-agent-harness.md) | 완료 |
| 개발 환경 | 버전·프로젝트 위치·최소 C# 실행·검증 절차 확인 | [PLAN-002](../plans/PLAN-002-development-environment.md) | 완료 |
| Phase 1 | 실제 투명 표시·분리된 입력 영역·빈 영역 통과·좌표·즉시 숨김 | [PLAN-003](../plans/PLAN-003-overlay-validation.md) | 완료 (환경 한계는 PLAN 참조) |
| Phase 2 | 창 발판·이동·최소화/종료·낙하·착지·도약 검증 | [PLAN-004](../plans/PLAN-004-window-platforms.md) | 완료: 공통 물리·LG 실제 창 검증, 개별 SCP 통합은 Phase 3 |
| Phase 3 / 999 | 추적·타이핑·쓰다듬기·공통 스트레스 감소 | [PLAN-005](../plans/PLAN-005-scp-999.md) | 완료: LG 창 물리·행동·입력·효과 통합, 점액 정식 아트 적용도 PLAN-010 완료 |
| Phase 3 / 049 | 가짜 대상 생성·치료·좀비 변이와 수명 | [PLAN-006](../plans/PLAN-006-scp-049.md) | 완료: 당시 임시 표현으로 기능·LG 입력 검증. 실제 아이콘은 PLAN-012, 본체 정식 적용은 PLAN-013, 승인된 ART-006/007 통합은 PLAN-014 완료 |
| Phase 1·3 후속 수정 | 개체 겹침·드래그 중 안정된 창 순서와 입력 | [PLAN-011](../plans/PLAN-011-overlay-stacking.md) | 완료: 공통 창 순서·드래그 우선순위와 LG 겹침/입력 회귀 검증 |
| Phase 3 / 실제 아이콘 복제 | 실제 Desktop 아이콘 취득·캐시·게임 복제본·049 연결·입력/수명 검증 | [PLAN-012](../plans/PLAN-012-desktop-icon-clones.md) | 완료: 실제 아이콘 복제·LG 입력·사용자 화면 승인. 한계는 PLAN-012 참조 |
| Phase 3 / 좀비·효과 정식 적용 | 승인된 두 발·치료/변이 오라·소멸 수명·알파 입력 연결 | [PLAN-014](../plans/PLAN-014-zombie-art-integration.md) | 완료: r001 정식 적용·LG 입력/렌더·보행/깜빡임 수정·사용자 제품 시각 승인 |
| Phase 3 / 173 | 시선 정지·관찰 해제·순간이동·가짜 대상 파손 | [PLAN-007](../plans/PLAN-007-scp-173.md) | 완료: 임시 173·관찰/파손·실제 아이콘·LG 검증 |
| Phase 4 | 시스템 지표·강한 연출·봉쇄/복귀·트레이/단축키 통합 | [PLAN-008](../plans/PLAN-008-system-options.md) | 완료: 실제 지표·LG 소유 창 효과/복원·봉쇄/단축키·트레이 통합 검증 |
| Phase 4 후속 | 트레이 상주·제어창 숨김/종료 분리·명확한 봉쇄 안내·로컬 EXE | [PLAN-015](../plans/PLAN-015-tray-residency.md) | 완료: Windows 동작·사용자 실제 트레이 메뉴 확인·최종 EXE 및 배너 렌더 검증. 부트 표면 한계는 PLAN 참조 |
| Phase 3·4 후속 | SCP별 ON/OFF·설정 저장·공용 대상 분리·등록 기반 수명 관리 | [PLAN-016](../plans/PLAN-016-scp-activation.md) | 완료: SCP별 설정·공용 대상 분리·Windows/EXE 복원 검증. 범위·한계는 PLAN 참조 |

| 구조 개선 후속 | 공통 FSM·환경/이동 공유, SCP별 정책·월드·표현·수명 분리와 049 복구/999 배회 | [PLAN-017](../plans/PLAN-017-shared-behavior-and-decoupling.md) | 완료: 로직·LG 입력·EXE 메뉴/설정 검증. 초기 부트 포커스·비용 측정 제한은 PLAN 참조 |

| 다중 모니터 후속 | 049 중심 아이콘 생성과 공통 포털 전이 | [PLAN-018](../plans/PLAN-018-monitor-portals-and-local-spawning.md) | 완료: 모의/실제 두 모니터·입력·최종 EXE 사용자 확인. 혼합 DPI 등 한계는 PLAN 참조 |

승인된 순차 개발 PLAN-012 → PLAN-007 → PLAN-008 완료. ART-004 r001 정식 본체 적용은 [PLAN-013](../plans/PLAN-013-scp-049-art-integration.md) 완료. ART-006/007 r001도 제작·시각 승인을 완료했으며, 정식 적용은 [PLAN-014](../plans/PLAN-014-zombie-art-integration.md)에서 정식 적용·기술 검증·보행/깜빡임 수정과 사용자 제품 시각 승인을 완료했다.

## 단계 판정

- 단계는 PLAN의 증거로 판정한다. 문서 작성·컴파일·모의 검증만으로 실제 Windows 동작 완료를 선언하지 않는다.
- PLAN 진행의 원본은 PLAN 본문이다. 여기에는 단계 결과와 링크만 둔다.
- 후속 PLAN은 각자의 범위·선행 조건·완료 기준을 활성화 전에 보완한다.
- 성장·해금·실제 파일 정리 등 원본 프로토타입 범위 밖의 기능을 추가하지 않는다.
- 최종 제품 완료는 구현된 동작과 실제 환경 검증, 남은 제약을 종합해서 판단한다.
