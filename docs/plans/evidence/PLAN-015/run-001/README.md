# PLAN-015 실행 증거 색인

## 환경·범위

- 2026-11-25, Windows/Godot4.7.2 .NET·OpenGL, NVIDIA RTX5070. 기존 LG `GSM5BDE`, 물리 영역(-1920,1061)1920×1080·DPI96 환경을 사용했다. 기본 모니터 설정은 변경하지 않았다.
- C# Build는 `scripts/godot.ps1 -Action Build`. 최종 경고0/오류0. 일반 빌드·native·배포 EXE는 별도 실행했다.
- headless 환경: `-Action Smoke -Headless -LogFile <회차>/environment.log` 및 `-Action SystemSmoke -Headless -EvidenceDirectory <회차> -LogFile <회차>/execution.log`.
- Windows: `-Action SystemSmoke -MonitorId GSM5BDE -EvidenceDirectory <회차> -LogFile <회차>/execution.log`. 합성 입력 옵션은 사용하지 않았다.
- 배포: `scripts/export-windows.ps1 -LogFile <회차>/export.log`. 공식 Godot4.7.2 .NET 템플릿의 Windows x64 파일만 로컬 Godot 템플릿 경로에 준비했다. 로컬 다운로드 URL은 `https://github.com/godotengine/godot-builds/releases/download/4.7.2-stable/Godot_v4.7.2-stable_mono_export_templates.tpz`다.
- EXE 출력: `.artifacts/PLAN-015/windows-x64/`의 EXE·PCK·data_SiteDesktop_windows_x86_64·IconWorker. 게임과 Shell worker는 .NET 런타임을 함께 포함한다. 콘솔 래퍼·설치·외부 배포는 없다.

## 실행별 판정

| 회차 | 결과 | 증거 |
| --- | --- | --- |
| run-001 | 최초 headless18개·EnvironmentSmoke PASS | [결과](results.json), [환경](environment.log), [로그](execution.log) |
| run-002 | 최초 Windows38개 PASS | [결과](../run-002/results.json) |
| run-003 | export 성공, 실행 FAIL. JSON/PNG를 원본 폴더에서 직접 읽는 기존 로더가 배포 폴더에서 실패 | [내보내기](../run-003/export.log), [실행 실패](../run-003/execution.log) |
| run-004~005 | 패키지 API로 로더 변경 중 native38개 및 중간 export 성공. 최종 배포 판정 아님 | [native](../run-004/results.json), [중간 export](../run-005/export.log) |
| run-006 | ZombieArtSmoke headless109개 PASS | [결과](../run-006/results.json) |
| run-007 | 배포 EXE의 사용자 실제 메뉴·도구 X 클릭·프로세스 종료 PASS | [export](../run-007/export.log), [실행](../run-007/execution.log), [UI 기록](../run-007/ui-verification.json) |
| run-008 | DoctorArtSmoke native158개 PASS | [결과](../run-008/results.json) |
| run-009 | ZombieArtSmoke native717개 PASS | [결과](../run-009/results.json) |
| run-010 | 시작 관찰 FAIL. 초기 창 플래그 복구가 숨긴 루트 창을656×639로 다시 표시 | [관찰](../run-010/results.json) |
| run-011 | 지연 복구 조건 수정 중 FAIL. Godot Window.Size와 네이티브 override 크기가 달라 복구 분기가 실행되지 않음 | [로그](../run-011/execution.log) |
| run-012~014 | 명시적 최초 복구 상태로 수정, Windows38개 PASS·export 성공. 엄격한1px 관찰은 엔진64px 보정 때문에 FAIL | [native](../run-012/results.json), [관찰](../run-014/results.json) |
| run-015~017 | 새 봉쇄 배너 native39개·실제 렌더 PASS. 최소화 부트 실험은 순간 전경 획득으로 FAIL, 채택하지 않음 | [native](../run-015/results.json), [관찰](../run-017/results.json) |
| run-018~021 | 크기 조절 금지/화면 밖 부트 실험은 엔진 보정으로 크기 조건 FAIL. 둘 다 최종에서 제거 | [관찰19](../run-019/results.json), [관찰21](../run-021/results.json) |
| run-022~023 | 최종 export와 재실행 PASS. 236표본·제어창204표본·가시 개체589표본, 예상 밖 제어창/전경0, 종료0 | [export](../run-022/export.log), [관찰](../run-023/results.json), [로그](../run-023/execution.log) |
| run-024 | 최종 EnvironmentSmoke 및 headless18개 PASS | [환경](../run-024/environment.log), [결과](../run-024/results.json) |
| run-025 | 최종 Windows39개 PASS·1.5초 봉쇄 배너 렌더 확인 | [결과](../run-025/results.json), [배너](../run-025/lockdown-banner.png), [로그](../run-025/execution.log) |

