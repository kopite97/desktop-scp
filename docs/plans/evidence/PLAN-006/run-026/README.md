# PLAN-006 최종 검증 — 2026-11-24

## 환경과 실행

- 저장소 C:/Dev/scp_desktop. Godot 4.7.2 .NET Compatibility/OpenGL, RTX5070, .NET8 SDK. 최종 Build 경고0/오류0.
- LG FULL HD GSM5BDE, physical(-1920,1061), usable1920×1032, DPI96. [monitor.json](monitor.json). 주 모니터/디스플레이 설정과 사용자 창 위치는 변경하지 않았다.
- 다음 명령에서 새 회차를 지정했다. 사용자에게 약20초 입력 검사를 알렸고 “20초간 마우스 사용 중지” 응답 후 실행했다.

```powershell
./scripts/godot.ps1 -Action Scp049Smoke -MonitorId GSM5BDE -NativeInput -EvidenceDirectory docs/plans/evidence/PLAN-006/run-026 -LogFile docs/plans/evidence/PLAN-006/run-026/execution.log
```

## 결과

- **PASS: 71개**, 종료0, [결과](results.json), [오류 없는 실행 로그](execution.log). 입력 수신기와 모든 조작 대상 창은 하네스 소유다.
- 탐색→예약→이동→3초 치료→단일 좀비 변이, 대상 삭제/중복 소유/빠른 완료 거부, 생성 상한/수명, 좀비 군집 구간 왕복/45초 수명/클릭 회수.
- 같은 PlatformWorld로 의사·데이터가 실제 창에 착지한다. 창 이동으로 동반 이동/균형 전환, 최소화/소멸과 숨김 중 지지 소실로 치료 취소/재탐색. 인접 상단 도약 후 치료는 공통 상태 검사에도 포함했다.
- 실제 native 오른쪽 드래그로 의사/데이터 이동·중력 복귀, 가짜 데이터/좀비 왼쪽 클릭 회수, 본체 빈 모서리/숨긴 데이터 위치의 다른 프로세스 입력 통과.
- 실제 SCP-999 클릭이 Main의 같은 SCP-049 스트레스 값을3 감소시켰다. [별도 수신 down/up](receiver.json)으로 입력 전달과 본체 소비를 구분했다.
- 숨김 시 생성/치료/수명/이동 시간 동결·모든 입력 창 숨김, 복원 시 새 창 관찰, 종료 시 데이터/예약/창/스트레스 등록 정리.
- [치료 화면](native-treatment.png), [변이/좀비 화면](native-zombie.png), [제어 패널](control-panel.png). 제어 버튼이 창 안에 모두 들어오는지 검사했다. 캡처는 소유 배경/창 또는 자체 viewport만 포함한다.
- [현재 코드 SHA256](source-hashes.json). Product PlagueState/Settings/Controller/ActorWindow에는 File/Directory/FileStream/System.IO/외부 프로세스 호출이 없다. 데이터는 메모리 ID/종류/물리/시간으로만 존재하며 .tmp는 그림의 의미다. OS 파일을 생성/탐색/삭제하지 않는다.

## 별도 회귀

- [run-016](../run-016/results.json): headless43개 PASS, 인접 도약/상단 치료 포함. 같은 제품 상태와 시간을 호출했다.
- [환경](../run-016/environment.log), [Overlay headless10개](../run-016/overlay.log), [Import](../run-016/import.log) 종료0.
- [run-020 SCP-999 회귀](../run-020/results.json): 기존 native+실제 입력111개 PASS. 독립 999 진단에서는 Main.EnablePlague=false로 새 개체를 끄며, 개체 간 실제 공통 스트레스 연동은 본 회차에서 별도로 검사했다.
- [run-015](../run-015/results.json)의 최초 native69개 통과 후 자동 도약/제어 패널 검사2개를 더한 최종 결과가71개다.

## 실패·환경 간섭과 수정

- run-001은 중간 제품 미리보기다. CloseMainWindow 반환만 확인해 실제 종료를 보장하지 못했고, 남아 있던 미리보기 개체가 뒤의 입력 테스트와 겹쳤다.
- run-002: 로직42개는 통과했지만 실행 스크립트가 새 Scp049Smoke에 SCP999 마커를 요구했다. 액션별 마커 검사로 수정했다.
- run-003~014: 실제 발판/치료/변이 시나리오는 통과했으나 입력 단계가 실패했다. 중복 미리보기와 foreground 거부를 trace로 확인했다. run-013의 실제 클릭 대상 PID31144가 run-001 미리보기인 것을 CIM 명령행과 대조했다. 이 작업이 만든 프로세스만 종료했다.
- 이후 godot.ps1은 같은 프로젝트 live-preview.log 프로세스가 남아 있으면 native 진단을 시작 전에 거부한다. 물리/표시 창의 중복 인스턴스를 제품 입력 실패로 오인하지 않도록 입력 직전 실제 HWND도 확인한다.
- run-017~019/021~025: 수신기 Z순서·합성 클릭 타이밍 보완 중 일부 전달 실패/외부 커서 이동이 있었다. 중간 실패를 통과로 처리하지 않았다. run-025에는 원하는 커서(-1120,1965)와 실제(-896,1957)의 차이가 기록됐다. 마우스 사용을 잠시 멈출 수 있는지 확인한 뒤 run-026에서 전체 통과했다.
- 수신기는 선택적으로 topmost를 사용하고, 몸체는 제품의 NOACTIVATE 표시 갱신으로 위에 둔다. press와 release를 프레임 간격으로 나눠 실제 클릭을 검사한다. 사용자 앱으로 대체 입력을 보내지 않는다. 매 회차 종료 시 소유 수신기를 종료하고 원래 커서/포커스 복원을 시도한다.
- 실패 로그/당시 결과는 모두 보존했다. 모든 이전 실패의 원인을 하나로 단정하지 않는다.

## 아트와 한계

- 기능 표현은 코드 도형 임시본이다. ART-004 Scp049, ART-005 DummyData, ART-006 Scp0492, ART-007 Scp049Mutation은 별도 proposed이며 원본 제작/사용자 시각 승인/정식 적용은 완료하지 않았다.
- 탐색은 같은 화면의 가까운 대상을 선택하고 공통 도약/가장자리 낙하를 사용한다. 완전한 다중 창 최단 경로 탐색기는 아니다. 도달하지 못하면12초 뒤 예약 해제/2초 후 재시도한다.
- DWM 사각 근사·50ms 관찰 한계, 실제 혼합 DPI 장비/모니터 물리 분리/장기 성능/최대 효과 부하/배포 내보내기는 미측정이다.
- PLAN-008의 OS 부하 가속·스트레스 자동 증가·정식 봉쇄 UI·트레이와 PLAN-007의 SCP-173은 구현하지 않았다.
