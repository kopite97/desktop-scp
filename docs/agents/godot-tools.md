# Godot·.NET 도구

적용: 개발 환경 확인·엔진 실행·컴파일·테스트·내보내기.
탐색: [프로젝트 구조](project-structure.md), [검증](verification.md).

## 확인된 개발 환경

- Godot `4.7.2.stable.mono.official.ed1daf0bf`, Windows x64 .NET 에디션.
- .NET SDK 8.0.318, 런타임 8.0.21, 대상 net8.0, Godot.NET.Sdk 4.7.2.
- 프로젝트는 [GodotProject/project.godot](../../GodotProject/project.godot), 솔루션은 [SiteDesktop.sln](../../GodotProject/SiteDesktop.sln)이다.
- 렌더러는 Compatibility/OpenGL이다. Windows 11 10.0.26300과 NVIDIA RTX 5070에서 최소 창을 확인했다.
- Run은 SCP·복제 아이콘을 활동시키고 제어창 없이 트레이에 상주한다. PlatformPreview는 공통 창 발판/물리의 별도 실행 경로다.
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
Run은 저장된 SCP ON/OFF를 복원한 뒤 트레이를 시작한다. 트레이 좌클릭/Show controls로 제어창을 열고, X·Alt+F4·Hide controls로 창만 숨긴다. 봉쇄/재개는 Ctrl+Shift+L 또는 트레이 메뉴를 사용한다.
완전 종료는 트레이 Exit다. 트레이 생성 실패 시 제어창과 Exit 버튼을 유지한다. 재개 시 제어창은 자동으로 열리지 않는다.
진단은 사용자 설정을 격리한다. 로컬 EXE 재현은 `-- --scp-settings-file <격리 JSON 절대 경로>`로 저장 파일을 지정할 수 있다.

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
./scripts/godot.ps1 -Action LightingProbe -Revision r005 -EvidenceDirectory 'docs/plans/evidence/PLAN-005/run-039' -LogFile 'docs/plans/evidence/PLAN-005/run-039/execution.log' -MonitorId GSM5BDE -Preview
```

- r002는 4장, r003은 idle/move 12장, r004는 50장이다. r005-sample/r005는 외곽선 분리 4장/50장이다. 제품 진입 씬은 바꾸지 않는다.
- Preview를 생략하면 제한 시간 내 자동 종료한다. Preview에서는 방향키로 광원, N으로 밤, F로 반전, Space로 재생, H로 두 오버레이 숨김/복원, Esc/Exit로 종료한다.
- Measure는 렌더 검증 뒤 비용 검사, MeasureOnly는 렌더 캡처 없이 비용만 검사한다. 후자는 이미지·재생 검증 성공으로 보고하지 않는다.
- PlaybackOnly는 좌측/낮 프레임 렌더와 실제 시간 재생 재검사다. 전체 조명 행렬을 다시 검사한 것으로 보고하지 않는다. 진단 재생은 단조 경과 시간 기준이다.
- 측정은 1개/2개 표시 창·base/lit 교차 3회, 준비 2초+측정 10초다. Preview 없는 측정은 TimeoutSeconds 240을 지정한다.
- 외곽선 분리 리비전에서는 비용의 기준이 기존 combined lit(legacy)이며 split lit와 비교한다. 전체 캡처+재생+비용을 한 번에 할 때는 Preview로 실행/관찰하거나 목적별 회차를 나눈다.
- 세부 결과·한계는 [PLAN-005](../plans/PLAN-005-scp-999.md)와 연결된 실행 증거를 따른다.

## 실행 기록과 마감

창 발판/물리는 LG 하드웨어 ID를 지정해 실행한다. WMI의 LG FULL HD와 Win32 경계를 대조한 식별자이며 주 모니터 설정은 변경하지 않는다.

```powershell
./scripts/godot.ps1 -Action PlatformPreview -MonitorId GSM5BDE
./scripts/godot.ps1 -Action PlatformSmoke -MonitorId GSM5BDE -EvidenceDirectory 'docs/plans/evidence/PLAN-004/run-009' -LogFile 'docs/plans/evidence/PLAN-004/run-009/execution.log'
```

- 새 회차 경로를 사용한다. Headless 옵션은 공통 규칙/재현 검사, native는 선택 화면의 소유한 시험 창 이동·가려짐·최소화·최대화·종료·화면 픽셀까지 검사한다.
- Preview는 물리 마커를 오른쪽 드래그 후 놓아 낙하시키고 왼쪽 클릭/Jump로 도약한다. 제어 창의 보행/정지/숨김/복원/종료를 사용할 수 있다.
- MonitorId 선택은 PlatformPreview/PlatformSmoke/Run/Scp999Smoke/Scp999Color에서 지원한다. 지정 화면이 초기부터 없거나 여러 개면 실패한다.
- [PLAN-004 최종 증거](../plans/evidence/PLAN-004/run-008/README.md)는 개별 SCP-999 통합/아트 검수와 구분한다.

SCP-999 제품 검사는 새 회차 경로에 다음과 같이 실행한다.

```powershell
./scripts/godot.ps1 -Action Scp999Smoke -EvidenceDirectory 'docs/plans/evidence/PLAN-005/run-039' -LogFile 'docs/plans/evidence/PLAN-005/run-039/execution.log' -MonitorId GSM5BDE
```

- `-Headless`는 공통 상태·규칙만 확인한다. 기본 Windows 검사는 실제 창/수명/정리를 포함한다.
- `-NativeInput`은 별도 소유 수신기에 합성 클릭과 F24 활동을 보내 실제 입력 전달·문지름·오른쪽 드래그를 검사한다. 포커스 요청이 거부되면 WindowFromPoint/PID로 소유 수신기를 확인한 뒤 활성화 클릭을 시도한다. 이후에도 포커스가 달라지면 입력을 중단하며 사용자 앱에 대신 보내지 않는다.
- NativeInput 실행 중 커서를 사용하면 검사와 충돌할 수 있다. 시험은 제한 시간 안에 끝나며 생성한 수신기만 종료하고 원래 커서/포커스 복원을 시도한다.
- 결과·실패 원인·회귀와 한계는 [최종 통합 증거](../plans/evidence/PLAN-005/run-035/README.md)에 있다. 네이티브 창 플래그만으로 외부 클릭 전달 성공을 판정하지 않는다.

- `Scp999Color`는 EvidenceDirectory/LogFile/MonitorId로 동일 프레임의 실제 제품 조명 전후를 비교한다. Headless는 지원하지 않는다.
- `Run -MonitorId GSM5BDE`는 SCP를 LG에 표시하고 제어창은 트레이에서 열 때 LG에 표시한다. 오른쪽 드래그를 놓으면 실제 창/바닥에 낙하한다.

- 정확한 버전·명령·작업 디렉터리·대상·종료 결과·산출물 위치를 PLAN 증거에 남긴다.
- 일반 C# 컴파일, 에디터 실행, 테스트 실행, 배포용 내보내기를 구분한다. BehaviorSmoke의 공통 FSM/비용 검사와 MonitorPortalSmoke의 모니터 전이 검사는 [하네스 지침](harness.md)을 따른다.
- 실제 Windows 표시·클릭 투과를 화면 없는 테스트 통과로 대신하지 않는다.
- 테스트 도구 연결 성공과 테스트 수행·제품 검증 성공을 구분한다.

## SCP-049 진단

- `Scp049Smoke -MonitorId GSM5BDE -EvidenceDirectory <새 회차> -LogFile <새 로그>`: LG의 소유 창에서 제품 사이클/물리를 검증한다. `-Headless`는 규칙, `-NativeInput`은 별도 수신기 입력을 추가한다.
- native Scp049Smoke/Scp999Smoke/PlatformSmoke는 같은 프로젝트의 live-preview.log 프로세스가 남아 있으면 시작 전에 거부한다. CloseMainWindow 반환만으로 종료를 가정하지 말고 소유 PID/프로세스 종료를 확인한다.
- 실제 입력 중 사용자 마우스 이동과 충돌하면 실패를 보존하고 간섭 없는 구간에 다시 검사한다. [PLAN-006 증거](../plans/evidence/PLAN-006/run-026/README.md).

- `OverlayStackSmoke -MonitorId GSM5BDE -EvidenceDirectory <새 회차> -LogFile <새 로그> -TimeoutSeconds 90`: 실제 겹침 30초·순서·창 수명 검사. `-NativeInput`은 별도의 짧은 실제 겹침 드래그/수신기 검사다. Headless는 거부하며 [PLAN-011](../plans/PLAN-011-overlay-stacking.md)에 증거를 남긴다.

## 공식 기준

- 로컬 EXE: [export-windows.ps1](../../scripts/export-windows.ps1) `-LogFile <새 로그>`로 Windows x64를 `.artifacts/PLAN-015/windows-x64`에 내보낸다. 4.7.2 .NET Windows 템플릿이 필요하다. `SiteDesktop.exe`와 PCK·data·IconWorker 폴더를 함께 유지한다. 콘솔 래퍼는 생성하지 않는다.
- 시작 관찰: [observe-tray-startup.ps1](../../scripts/observe-tray-startup.ps1) `-EvidenceDirectory <새 회차>`는 소유 EXE만 최대30초 관찰하고 180프레임 후 종료한다. 입력 없이 창 크기·표시·포커스를 표본 기록하며 트레이 실제 메뉴 검증과 구분한다.

- `DesktopIconSmoke -MonitorId GSM5BDE -EvidenceDirectory <새 회차> -LogFile <새 로그>`는 PLAN-012의 테스트 소유 ICO/바로가기 fixture를 준비한 뒤 실행한다. `-NativeInput`은 별도 수신기로 실제 클릭/드래그를 검사한다. helper는 Build 시 함께 빌드되며 [최종 기록](../plans/evidence/PLAN-012/run-014/README.md)에 준비/정리·한계를 남겼다.

- [Godot 4.7 C# 준비와 작업 흐름](https://docs.godotengine.org/en/4.7/tutorials/scripting/c_sharp/c_sharp_basics.html)
- [Godot 4.7 CLI](https://docs.godotengine.org/en/4.7/tutorials/editor/command_line_tutorial.html)
- [렌더러 선택](https://docs.godotengine.org/en/4.7/tutorials/rendering/renderers.html)