## 실제 UI와 종료

- 기존 PLAN-014 미리보기는 Alt+F4로 정상 종료하고 프로세스 부재를 확인했다.
- run-007은 에디터 없는 `SiteDesktop.exe` PID4976을 실행했다. 제어창 없이 SCP999/049/173·복제 아이콘 창이 나타났다. 이후 제어창에서 Desktop icons13을 관찰하여 독립 worker 로딩도 확인했다.
- 도구로 실제 제어창의 X 버튼을 클릭했다. 창 목록에서 제어창은 사라지고 SCP·복제 아이콘 창은 유지됐다.
- Windows 도구가 숨겨진 트레이 영역·팝업을 대상 창으로 노출하지 않아 해당 조작만 사용자가 수행했다. Show controls→Alt+F4, Lockdown→Resume(제어창 숨김 유지), Exit에 대해 사용자 “세 단계 모두 정상이에요” 확인을 받았다.
- 실행 로그에 Show/Hide·LOCKED/RESUMED·EXIT_REQUESTED·모든 STOPPED가 기록되었고 PID4976 및 관련 worker 부재를 확인했다. 최종 재실행은 run-023의180프레임 후 정상 종료를 따로 확인했다.
- 사용자 확인 이후의 변경은 초기 표면/제어창 최초 복구와 봉쇄 배너다. 트레이 메뉴 연결은 동일하며 최종 native39개와 EXE 재실행으로 후속 확인했다. 최종 EXE의 메뉴 전체를 사용자가 다시 클릭한 것으로 기록하지 않는다.

## 초기 표시 판정과 한계

- 최초 관찰은 Win32 Visible HWND가1×1보다 큰 경우 전부 실패시켰다. 실제656×639 제어창 노출은 발견 후 수정했으며, 정상 창 크기/테두리를 트레이에서 열 때만 복구한다.
- Windows/Godot가 C# 시작 전에 빈 투명 부트 표면을64×64로 보정하는 것은 별개다. 최종 관찰기는 이 표면을 명시적으로 허용하고,64px 초과의 화면 내 창·no_focus 누락·전경 획득을 계속 실패시킨다. 최종 실행에는64×64 빈 표면1표본이 있었다.
- 이 판정 보완은 네이티브 창이 전혀 생성되지 않는다는 보장이 아니다. 부트 splash와 viewport는 투명하게 구성했고 사용자용 제어창은 숨긴다. 순수1px 요구를 달성했다고 보고하지 않는다.
- 관찰은10ms 요청의 주기 표본이며 실제 Windows 스케줄링 간격은 더 길 수 있다. 모든 프레임·모든 PC에서 무깜빡임/포커스 무변화를 입증한 것으로 확대하지 않는다. 실패한 최소화/크기 조절 금지/오프스크린 실험 설정은 남기지 않았다.

## 배너와 미검증

- 사용자 가로줄 피드백을 반영하여 전체 화면 셔터를 제거했다. 440×112 배너에 잠금 아이콘·LOCKDOWN·한국어 일시정지/재개 안내를 표시하며1.5초 수명, 등장0.12초·퇴장0.3초다. 실제 PNG를 열어 한글·아이콘·여백·대비를 확인했다.
- SystemSmoke는 배너 표시 중 이미 SCP가 숨겨지고 시계가 멈춘 상태, 배너 수명 뒤 native 창 제거, 빠른 재개 시 표시 해제를 검사한다. 기존 클릭 투과 코드는 유지했다. 배너 위치에서 별도 외부 수신기로 클릭을 다시 주입하는 검증은 수행하지 않았다.
- Explorer 재시작, 혼합 DPI/탈착, 다른 PC, 장시간 성능, Windows 로그인 자동 실행은 검증/구현 범위 밖이다. UI 표시를 바꾸며 기존 게임/아트 검사를 완화하지 않았다.
- 새 배너 최초 컴파일의 DrawString 인수 순서 오류3개는 수정 후 빌드 경고0/오류0을 확인했다. run-003 실패 프로세스만 종료했고 이후 모든 성공 실행은 정상 종료했다.
