# 프로젝트 구조

적용: 파일 생성·배치·이동·명명.
탐색: [작업 절차](workflow.md), [아키텍처](architecture.md).

## 현재 기준 루트

프로젝트 루트는 `C:/Dev/scp_desktop`이다. 도구는 가능하면 스크립트 위치에서 루트를 찾는다.
현재 구현은 문서 하네스와 Godot C# 오버레이 기반이다. Git 기본 브랜치는 main이며 원격 origin은 https://github.com/kopite97/desktop-scp.git 이다.

| 위치 | 책임 |
| --- | --- |
| AGENTS.md | 진입·작업별 문서·기준 위치 MAP |
| SCP_site-desktop.md | 기존 기획 원본 |
| docs/agents | 주제별 작업 지침 |
| docs/game-design | 기획 MAP·로드맵·향후 분리한 제품 규칙 |
| docs/plans | 개발·Godot 통합 PLAN·목록·템플릿·관리 규칙 원본 plan.md |
| docs/plans/evidence | 실행별 검증 증거 |
| docs/decisions | 기술 결정·목록·템플릿 |
| scripts | 문서 검사·개발 자동화와 해당 도구의 테스트 |
| GodotProject | Godot에서 여는 프로젝트·C# 솔루션·원본 씬과 스크립트 |
| Art | [Art MAP](../../Art/AGENTS.md)이 안내하는 아트 작업 공간 |
| .tools | 로컬 Godot 배포본·다운로드. 원본 관리 대상에서 제외 |

## 제품 배치

- `GodotProject/Scenes/App/`와 `GodotProject/Scripts/App/`는 실제 진입 씬·C# 코드다.
- `Scripts/Desktop/`는 좌표·오버레이 상태, `Scripts/Overlay/`는 창·표현·입력 연결, `Scripts/Platform/Windows/`는 Win32 경계다.
- `GodotProject/Scenes/Diagnostics/`와 `GodotProject/Scripts/Diagnostics/`는 환경 검증 씬·C# 코드다. 제품 진입은 여기에 의존하지 않는다.
- `.sln`, `.csproj`, `global.json`, `project.godot`, `.tscn`, `.cs`, 생성된 `.cs.uid`는 보존할 원본이다.
- `.godot/`는 Godot 임포트와 C# 중간/출력 캐시다. bin·obj·IDE 사용자 설정과 함께 원본 관리에서 제외한다.
- `.gitignore`로 로컬 도구·캐시·머신별 설정을 제외한다. 기획·지침·코드·Art 원본/리비전·검증 증거는 함께 보존한다.
- 머신별 실행 파일 경로는 `scripts/tool-paths.local.json`에 두고 예제만 공유한다.
- 없는 엔진 프로젝트나 기능을 표현하려고 빈 폴더를 만들지 않는다.
- 엔진 캐시·내보내기 결과·사용자 저장 데이터를 문서 증거나 원본 에셋과 혼동하지 않는다.
- 패키지·Git·LFS·배포 설정은 해당 실행 범위가 생긴 뒤 결정한다.

## 배치와 명명

- 같은 디렉터리의 파일은 같은 책임에 속한다. 형제 폴더는 동일한 분류 기준을 사용한다.
- 문서·도구 파일은 설명적인 소문자 kebab-case를 기본으로 한다. AGENTS·README·PLAN·ART·TECH 명명은 예외다.
- PLAN·ART·TECH ID는 각각 독립적으로 증가시키며 재사용하지 않는다.
- Common·Utils·Misc를 서로 무관한 파일의 보관함으로 사용하지 않는다.
- 실제 파일이 생길 때 폴더를 만든다. 예정 경로는 코드 표기로 적는다.
- 이전 자료를 이동하면 현재 링크와 참조를 함께 갱신한다. 과거 증거의 기록은 보존한다.
- 제품 원본과 테스트용 사본·임시 데이터의 위치를 구분한다.
- 임시 시각 에셋은 용도·사용처·교체 대상을 PLAN에 기록하고 게임 규칙의 식별자로 사용하지 않는다.
