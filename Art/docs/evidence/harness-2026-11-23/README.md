# Art 하네스 구축 기록

- 날짜: 2026-11-23.
- 요청: Doom_Inc와 C:/Dev/Unity/Art의 Aseprite 하네스를 참고해 프로젝트 최상단 Art를 생성.
- 추가 지시: 아트 지침은 루트 docs/agents가 아닌 Art 내부에서 관리.
- 결정: [TECH-004](../../../../docs/decisions/TECH-004-art-workspace.md).

## 확인한 참고 원본

- C:/Dev/Unity/Doom_Inc/AGENTS.md 및 docs/agents/art-assets.md.
- C:/Dev/Unity/Art/AGENTS.md, docs/agetns/workflow.md, project-structure.md.
- 참고 Art의 docs/plans/AGENTS.md·revisions.md.
- 참고 Art의 docs/agetns/production/aseprite.md·aseprite-mcp.md·export-and-handoff.md·verification.md.
- 참고 Art의 디자인·캐릭터·프레임 정렬·태그·시간 지침.

참고 프로젝트의 docs/agetns 철자는 그 프로젝트의 지정이다. 여기서는 Art/docs/agents를 사용한다.
참고 경로는 조사 출처이며 새 하네스 실행 시 해당 프로젝트가 존재해야 하는 의존성이 아니다.

## 적용과 검증

- Art MAP·9개 지침 문서·에셋 목록·명세/리비전/전달 양식 생성.
- 작업 source와 제출 revisions/rNNN/source·export·review의 저장 계약 정의.
- 프로젝트 PLAN/TECH 통합 유지. 기존 PLAN 상태 변경·새 제작 PLAN 활성화 없음.
- 문서 검사기를 확장해 Art 필수 문서·MAP/지침 길이·링크·별도 PLAN 저장소를 확인한다.
- [Aseprite status 원본](aseprite-status.json): batch 응답 성공, Live 미연결, Bridge 미설치 응답.
- 도구 기본 workDir이 C:/Dev/Unity/Art이므로 현재 프로젝트 절대 경로 지정 규칙 추가.
- 검사 명령: 프로젝트 루트에서 node --test scripts/validate-agent-docs.test.mjs 및 node scripts/validate-agent-docs.mjs.
- 검사기 회귀 테스트: 16개 통과, 실패·건너뜀 0개, 종료 코드 0. [관찰 결과](validation-results.json).
- 문서 검사: 필수 문서·로컬 링크·길이·PLAN/TECH 목록 검사 통과. 최종 출력은 docs-validation.log에 저장했다.
- Art 내부 작업 PLAN 우회와 개발/아트 PLAN 동시 active를 거부하는 회귀 시나리오도 통과했다.

## 범위와 한계

문서·관리 검사기·도구 상태 확인 작업이다. 실제 .aseprite·PNG·애니메이션은 제작하지 않았다.
Live 연결·Godot 임포트·실제 아트 품질은 미검증이다. 전역 MCP 설정·참고 프로젝트 파일·기획 원본은 변경하지 않았다.
루트 AGENTS.md는 Art MAP 링크만 추가했고 아트 세부 지침은 Art 내부에 둔다.
