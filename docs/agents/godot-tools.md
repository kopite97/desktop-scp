# Godot·.NET 도구

적용: 개발 환경 확인·엔진 실행·컴파일·테스트·내보내기.
탐색: [프로젝트 구조](project-structure.md), [검증](verification.md).

## 확인된 개발 환경

- Godot `4.7.2.stable.mono.official.ed1daf0bf`, Windows x64 .NET 에디션.
- .NET SDK 8.0.318, 런타임 8.0.21, 대상 net8.0, Godot.NET.Sdk 4.7.2.
- 프로젝트는 [GodotProject/project.godot](../../GodotProject/project.godot), 솔루션은 [SiteDesktop.sln](../../GodotProject/SiteDesktop.sln)이다.
- 렌더러는 Compatibility/OpenGL이다. Windows 11 10.0.26300과 NVIDIA RTX 5070에서 최소 창을 확인했다.
- 현재 구현은 C# 제어 창·두 투명 오버레이·클릭 투과·드래그·숨김/복원과 진단 씬이다. SCP 행동은 아직 없다.
- 버전·선택 이유·한계는 [TECH-002](../decisions/TECH-002-development-toolchain.md), 실행 증거는 [PLAN-002](../plans/PLAN-002-development-environment.md)를 따른다.

## 도구 경로와 준비

