# SCP: Site-Desktop Art

이 프로젝트의 Aseprite 작업 루트는 `C:/Dev/scp_desktop/Art`다.
시작은 [Art MAP](AGENTS.md), 세부 지침은 [Art 내부 지침](docs/agents/README.md)을 따른다.

## 구성

| 경로 | 역할 |
| --- | --- |
| AGENTS.md | 아트 작업 탐색 MAP |
| docs/agents | 아트 제작·도구·리비전·전달·검수 지침 |
| docs/plans | ART-NNN 제작 계획·승인·상태·진행·검증 기록 |
| assets/README.md | 실제 제작 에셋과 ART의 연결 목록 |
| templates | 에셋 명세·제출 리비전·Godot 전달 기록 양식 |
| docs/evidence | PLAN에 속하지 않는 하네스 관리 작업의 검증 기록 |

작업용 원본과 제출 보존본은 실제 제작 때 `assets/<category>/<AssetId>/` 아래에 만든다.
`palettes/`, `references/`는 채택한 파일이 생길 때 만든다. 빈 캐릭터 폴더나 가상 스프라이트는 생성하지 않는다.

## 프로젝트 문서와의 관계

- 아트 지침 원본은 Art 내부에만 둔다. 프로젝트 루트 docs/agents에 복제하지 않는다.
- 아트 제작은 [Art PLAN 목록](docs/plans/README.md), 개발·Godot 적용은 [개발 PLAN 목록](../docs/plans/README.md)에서 관리한다.
- [Art PLAN 관리](docs/plans/plan.md)에 따라 사용자 승인 없이 active로 전환하지 않는다. 개발·아트 각각 active 최대 1개다.
- 에셋 목록은 제작 ART를 연결하며, 상태 원본은 개별 ART 메타데이터다.
- 기술 결정은 프로젝트 docs/decisions에서 관리한다. 분리 결정은 [TECH-005](../docs/decisions/TECH-005-separate-art-plans.md)에 기록했다.
- 원본 제작 완료·사용자 시각 확인·Godot 적용 완료는 별도로 기록한다.

## 검사

프로젝트 루트: `node scripts/validate-agent-docs.mjs`
Art 루트: `node ../scripts/validate-agent-docs.mjs`

동일한 검사기가 Art MAP·필수 지침·문서 길이·로컬 링크와 PLAN/ART/TECH를 함께 검사한다.
현재 이 폴더는 제작 하네스다. SCP 정식 아트 제작이나 PLAN-005 실행 완료를 뜻하지 않는다.
