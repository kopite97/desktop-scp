# 실행 하네스 지침

적용: 명령 실행·상태 관찰·환경 입력·시나리오 재현.
탐색: [아키텍처](architecture.md), [검증](verification.md).

## 현재 범위

PLAN-001은 문서 하네스·검사기, PLAN-002는 최소 C# 실행·종료 검증 씬을 구현했다.
PLAN-003은 OverlaySmoke의 상태/네이티브 검증과 별도 프로세스 InputProbe를 추가했다.
PLAN-004의 PlatformSmoke는 공통 PlatformWorld에 모의/실제 창 관찰을 전달하고, 기록한 입력을 두 번 재생해 trace를 비교한다. 실제 시험은 선택 모니터의 소유 창만 변경한다.
PLAN-005의 Scp999Smoke는 제품 상태/애니메이션/효과를 검사한다. `-NativeInput`은 소유한 별도 InputProbe의 실제 수신과 공통 상태를 대조하며, foreground 확보 실패 시 합성 입력 전에 중단한다.
실제 실행은 [Godot 도구](godot-tools.md)를 따른다. 모의 Windows 입력·게임 시나리오는 각 기능 PLAN에서 구현한다.

## 공통 실행 경로

- 제품 UI와 하네스는 같은 명령 처리·규칙 검사·상태 조회를 사용한다.
- 실제 게임 상태를 관찰한다. 검증 전용 SCP 행동이나 별도 정답 시뮬레이션을 만들지 않는다.
- 입력 변환과 출력 형식은 달라도 같은 명령의 허용 여부와 상태 변화는 같아야 한다.
- 테스트 초기 조건 준비와 사용자 조작을 구분한다. 상태 주입만으로 UI 조작을 검증했다고 기록하지 않는다.

## 환경과 시간

- 커서·모니터·창 상태·타이핑 활동·시스템 지표를 필요한 범위에서 모의 입력으로 제공한다.
- 모의 환경은 동일한 계약으로 전달한다. 실제 연결 경계도 별도로 검증한다.
- 실제 기다림만 사용하지 않고 같은 게임 갱신 경로에 경과 시간을 전달할 수 있게 설계한다.
- 봉쇄·일시정지 규칙은 시간 제어 중에도 동일하다. 완전한 결정성을 미리 요구하지 않는다.
- 무작위 행동이 있으면 재현에 필요한 시드·입력·실행 조건을 기록한다.
- 시나리오마다 상태와 테스트 저장 경로를 초기화한다. 사용자의 설정·저장을 덮어쓰지 않는다.
- 무한 대기 대신 종료 조건과 시간/단계 한계를 정한다. 한계 초과는 성공이 아니다.

## 시나리오 명세

| 요소 | 기록 내용 |
| --- | --- |
| 기준 | 기획 규칙·관련 PLAN·확인할 동작 |
| 초기 상태 | 개체·발판·모니터·옵션·스트레스 등 |
| 입력 | 공통 명령과 환경 변화의 순서 |
| 진행 | 경과 시간·종료 조건·최대 진행량 |
| 기대 결과 | 관찰 가능한 상태와 유지할 관계 |
| 증거 | 실제 결과·실패 상태·실행 환경·재현 입력 |

## 단계별 확장

- 오버레이: 표시·입력 영역의 일치, 빈 영역 통과, 즉시 숨김.
- 창 연동: 창 이동·최소화·종료, 발판 변경·낙하·착지.
- SCP-999: 추적·타이핑 반응·쓰다듬기와 주변 스트레스 감소.
- SCP-049: 대상 선택·치료·변이·좀비 수명.
- SCP-173: 시선 정지·관찰 해제·시간 경계·이동.
- 시스템·옵션: 지표 반응·강한 연출 OFF·봉쇄 중단과 복귀.

필요한 기능을 구현할 때 해당 시나리오를 추가한다. 모든 후속 기능을 먼저 하네스로 구현하지 않는다.
기획에 없는 기대값을 테스트에 넣어 제품 규칙으로 확정하지 않는다.
로직 시나리오 통과는 실제 화면·Windows 클릭 투과·성능 검증을 대신하지 않는다.

Scp999Smoke는 같은 PlatformWorld를 사용하는 제품 상태/입력/표현을 검사한다. native는 소유한 창 이동·최소화·가림·종료·도약과 점액 부착을 검증하며, 선택적 NativeInput은 별도 소유 수신기의 실제 down/up/키 활동을 확인한다. [최종 증거](../plans/evidence/PLAN-005/run-035/README.md).

Scp049Smoke는 실제 PlagueState의 생성/예약/치료/변이/수명/물리/공통 스트레스를 검사한다. native 입력 시 같은 프로젝트의 미리보기를 먼저 종료 확인하고, 소유한 수신기/입력 HWND와 커서 간섭을 구분한다. [최종 증거](../plans/evidence/PLAN-006/run-026/README.md).

SystemSmoke는 시스템 지표·단일 봉쇄·트레이/핫키·외부 창 효과를 검사한다. `scripts/godot.ps1 -Action SystemSmoke -MonitorId GSM5BDE -EvidenceDirectory <새 회차> -LogFile <새 로그>`를 사용한다. `-Headless`는 규칙/제품 상태, `-NativeInput`은 사용자 입력 시간 승인 후 소유 창 이동·실제 글로벌 단축키·클릭을 추가한다. [PLAN-008 증거](../plans/evidence/PLAN-008/run-001/README.md).

PLAN-015 확장 SystemSmoke는 시작 시 제어창 숨김·SCP 시계 진행·닫기 신호·숨김 버튼·봉쇄 중 열기·재개 시 표시 보존·트레이 등록 실패 복구를 검사한다. 신호/콜백 호출과 실제 X/Alt+F4/트레이 메뉴 조작은 별도 증거다. [시작 관찰](../../scripts/observe-tray-startup.ps1)은 소유 EXE의 창만 읽기 전용으로 관찰하며 초기 표시·종료 로그를 남긴다.

DoctorArtSmoke는 승인 r001 해시·태그/상태·양방향 실제 RGBA·알파 입력·Unshaded 색 불변성을 검사한다. `scripts/godot.ps1 -Action DoctorArtSmoke -Headless` 또는 `-MonitorId GSM5BDE`에 새 EvidenceDirectory/LogFile을 지정한다. `-NativeInput`은 시간 승인 후 별도 수신기 클릭·049/999 겹침 드래그를 추가한다. [PLAN-013 증거](../plans/evidence/PLAN-013/run-001/README.md).

ZombieArtSmoke는 ART-006/007의 해시·게임 시계·소멸/캐시 수명을 검사한다. `scripts/godot.ps1 -Action ZombieArtSmoke -Headless` 또는 `-MonitorId GSM5BDE`에 새 EvidenceDirectory/LogFile을 지정한다. native는9종 아이콘·30프레임·양방향 렌더/알파·효과·봉쇄·정리·짧은 비용을 추가한다. `-NativeInput`은 시간 승인 후 별도 소유 수신기로 구멍/효과/잔상 투과·드래그·회수를 검사한다. [PLAN-014 증거](../plans/evidence/PLAN-014/README.md).

ScpActivationSmoke는 제품 관리자·공용 상태로 8개 조합·봉쇄 중 변경·치료/좀비 중단·설정 오류/복구·신규 등록을 검사한다. `ScpActivationSmoke -Headless` 또는 `-MonitorId GSM5BDE`에 새 EvidenceDirectory/LogFile을 지정한다. 진단 Main은 PersistScpSettings=false 또는 격리 SettingsPath를 사용한다.
