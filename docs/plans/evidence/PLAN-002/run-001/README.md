# PLAN-002 개발 환경 검증 / run-001

- 사용자 날짜: 2026-11-23, Asia/Seoul.
- 작업 루트: `C:/Dev/scp_desktop`.
- 관련 작업: [PLAN-002](../../../PLAN-002-development-environment.md).
- 결과: [results.json](results.json).

## 환경과 출처

- Windows 11 Home 10.0.26300 x64, NVIDIA GeForce RTX 5070, OpenGL 3.3 / 드라이버 621.01.
- Godot 4.7.2.stable.mono.official.ed1daf0bf; .NET SDK 8.0.318; 런타임 8.0.21; net8.0.
- [공식 Godot .NET 다운로드](https://downloads.godotengine.org/?flavor=stable&platform=windows.64&slug=mono_win64.zip&version=4.7.2).
- 압축 파일 SHA-256: `A2A48473A7414C5F19FAB690518CAEBB738C09EF9601F6BD2388676A7F53B3C0`.
- 실행 파일 Authenticode: Valid, Prehensile Tales B.V. 서명 확인. 다운로드 해시는 수령본 식별값이며 공식 체크섬 대조를 주장하지 않는다.
- SDK는 Unity 6000.6.7f1의 `Editor/Data/DotNetSdk`에 있던 것을 사용했다. Unity 실행·수정·전역 SDK 설치는 하지 않았다.
- 도구 경로와 버전 원문: [environment.log](environment.log).

## 실제 실행

아래 명령은 프로젝트 루트에서 실행했다. Tee-Object 기록 경로는 절대 경로를 사용했다.

| 확인 | 실행 | 결과·증거 |
| --- | --- | --- |
| 도구 정보 | `./scripts/godot.ps1 -Action Info` | Godot·SDK·런타임 확인, environment.log |
| C# 컴파일 | `./scripts/godot.ps1 -Action Build` | 종료 0, 경고 0·오류 0, [build.log](build.log) |
| 에디터 리소스 스캔 | `./scripts/godot.ps1 -Action Import -LogFile docs/plans/evidence/PLAN-002/run-001/import.log` | 정상 종료, [import.log](import.log) |
| 화면 없는 실행 | `./scripts/godot.ps1 -Action Smoke -Headless -LogFile docs/plans/evidence/PLAN-002/run-001/smoke-headless.log` | 실제 Main C# 초기화·3프레임·공통 종료·ExitTree, [로그](smoke-headless.log) |
| 실제 Windows 실행 | `./scripts/godot.ps1 -Action Run -LogFile docs/plans/evidence/PLAN-002/run-001/windowed.log` | 화면의 C# runtime ready·Exit 버튼 확인, 클릭 후 창 사라짐·종료 코드 0, [로그](windowed.log)·[화면](windowed.png) |
| 재실행·환경 복원 | `./scripts/godot.ps1 -Action Smoke -Headless -LogFile docs/plans/evidence/PLAN-002/run-001/smoke-restart.log` | 정상 재실행·종료, PATH/DOTNET_ROOT/telemetry 환경 복원, [로그](smoke-restart.log) |
| 잘못된 호출·증거 보호 | Smoke에 LogFile 생략, 기존 smoke-restart.log 지정 | 실행 전 오류 반환·기존 로그 덮어쓰기 거부, results.json |

최종 문서 상태 검사는 `doc-validation.log`에 별도 기록한다.
검증 씬의 통과는 실제 C# 씬을 로드하고 실행한 결과이며, 단순 컴파일 성공만으로 판정하지 않았다.

## 관찰과 복구

- 첫 Tee-Object 시도는 스크립트 내부 작업 디렉터리 전환 때문에 상대 로그 경로가 GodotProject 아래로 해석되어 실패했다.
- 절대 경로로 수정해 새 로그를 생성했다. 실패한 로그 수집을 통과 증거로 사용하지 않았다.
- 첫 UI 캡처는 가려진 배경을 보였다. 대상 창을 활성화한 뒤 새 캡처로 제품 화면을 확인했고 그 캡처만 저장했다.
- Exit 클릭 뒤 실제 창 목록에서 대상 창 제거와 프로세스 종료 0, EXIT_REQUESTED·STOPPED 로그를 함께 확인했다.
- 기획 원본 SHA-256은 `7D7ADC3BFE0AB12373D7CFAF3A9BFA6E4A4354381F02515EAAE5FD12ADFB0E44`로 유지되었다.

## 범위와 한계

- 일반 C# 컴파일·에디터 임포트·최소 프로젝트 실행을 검증했다. 배포용 내보내기는 하지 않았다.
- 실제 화면은 정상적인 불투명 개발용 창이다. 투명 오버레이·클릭 투과·다중 모니터·창 물리·SCP AI는 미구현이다.
- 에디터 UI 조작·최종 시각 디자인·성능·다른 Windows 기기 동작은 검증하지 않았다.
- 제품의 기본 진입 씬은 Main이며 검증 씬에 의존하지 않는다. 다음 단계가 검증 씬 통과만으로 완료되는 것은 아니다.
