---
id: PLAN-002
type: development
status: completed
title: Godot/.NET 개발 환경과 최소 실행
updated: 2026-11-23
---

# PLAN-002 Godot/.NET 개발 환경과 최소 실행

## 목표와 범위

- 목표·포함: 설치된 Godot .NET·SDK 확인, 프로젝트 루트와 버전 선택, 최소 C# 실행·종료, 실제 검증 명령 기록.
- 제외: SCP 행동·Windows 창 연동·배포용 내보내기.
- 요청 근거: 2026-11-23 사용자 지시 “PLAN-002를 승인할게 작업 시작해줘.”에 따라 이 PLAN의 개발 환경 확인·최소 실행 작업을 승인받았다.

## 기준 문서와 선행 조건

- [기획 원본](../../SCP_site-desktop.md), [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md).
- 선행: [PLAN-001](PLAN-001-agent-harness.md).
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](../agents/plan-management.md), [관련 구현 지침](../agents/godot-tools.md), [검증](../agents/verification.md).
- 확인 결과: 공식 Godot 4.7.2 .NET을 프로젝트 로컬에 준비하고 기존 .NET SDK 8.0.318을 사용했다. 버전·배치·검증 명령은 TECH-002와 Godot 도구 지침에 기록했다.

## 결과물 저장 위치

기준 루트는 `C:/Dev/scp_desktop`이다. 실제 제품 배치는 아래와 같이 확정했다.

- 프로젝트: `GodotProject/project.godot`, `GodotProject/SiteDesktop.csproj`, `GodotProject/SiteDesktop.sln`, `GodotProject/global.json` 생성.
- 진입 씬·코드: `GodotProject/Scenes/App/Main.tscn`, `GodotProject/Scripts/App/Main.cs` 생성.
- 환경 검증 씬·코드: `GodotProject/Scenes/Diagnostics/EnvironmentSmoke.tscn`, `GodotProject/Scripts/Diagnostics/EnvironmentSmoke.cs` 생성.
- 임포트가 생성한 각 `.cs.uid`도 원본으로 보존한다.
- 도구: `scripts/godot.ps1`, `scripts/tool-paths.example.json`, 로컬 전용 `scripts/tool-paths.local.json` 생성.
- 생성물 제외: 루트 `.gitignore` 생성. Git 저장소는 초기화하지 않는다.
- Godot 배치: `.tools/godot/Godot_v4.7.2-stable_mono_win64/`에 공식 휴대용 배포본 준비. 기존 Unity SDK를 읽기 전용으로 재사용한다.
- 작업 기록: `docs/plans/PLAN-002-development-environment.md` 갱신.
- 검증 색인: `docs/plans/evidence/PLAN-002/run-001/README.md`.
- 검증 결과: `docs/plans/evidence/PLAN-002/run-001/results.json`.
- 실행 로그: `docs/plans/evidence/PLAN-002/run-001/environment.log`, `build.log`, `import.log`, `smoke-headless.log`, `windowed.log`, `smoke-restart.log`. 문서 최종 검사는 `doc-validation.log`에 기록한다.
- 시각 확인: `docs/plans/evidence/PLAN-002/run-001/windowed.png`.
- 검증 회차는 run-001부터 증가시키며 기존 증거를 덮어쓰지 않는다. 선택 자료가 미생성인 경우 이유를 기록한다.
- 여러 기능에 영향을 주는 기술 선택이 생기면 다음 미사용 TECH ID로 기록한다.

## 작업 목록

- [x] 실행 요청·선행 결과 확인, 미정 규칙·제품 파일 경로 확정
- [x] 범위의 최소 C# 진입 씬과 환경 검증 경로 구현
- [x] 아래 필수 검증·실제 결과·미검증 한계 기록

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 증거 |
| --- | --- | --- |
| 설치 버전·도구 경로·지원 조합 확인 | 로컬 환경 조사와 해당 버전 공식 문서 확인 | 해당 실행 README·결과·로그 |
| 최소 C# 프로젝트가 실행·종료됨 | 실제 에디터/프로젝트 실행·컴파일 결과 확인 | 해당 실행 README·결과·로그 |
| 실행과 검증 경로 재현 가능 | docs/agents/godot-tools.md에 실제 명령·환경 기록 | 해당 실행 README·결과·로그 |

## 현재 상황과 다음 행동

- 현재: 개발 환경·최소 C# 실행·검증 절차와 증거 기록을 완료했다. 테스트 앱은 정상 종료했다.
- 다음 행동: active 0개로 대기한다. PLAN-003은 proposed이며 별도 실행 승인을 받은 뒤 진행한다.
- 막힌 점: 없음.

## 검증 결과

| 날짜 | 환경·대상·방법 | 실제 결과 | 증거 |
| --- | --- | --- | --- |
| 2026-11-23 | Godot 4.7.2 .NET·SDK 8.0.318·Windows 11, Info·Build | 지원 조합 확인, C# 컴파일 종료 0·경고 0·오류 0 | [실행 증거](evidence/PLAN-002/run-001/README.md) |
| 2026-11-23 | headless 에디터 임포트·실제 C# Main 로드 | 초기화·3프레임 진행·공통 종료·씬 정리 통과 | [headless 로그](evidence/PLAN-002/run-001/smoke-headless.log) |
| 2026-11-23 | Windows/OpenGL 창·computer-use 시각/입력 확인 | C# runtime ready 표시, Exit 버튼 클릭 후 창 제거·프로세스 종료 0 | [화면](evidence/PLAN-002/run-001/windowed.png)·[로그](evidence/PLAN-002/run-001/windowed.log) |
| 2026-11-23 | 재실행·잘못된 호출·로그 보호·환경 복원 | 모두 통과 | [결과 JSON](evidence/PLAN-002/run-001/results.json) |

시스템 PATH·전역 NuGet 설정을 변경하지 않았다. 기존 Unity SDK 경로에 의존하며 해당 Unity 제거 시 로컬 도구 설정 갱신이 필요하다.
에디터 UI 조작·배포용 내보내기·투명 오버레이·클릭 투과·다중 모니터·SCP AI·성능은 미검증이다.
초기 상대 경로 로그 수집 실패와 UI 캡처 재관찰은 실행 증거에 기록했다.

## 관련 결정과 상태 전환

- 관련 기술 결정: [TECH-002](../decisions/TECH-002-development-toolchain.md).
- 2026-11-23: proposed 생성. 후속 작업을 proposed로 작성하라는 사용자 지시를 따른다.
- 2026-11-23: proposed → active. 사용자 명시적 승인 “PLAN-002를 승인할게 작업 시작해줘.”를 근거로 전환. 후속 PLAN 실행은 포함하지 않는다.
- 2026-11-23: active → completed. 개발 환경·최소 C# 실행·종료·재실행·검증 경로 기록의 필수 기준 충족. 후속 PLAN은 활성화하지 않는다.

