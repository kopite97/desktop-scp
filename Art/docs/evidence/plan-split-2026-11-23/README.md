# 아트 PLAN 분리 변경 기록

- 날짜: 2026-11-23
- 요청: 사용자가 개발 PLAN과 Art 내부 제작 계획의 분리 구조를 채택하고 반영을 요청했다.
- 결정: [TECH-005](../../../../docs/decisions/TECH-005-separate-art-plans.md).
- 관리: [Art PLAN 규칙](../../plans/plan.md), [개발 PLAN 규칙](../../../../docs/plans/plan.md).

## 적용 내용

- Art/docs/plans에 목록·관리 지침·템플릿을 추가했다. 실제 ART 작업은 아직 없다.
- ART-NNN ID·에셋/리비전 메타데이터와 개발/아트 각각 active 최대 1개를 도입했다.
- 명시적 사용자 승인 없이 active로 전환하지 않는 규칙을 양쪽에 유지했다.
- Art MAP·제작/리비전/전달 지침·양식·에셋 목록을 새 위치로 연결했다.
- TECH-004의 최초 결정과 초기 증거는 보존하고 TECH-005로 대체 관계를 기록했다.
- 기존 개발 PLAN 8개의 파일·상태는 유지했다. 제품 코드·이미지·Aseprite 설정은 변경하지 않았다.

## 검증

- 검사기 회귀 테스트: `node --test scripts/validate-agent-docs.test.mjs` — 21개 통과, 실패·건너뜀 0개, 종료 코드 0. [로그](tests.log).
- 문서 검사: `node scripts/validate-agent-docs.mjs` — 통과, 종료 코드 0. [로그](docs-validation.log).
- 상태: 개발 PLAN 8개(active 0개), ART 0개(active 0개), TECH 5개.
- 검사 대상: 필수 문서·길이·PLAN/ART/TECH 메타데이터·목록·스코프별 active 수·로컬 링크.
- 승인 진위·픽셀 품질·Godot 제품 동작은 자동 검사 대상이 아니다. 이번 변경에서는 제품을 실행하지 않았다.
