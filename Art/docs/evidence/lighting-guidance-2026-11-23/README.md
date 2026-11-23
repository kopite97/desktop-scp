# 조명 제작 지침 반영 기록

- 날짜: 2026-11-23. 사용자가 앞으로 조명 준비를 반영하도록 지침 수정을 요청했다.
- 규칙 원본: [조명과 재질](../../agents/lighting.md), [결정 TECH-006](../../../../docs/decisions/TECH-006-lighting-aware-art.md).
- 변경: 조명 방식 사전 명세, 기본색/보조 맵 역할, 대표 샘플 엔진 검증 후 전체 확장, 맵/프레임/반전 계약, Art와 Godot 책임·시간대·성능 검증, 완료 구분.
- 연결: Art MAP·제작/디자인/애니메이션/전달/검증 지침·ART 관리·양식·현재 ART-001~003·PLAN-005.
- 루트 MAP은 링크만, 루트 개발 지침은 Art의 계약 참조만 추가했다. 아트 규칙 원본을 루트 docs/agents에 복제하지 않았다.
- 기존 r001 이미지·원본·제출 증거는 보존했다. spec에는 현재 조명 준비 미완료 상태만 보완했다.
- ART-001 active·approved_revision 0, ART-002/003 proposed, 개발 PLAN-005 proposed 유지. 새 제작/개발 PLAN 실행 없음.
- 문서 검사기에 Art/docs/agents/lighting.md를 필수 문서로 추가했다. 조명 품질이나 승인 사실을 자동 판정하는 검사로 확대하지 않았다.

## 검증

- 명령: node --test scripts/validate-agent-docs.test.mjs. 21개 통과, 실패/건너뜀 0, 종료 코드 0. [로그](tests.log).
- 명령: node scripts/validate-agent-docs.mjs. 문서/목록/길이/링크 검사 통과, 종료 코드 0. [로그](docs-validation.log).
- 편집/제출 Scp999.aseprite의 SHA256은 기존 run-001 결과와 동일한 B9FD48A960EC211C883A1A0C10AFFBFE6938A988A1CD64ADED2D60578EFBD2DA다.
- 실제 Godot 조명·성능·픽셀 품질 검증은 실행하지 않았다. 이번 결과는 지침/문서 하네스 변경 검증이다.
