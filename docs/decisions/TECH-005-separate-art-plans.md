---
id: TECH-005
status: accepted
title: ART 제작 계획과 개발 PLAN의 분리
updated: 2026-11-23
---

# TECH-005 ART 제작 계획과 개발 PLAN의 분리

- 근거: 2026-11-23 사용자가 Art/docs/plans와 ART-NNN ID, 개발·아트 각각 active 최대 1개 구조를 채택하고 변경을 요청했다.
- 대체: [TECH-004](TECH-004-art-workspace.md)의 통합 PLAN·Art PLAN 금지 결정을 대체한다.
- 유지: Art 내부 지침·작업 원본/제출 리비전 분리·Aseprite 절대 경로·프로젝트 공통 TECH 위치.
- 적용: [Art PLAN 관리](../../Art/docs/plans/plan.md), [개발 PLAN 관리](../plans/plan.md), [문서 검사기](../../scripts/validate-agent-docs.mjs).
- 이번 구조 채택은 특정 개발 PLAN이나 ART 제작 실행 승인이 아니다.

## 문제와 제약

아트 제작·피드백·시각 확인은 Godot 기능 개발·통합 검증과 다른 주기로 진행한다.
기존 프로젝트 전체 단일 active와 Art PLAN 금지는 제작 상황을 독립적으로 관리하기 어렵게 했다.

## 결정

- 개발/적용은 docs/plans의 PLAN-NNN, 아트 제작은 Art/docs/plans의 ART-NNN으로 관리한다.
- 두 스코프에서 각각 active 최대 1개를 허용한다. 0개도 정상이다.
- 새 작업은 proposed이며 사용자 명시적 승인 없이 active로 전환하지 않는다.
- ART는 type art와 asset_id·revision·approved_revision을 기록하고 목록과 동기화한다.
- 에셋 원본·제출본·리뷰는 Art/assets의 기존 리비전 구조에 유지한다.
- 개발 PLAN과 ART는 경로·AssetId·리비전으로 연결하고 실제 적용과 제작 완료를 구분한다.
- 기술 결정은 기존 docs/decisions에서 계속 관리한다.
- 하나의 문서 검사기로 두 스코프의 메타데이터·목록·active 제한·문서 길이·링크를 검사한다.

## 대안과 선택 이유

- 기존 통합 PLAN 유지: 사용자가 아트 독립 관리를 요청하여 대체한다.
- 두 목록에서 모두 PLAN-NNN 사용: 대화·전달 기록의 혼동을 줄이기 위해 ART 접두사를 선택했다.
- 별도 검사기 복제: 공통 검사 규칙이 갈라지지 않도록 기존 검사기를 확장했다.

## 영향과 검증

- 기존 개발 PLAN 8개는 이동하거나 재번호화하지 않는다. 이관할 실제 아트 PLAN은 없었다.
- ART 목록은 작업 없는 상태로 시작한다. 예시 에셋·새 후보·활성 작업을 자동 생성하지 않는다.
- 기존 TECH-004와 초기 구축 증거는 과거 결정으로 보존한다.
- 검사기 회귀 테스트에서 개발 1개+ART 1개 동시 active 허용과 각 스코프 2개 거부를 확인한다.
- 승인 진위·시각 품질은 자동 검사 대상이 아니다. 실제 검증 결과는 [변경 기록](../../Art/docs/evidence/plan-split-2026-11-23/README.md)에 남긴다.
