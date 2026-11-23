---
id: TECH-004
status: superseded
title: 프로젝트 내부 Art 작업 공간과 Aseprite 제작 하네스
updated: 2026-11-23
---

# TECH-004 프로젝트 내부 Art 작업 공간과 Aseprite 제작 하네스

통합 PLAN·Art PLAN 금지 결정은 [TECH-005](TECH-005-separate-art-plans.md)로 대체했다.
Art 내부 지침·리비전 보존·Aseprite 절대 경로 운영은 유지한다. 아래는 최초 결정 당시 기록이다.

- 근거: 2026-11-23 사용자 요청으로 Doom_Inc와 C:/Dev/Unity/Art를 참고해 프로젝트 최상단 Art를 생성한다.
- 추가 지시: 아트 지침은 프로젝트 docs/agents가 아니라 Art 디렉터리 안에서 따로 관리한다.
- 적용: [Art MAP](../../Art/AGENTS.md), [폴더 안내](../../Art/README.md), 프로젝트 문서 검사기.
- 대체 관계: 최초 아트 작업 공간 결정. TECH-001의 MAP·PLAN·TECH 역할 분리를 유지한다.
- 관련 PLAN: 이번 요청은 문서·도구 하네스 관리다. 기존 PLAN의 활성화·아트 제작·게임 적용을 포함하지 않는다.

## 문제와 제약

참고 프로젝트는 Unity와 별도 Art 저장소로 제작·전달을 나누고 에셋별 제출 리비전을 보존한다.
현재 프로젝트는 Godot이며 기존 docs/plans의 사용자 승인·단일 active 규칙을 유지해야 한다.
현재 Aseprite MCP 기본 workDir은 C:/Dev/Unity/Art다. 상대 경로 저장 시 다른 프로젝트로 출력할 수 있다.

## 결정

- C:/Dev/scp_desktop/Art를 독립적인 아트 파일·지침 공간으로 둔다.
- Art/AGENTS.md는 MAP, Art/docs/agents는 제작·디자인·애니메이션·도구·리비전·전달·검수 지침으로 사용한다.
- 프로젝트 AGENTS.md에는 Art MAP 링크만 추가한다. 루트 docs/agents에 아트 세부 지침을 만들지 않는다.
- 실행 PLAN과 기술 결정은 프로젝트 docs/plans·docs/decisions에서 통합 관리한다. 별도 Art PLAN 저장소는 만들지 않는다.
- 작업용 source와 제출 revisions/rNNN/source·export·review를 분리한다. 실제 파일 생성 시 에셋 목록과 명세를 연결한다.
- Aseprite 입력·출력에 현재 Art 루트의 절대 경로를 사용한다. 전역 MCP 설정과 참고 프로젝트 파일을 수정하지 않는다.
- 기존 문서 검사기를 확장해 Art 문서·MAP·길이·로컬 링크와 별도 Art PLAN 생성을 검사한다.

## 대안과 선택 이유

- 참고 Art 전체 복사: 과거 에셋·승인·Unity PPU·광원·눈 표현이 현재 프로젝트 기준으로 오해될 수 있어 운영 원칙만 적용했다.
- 독립 Art PLAN/검사기: 같은 프로젝트 내 승인·active 상태 원본을 둘로 늘리므로 통합 PLAN과 검사기를 선택했다.
- 프로젝트 docs/agents에 아트 지침 추가: 사용자의 Art 내부 관리 지시에 따라 선택하지 않았다.

## 영향과 검증

- Aseprite batch 1.3.18.6/API 41/runtime 0.1.1 응답 확인. Live disconnected·extensionInstalled false를 현재 사실로 기록했다.
- Live Bridge 설치·연결, 정식 스프라이트 제작, PNG 출력, Godot 임포터는 이번 범위에 포함하지 않는다.
- 기획의 2등신·1px 다크 아웃라인·SCP별 외형을 반영하고 픽셀 크기·방향 수·팔레트 등 미정 값은 확정하지 않았다.
- 검사 결과와 참고 원본 목록은 [구축 기록](../../Art/docs/evidence/harness-2026-11-23/README.md)에 남긴다.
