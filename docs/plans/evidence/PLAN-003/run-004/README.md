# PLAN-003 run-004 최종 검증

2026-11-23. 기준 루트 C:/Dev/scp_desktop. [PLAN-003](../../../PLAN-003-overlay-validation.md).
제품 코드는 run-003과 같고 진단에 실제 각 모니터 배치 확인을 추가했다.

## 환경

- Windows 11 10.0.26300, NVIDIA GeForce RTX 5070, 드라이버 621.01, Compatibility/OpenGL.
- Godot 4.7.2.stable.mono.official.ed1daf0bf, .NET SDK 8.0.318 / 런타임 8.0.21.
- 주 모니터 native (0,0), 3440×1440, 작업 영역 (0,0,3440,1392), 96 DPI.
- 보조 모니터 native (-1920,1061), 1920×1080, 작업 영역 (-1920,1061,1920,1032), 96 DPI.
- Godot 가상 원점은 Win32보다 X +1920. WindowsDesktop에서 변환한다. 사용자 OS 배율·배치를 변경하지 않았다.

## 명령·대상·기대 결과

루트 PowerShell 7에서 다음을 실행했다. 로그 파일이 이미 있으므로 재현할 때 새 run 번호를 사용한다.

~~~powershell
./scripts/godot.ps1 -Action Build | Tee-Object -FilePath C:/Dev/scp_desktop/docs/plans/evidence/PLAN-003/run-004/build.log
./scripts/godot.ps1 -Action OverlaySmoke -Headless -LogFile docs/plans/evidence/PLAN-003/run-004/overlay-headless.log
./scripts/godot.ps1 -Action OverlaySmoke -LogFile docs/plans/evidence/PLAN-003/run-004/overlay-windows.log
~~~

- 대상: OverlaySmoke가 제품 Main·OverlayController·OverlayState를 사용한다.
- 기대: 상태 규칙, native 창과 입력 다각형, 숨김·복원, 각 모니터의 native 좌표 일치, 정리 완료. 오류·60초 초과는 실패.
- [Build](build.log): 0경고·0오류. [headless](overlay-headless.log): 10개 통과. [Windows](overlay-windows.log): 23개 통과. 명령 모두 종료 코드 0.
- 실제 native 배치: 주 모니터 (64,64), 보조 모니터 (-1856,1125). 상태 명령으로 이동한 뒤 GetWindowRect로 일치·작업 영역 포함을 확인했다.
- 위 배치는 실제 창 검증이며 모니터 간 손 드래그를 수행했다는 의미는 아니다.
- [results.json](results.json), [검증 시점 소스 SHA256](source-sha256.json).

## 실제 입력 검증과 수정 이력

- [run-001](../run-001/README.md): 원점 혼동 발견. 변환 수정 후 run-002 자동 검사 통과.
- [run-002](../run-002/README.md): 별도 프로세스로 빈 곳·투명 모서리 통과/몸체 차단 확인. 빠른 드래그 결함 발견.
- [run-003](../run-003/README.md): 드래그 수정 후 이동된 입력 영역·숨김·복원·종료와 두 개체 입력 확인. 초기화/종료 회귀도 통과.
- 화면은 run-003을 재사용한다. 진단만 확장한 이번 회차에서 중복 화면·InputProbe 실행을 만들지 않았다.
- 문서 검사는 [docs-validation-final.log](docs-validation-final.log)에 통과 기록이 있다. 제품 검증과 별도 판정이다.
- 최초 검사에서는 아직 생성 전인 검사 로그 링크 1개가 실패했다. [최초 표준 출력](docs-validation.log)을 보존하고 파일 생성 후 재검사했다.

## 한계와 판정

- 완료: 두 개체의 투명 표시, 분리 입력 윤곽, 빈 공간 통과, 실제 음수 원점 native 배치, 드래그, 즉시 숨김·복원·종료.
- 모의만 확인: 96/144 혼합 DPI 좌표의 중복 배율 방지, 경계·작업 영역 제한, 모니터 제거 후 복구.
- 미검증: 실제 혼합 DPI·핫플러그·모니터 간 실제 마우스 드래그·RDP·배포 내보내기·다량 개체 성능.
- 테스트 표현은 두 개의 임시 다각형이다. SCP AI·발판 물리·정식 봉쇄·트레이는 이 PLAN 범위가 아니다.
- 실제 가능한 배치와 필수 입력/수명 기준을 충족했다. 후속 PLAN 활성화 근거로 사용하지 않는다.
