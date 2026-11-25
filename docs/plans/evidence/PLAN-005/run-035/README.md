# PLAN-005 최종 제품 통합 검증 — 2026-11-24

## 환경과 실행

- Windows, Godot 4.7.2 .NET Compatibility/OpenGL, NVIDIA RTX 5070. 기존 .NET8 SDK, Build 경고/오류0.
- LG FULL HD, hardware GSM5BDE, physical (-1920,1061), 1920×1080, usable1920×1032, DPI96. 시스템 화면 설정 변경 없음.
- 저장소 C:/Dev/scp_desktop에서 실행:

```powershell
./scripts/godot.ps1 -Action Build
./scripts/godot.ps1 -Action Scp999Smoke -MonitorId GSM5BDE -NativeInput -EvidenceDirectory docs/plans/evidence/PLAN-005/run-035 -LogFile docs/plans/evidence/PLAN-005/run-035/execution.log
```

## 결과

- PASS: 종료0, [105개 검사](results.json), [오류 없는 로그](execution.log). 실제 입력 없이 플래그만 확인한 결과가 아니다.
- 창 상단 낙하/착지, 실제 이동 시 balance→balance_recover, 인접 창 도약, 점액의 지지 창 부착/이동, 최소화·가림·종료 시 즉시 제거.
- 실제 몸체 위치가 음수 물리 좌표와 일치. 실제 LG 창과 밝아진 본체는 [화면](native-platforms.png), [모니터](platform-monitor.json)에 기록했다. 캡처 배경과 모든 시험 창은 하네스 소유다.
- 별도 프로세스에서 몸체 클릭/문지름·공통 스트레스 감소, 빈 영역/반응/점액의 down/up 투과, 키 활동 반응, 오른쪽 드래그 해제 후 중력 낙하, 추적, 실시간 효과 소멸 확인. [수신 결과](receiver.json), [입력 환경](desktop.json).
- 숨김은 입력 구독/효과를 정리하고 물리 시간을 정지한다. 복원은 최신 발판을 관찰한다. 종료 시 본체/효과/키 감시를 정리한다.
- 후속 [run-037](../run-037/README.md)은 제품 코드 변경 없이 자동 커서 도약·발판 위 쓰다듬기·지지 소실의 pet 중단을 추가한 headless66개 및 Import/환경 검사를 통과했다.
- [최종 적용 계약](art-integration.md), [색 비교](../run-029/README.md). 원본/런타임10개 SHA256 동일, ART-003 r001은 미승인 임시 표현이다.

## 실패와 수정

- run-030: 숨긴 Godot Window의 native handle을 Show 전에 사용. 표시 복원 후 Z순서를 조정하도록 제품 수정.
- run-031: 최소화 후 관찰 generation이 바뀌었는데 시험이 이전 ID 사용. 복원 후 ID를 다시 읽도록 하네스 수정.
- run-032: Windows가 별도 수신기의 foreground 요청 거부. 입력 전에 중단. 후속 fixture는 WindowFromPoint/PID로 확인한 소유 수신기에만 활성화 클릭을 허용하며 이후에도 포커스를 검사한다.
- run-033: 앞 시나리오에서 해제한 키 감시를 시험 시작 시 재설치하지 않음. 실제 표시/복원 명령으로 구독을 다시 설치하도록 하네스 수정.
- run-034: 105개 상태 검사는 통과했지만 숨긴 시험 창의 handle을 정리 중 조회해 엔진 오류가 발생했다. 스크립트가 전체 실행을 실패로 판정했다. 정리 시 저장된 포함 목록을 비우도록 수정 후 run-035 오류 없이 통과.
- run-036: 자동 도약 통과 후 pet 시나리오가 land 애니메이션 완료 전에 입력. 계약 시간까지 기다린 run-037에서 통과. 제품 규칙을 완화하지 않았다.
- 실패 로그와 당시 결과는 모두 보존했다.

## 한계

- DWM 사각 경계/50ms 관찰의 근사, 실제 혼합 DPI 장비와 물리적 모니터 분리 미시험. 모의 화면 제거/음수 좌표/144 DPI 입력은 공통 상태 검사에 포함한다.
- 제품 전체 장기 성능·효과 최악 조건 비용·배포 내보내기는 이번 범위에서 측정하지 않았다. 기존 조명 비용 증거를 새로운 측정으로 보고하지 않는다.
- 점액 원본의 사용자 시각 승인은 별도 ART-003, 정식 승인 적용은 proposed PLAN-010에서 관리한다.