- Godot 실행 파일은 `.tools/godot/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe`다.
- SDK는 기존 `C:/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Data/DotNetSdk/dotnet.exe`를 재사용했다. Unity 에디터를 실행하거나 수정하지 않는다.
- 머신별 경로는 `scripts/tool-paths.local.json`에 둔다. 새 환경에서는 [예제](../../scripts/tool-paths.example.json)를 복사하고 확인한 실행 파일 경로를 입력한다.
- Godot이 없으면 [공식 4.7.2 .NET Windows 배포](https://downloads.godotengine.org/?flavor=stable&platform=windows.64&slug=mono_win64.zip&version=4.7.2)를 내려받아 위 위치에 푼다. GodotSharp 하위 폴더도 함께 보존한다.
- SDK 경로가 없어지면 호환되는 .NET 8 SDK 경로로 로컬 설정을 갱신한다. global.json의 SDK 선택 조건도 확인한다.
- [실행 스크립트](../../scripts/godot.ps1)는 PowerShell 7에서 사용한다. PATH·DOTNET_ROOT는 호출 동안만 설정하고 종료·실패 시 복원한다.
- 로컬 도구와 경로 설정, .godot 캐시는 .gitignore 대상이다. 시스템 PATH·레지스트리는 변경하지 않았다.

## 확인된 문서 도구 명령

프로젝트 루트에서 실행한다.

```powershell
node scripts/validate-agent-docs.mjs
node --test scripts/validate-agent-docs.test.mjs
```

검사기는 현재 작업 디렉터리와 무관하게 자신의 위치에서 프로젝트 루트를 찾는다.
도구 테스트는 임시 디렉터리에서 동작하며 제품 문서·사용자 데이터를 변경하지 않는다.

## 프로젝트 실행

프로젝트 루트의 PowerShell 7에서 실행한다. Import는 에디터의 리소스 스캔 후 자동 종료한다.

```powershell
./scripts/godot.ps1 -Action Info
./scripts/godot.ps1 -Action Build
./scripts/godot.ps1 -Action Import
./scripts/godot.ps1 -Action Run
./scripts/godot.ps1 -Action Editor
```

Build는 C# 컴파일이며 배포용 내보내기가 아니다. C# 수정 뒤 다시 실행한다.
Run은 제어 창과 두 오버레이를 연다. 개체를 드래그하고 Hide/Show로 숨김·복원, Reset positions로 위치를 초기화한다.
Exit 버튼 또는 제어 창 닫기로 모두 종료한다. Hide 중 제어 창은 유지한다.
Editor는 편집기 실행용이며 PLAN-002의 시각 검증은 Run 창에서 수행했다.

## 환경 검증

```powershell
./scripts/godot.ps1 -Action Smoke -Headless -LogFile 'docs/plans/evidence/PLAN-002/run-002/smoke-headless.log'
```

- 예시 run-002는 다음 실행용이다. 이전 증거를 덮어쓰지 않도록 새 회차를 지정한다.
- Smoke는 검증 씬에서 실제 Main 씬을 로드하고 C# 초기화·3프레임 진행·공통 종료 명령·씬 정리를 확인한다.
- 출력에 READY·SMOKE_PASS·EXIT_REQUESTED·STOPPED 표시가 있어야 통과하며, 오류나 60초 한계 초과는 실패다.
- `-Headless`를 생략하면 Windows 디스플레이로 같은 자동 검증을 실행한다. 시각 관찰과 입력 검증은 별도로 수행한다.
- 로그가 없거나 기존 로그를 덮어쓰려 하면 실패한다. 외부 Tee-Object로 실행 출력을 저장할 때는 절대 경로를 사용한다.
- 제품 씬은 검증 코드에 의존하지 않는다. 진단 씬을 제품 기본 진입점으로 바꾸지 않는다.

기획의 API 표기를 검증된 호출 서명으로 간주하지 않는다.
설치되지 않은 도구를 사용 가능한 것으로 기록하거나 확인하지 않은 명령을 완료 절차로 제시하지 않는다.
설치·환경 변경이 필요하면 사용자 요청 범위와 기존 환경을 확인한 뒤 진행한다.

## 오버레이 검증

다음 run-005는 새 실행 예시다. 이미 존재하면 다음 회차를 사용한다.

```powershell
./scripts/godot.ps1 -Action OverlaySmoke -Headless -LogFile 'docs/plans/evidence/PLAN-003/run-005/overlay-headless.log'
./scripts/godot.ps1 -Action OverlaySmoke -LogFile 'docs/plans/evidence/PLAN-003/run-005/overlay-windows.log'
./scripts/godot.ps1 -Action InputProbe -LogFile 'docs/plans/evidence/PLAN-003/run-005/input-probe.log'
```

- OverlaySmoke는 실제 상태 로직 10개를 확인한다. Windows에서는 실제 창·윤곽·숨김·복원과 각 모니터의 native 좌표도 확인한다.
- InputProbe는 별도 프로세스 입력 수신 창이다. 다른 터미널에서 Run을 실행하고 개체·빈 곳·모서리·이동 전후·숨김·종료 후의 수신 수를 비교한다.
- 자동 창 검증은 직접 클릭 투과 증거를 대신하지 않는다. 조작 결과와 로그는 [PLAN-003](../plans/PLAN-003-overlay-validation.md)에 있다.
- InputProbe는 사용 후 창 닫기로 종료한다. 제품 시작 경로에 포함하지 않는다.

## SCP-999 조명 진단

```powershell
./scripts/godot.ps1 -Action LightingProbe -Revision r005 -EvidenceDirectory 'docs/plans/evidence/PLAN-005/run-015' -LogFile 'docs/plans/evidence/PLAN-005/run-015/execution.log' -Preview
```

- run-015는 다음 실행 예시다. 기존 결과/로그를 덮어쓰지 않는다. 실제 Windows 디스플레이가 필요하며 Headless는 거부한다.
- r002는 4장, r003은 idle/move 12장, r004는 50장이다. r005-sample/r005는 외곽선 분리 4장/50장이다. 제품 진입 씬은 바꾸지 않는다.
- Preview를 생략하면 제한 시간 내 자동 종료한다. Preview에서는 방향키로 광원, N으로 밤, F로 반전, Space로 재생, H로 두 오버레이 숨김/복원, Esc/Exit로 종료한다.
- Measure는 렌더 검증 뒤 비용 검사, MeasureOnly는 렌더 캡처 없이 비용만 검사한다. 후자는 이미지·재생 검증 성공으로 보고하지 않는다.
- PlaybackOnly는 좌측/낮 프레임 렌더와 실제 시간 재생 재검사다. 전체 조명 행렬을 다시 검사한 것으로 보고하지 않는다. 진단 재생은 단조 경과 시간 기준이다.
- 측정은 1개/2개 표시 창·base/lit 교차 3회, 준비 2초+측정 10초다. Preview 없는 측정은 TimeoutSeconds 240을 지정한다.
- 외곽선 분리 리비전에서는 비용의 기준이 기존 combined lit(legacy)이며 split lit와 비교한다. 전체 캡처+재생+비용을 한 번에 할 때는 Preview로 실행/관찰하거나 목적별 회차를 나눈다.
- 세부 결과·한계는 [PLAN-005](../plans/PLAN-005-scp-999.md)와 연결된 실행 증거를 따른다.

## 실행 기록과 마감

- 정확한 버전·명령·작업 디렉터리·대상·종료 결과·산출물 위치를 PLAN 증거에 남긴다.
- 일반 C# 컴파일, 에디터 실행, 테스트 실행, 배포용 내보내기를 구분한다.
- 실제 Windows 표시·클릭 투과를 화면 없는 테스트 통과로 대신하지 않는다.
- 테스트 도구 연결 성공과 테스트 수행·제품 검증 성공을 구분한다.

## 공식 기준

- [Godot 4.7 C# 준비와 작업 흐름](https://docs.godotengine.org/en/4.7/tutorials/scripting/c_sharp/c_sharp_basics.html)
- [Godot 4.7 CLI](https://docs.godotengine.org/en/4.7/tutorials/editor/command_line_tutorial.html)
- [렌더러 선택](https://docs.godotengine.org/en/4.7/tutorials/rendering/renderers.html)
