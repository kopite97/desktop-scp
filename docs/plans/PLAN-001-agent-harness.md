---
id: PLAN-001
type: development
status: completed
title: 문서 하네스와 문서 검사기
updated: 2026-11-23
---

# PLAN-001 문서 하네스와 문서 검사기

## 목표와 범위

- 목표: 에이전트가 필요한 지침과 현재 작업을 찾고, 문서·PLAN·기술 결정의 구조적 오류를 검사할 수 있게 한다.
- 포함: 루트 MAP, 작업 지침 8종, 기획 MAP·로드맵, PLAN/TECH 템플릿·목록, 후속 proposed PLAN, 문서 검사기와 회귀 테스트.
- 제외: Godot 설치·프로젝트 생성·게임 코드·Windows 실행 하네스·Git 초기화·내보내기·고부하 측정.
- 요청 근거: 2026-11-23 사용자가 제안 구조로 하네스를 작성하고 PLAN-001부터 구현하며 후속 PLAN은 proposed로 작성하도록 명시했다.

## 기준 문서와 선행 조건

- [기획 원본](../../SCP_site-desktop.md), [로드맵](../game-design/roadmap.md).
- [작업 절차](../agents/workflow.md), [PLAN 관리](../agents/plan-management.md), [프로젝트 구조](../agents/project-structure.md), [검증](../agents/verification.md).
- 참고: C:/Dev/Unity/Doom_Inc의 문서 분리·공통 명령 하네스 원칙. Unity·Android·별도 승인 규칙은 자동 복제하지 않는다.
- 선행 PLAN 없음. 기존 폴더에는 기획 원본만 있었으며 Git 저장소가 아니었다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다.

| 산출물 | 경로 | 상태 |
| --- | --- | --- |
| 루트 MAP | `AGENTS.md` | 생성 |
| 운영 지침 | `docs/agents/workflow.md`, `plan-management.md`, `project-structure.md`, `architecture.md`, `godot-tools.md`, `windows-integration.md`, `harness.md`, `verification.md` | 생성 |
| 기획 안내 | `docs/game-design/README.md`, `docs/game-design/roadmap.md` | 생성 |
| PLAN 체계 | `docs/plans/README.md`, `docs/plans/_template.md`, 본 PLAN 및 목록의 PLAN-002~008 파일 | 생성 |
| 기술 결정 체계 | `docs/decisions/README.md`, `_template.md`, `TECH-001-document-harness.md` | 생성 |
| 검사기와 회귀 테스트 | `scripts/validate-agent-docs.mjs`, `scripts/validate-agent-docs.test.mjs` | 생성 |
| 검증 증거 | `docs/plans/evidence/PLAN-001/run-001/README.md`, `doc-validation-active.log`, `validator-tests.log` | 생성 |
| 상태 전환 후 검사 | `docs/plans/evidence/PLAN-001/run-001/doc-validation.log` | 생성·통과 |
| 증거 문서 연결 후 검사 | `docs/plans/evidence/PLAN-001/run-001/record-validation.log` | 기록 마감 시 검사 로그 |

run-001부터 사용하고 별도 검증 회차는 번호를 증가시킨다. 기존 증거를 덮어쓰지 않는다.

## 작업 목록

- [x] 루트 MAP과 작업 지침 작성
- [x] 기획 MAP·로드맵·PLAN/TECH 체계와 후속 proposed PLAN 작성
- [x] 구조 검사기·오류 탐지 회귀 테스트 구현
- [x] 검사·내용 검토·원본 보존 확인과 증거 기록
- [x] PLAN 완료·목록·로드맵 일치 확인

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 루트는 MAP만, 지침은 주제별 분리 | 내용 검토·줄 수 검사 | 검증 README·문서 검사 로그 |
| PLAN 메타데이터·목록·단일 active·후속 proposed 일치 | 문서 검사·내용 검토 | 문서 검사 로그 |
| TECH ID·메타데이터·목록 일치 | 문서 검사 | 문서 검사 로그 |
| 끊어진 로컬 파일 링크와 주요 문서 오류 탐지 | 실제 문서 검사·정상/오류 fixture 테스트 | 검사기 테스트 로그 |
| 기획 원본 보존·제품 구현 상태 과장 없음 | 해시 비교·내용 검토 | 검증 README |

## 현재 상황과 다음 행동

- 현재: 문서 체계·검사기·오류 탐지 테스트를 완료했다. Godot·제품 코드는 구현하지 않았다.
- 다음 행동: 현재 active 없음. 후속 PLAN-002는 proposed이며 실행 요청 시 환경 확인부터 진행한다.
- 막힌 점: 없음.

## 검증 결과

| 날짜 | 환경·대상·방법 | 실제 결과·판정 | 증거·미검증 |
| --- | --- | --- | --- |
| 2026-11-23 | Windows·Node v26.13.0, 문서 검사기 실행 | PLAN 8개·TECH 1개·메타데이터/목록/링크 통과 | [실행 증거](evidence/PLAN-001/run-001/README.md) |
| 2026-11-23 | node:test 정상·오류 fixture | 10개 통과, 실패·건너뜀 0 | [테스트 로그](evidence/PLAN-001/run-001/validator-tests.log) |
| 2026-11-23 | 완료 상태 전환 후 문서 검사 | active 0개, PLAN-001 completed·후속 7개 proposed, 구조 검사 통과 | [상태 전환 후 로그](evidence/PLAN-001/run-001/doc-validation.log) |
| 2026-11-23 | 문서 내용 검토·원본 SHA-256 대조 | 루트 MAP 28줄, 지침 역할 분리, 기획 원본 해시 동일 | 실행 증거의 검토 기록 |

문서 검사 결과는 제품 구현 검증이 아니다. Godot 설치·실행·Windows 동작·게임 기능·성능은 미검증이다.
검사기는 승인 근거·완료 증거의 의미·제목 앵커·웹 링크를 판정하지 않는다.

## 관련 결정과 상태 전환

- [TECH-001 문서 하네스](../decisions/TECH-001-document-harness.md).
- 2026-11-23: proposed로 작업 범위를 정리한 뒤, 같은 요청의 PLAN-001 구현 지시를 근거로 active 전환.
- 2026-11-23: active → completed. 문서 체계·검사기 구현과 정상/오류 검증 완료. 목록·로드맵도 함께 갱신.
- 후속 PLAN 실행은 이번 요청에 포함되지 않는다.
