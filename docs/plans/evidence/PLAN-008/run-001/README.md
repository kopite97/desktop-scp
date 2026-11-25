# PLAN-008 시스템·옵션·봉쇄 검증 색인

## 환경과 방법

- 2026-11-25 Windows, Godot4.7.2 .NET/OpenGL, LG GSM5BDE 물리 원점(-1920,1061), 1920×1080/작업 영역1920×1032, DPI96.
- 제품 Main/SystemController/SystemOptions와 동일한 명령을 사용하는 SystemSmoke. 모의 지표를 실제 상태에 전달하며 실제 CPU/RAM 부하는 생성하지 않았다.
- `scripts/godot.ps1 -Action Build`: 경고0/오류0.
- `-Action SystemSmoke -Headless -EvidenceDirectory docs/plans/evidence/PLAN-008/run-001 -LogFile docs/plans/evidence/PLAN-008/run-001/execution.log`.
- native는 `-Headless` 대신 `-MonitorId GSM5BDE`. run-004는 `-NativeInput` 추가. 사용자가 “지금 20초간 입력 검사 진행”으로 승인했다. 수신기는 하네스가 생성한 별도 프로세스이며 종료 후 커서/포커스를 복원했다.

## 회차와 판정

| 회차 | 결과 | 증거 |
| --- | --- | --- |
| run-001 | headless 규칙·제품 상태18개 PASS | [결과](results.json), [로그](execution.log) |
| run-002 | FAIL: 결과 JSON은27개 성공이나 Godot 메인 Window.Hide 오류가 있어 전체 실패로 판정 | [로그](../run-002/execution.log) |
| run-003 | Windows ShowWindow로 수정 후 LG27개 PASS | [결과](../run-003/results.json), [로그](../run-003/execution.log) |
| run-004 | 실제 외부 소유 수신기 입력·창 이동 포함43개 PASS | [결과](../run-004/results.json), [로그](../run-004/execution.log), [수신 집계](../run-004/receiver.json) |
| run-005 | SCP-049 headless 회귀43개 PASS | [결과](../run-005/results.json) |
| run-006 | SCP-173 headless 회귀18개 PASS | [결과](../run-006/results.json) |
| run-007 | 공통 창 발판 headless 회귀36개 PASS | [결과](../run-007/results.json) |
| run-008 | 수집 비용/컨트롤 크기 검사 추가 LG28개 PASS | [결과](../run-008/results.json) |
| run-009 | 렌더 프레임 완료 후 hot/cool 화면 확인, LG28개 PASS | [결과](../run-009/results.json), [로그](../run-009/execution.log), [컨트롤](../run-009/controls.png), [열기](../run-009/companion-hot.png), [정상](../run-009/companion.png) |

## 확인한 제품 동작

- 임계값 경계·이력·음소거·유효하지 않은 값·5초 표본 만료·세대 폐기. 모의 고부하가 실제999 속도/형태와049 이동/치료 속도,173 공통 스트레스에 반영된다.
- 실제 집계 지표→제품 연결: run-009 CPU15.41%, RAM45%, 출력 볼륨48%, 음소거false. 실제 수집 호출 최대2.8083ms(짧은 해당 실행 표본). 1초당 단일 작업 상한과 UI 비동기 연결을 확인했다. 장시간 성능·누수 기준 달성으로 확대 해석하지 않는다.
- 999 세로75% 표현과 입력 윤곽의 같은 변환을 확인했다. 최종 PNG를 열어 납작해진 몸체/정상 몸체와 컨트롤700px 내 전체 버튼 배치를 시각 확인했다. run-008 정상 이미지가 직전 렌더를 포함할 수 있어 run-009를 최종 화면 증거로 사용한다.
- UI 버튼의 실제 연결 신호, 등록된 NativeMenu 콜백, 타 프로세스에서 실제 SendInput Ctrl+Shift+L이 같은 봉쇄 상태를 바꾼다. OS 트레이와 메뉴3개 생성·라벨 갱신·해제 후 재등록을 확인했다.
- 봉쇄 시 개체/컨트롤/셔터가 사라지고 위치·시간·스트레스가 정지한다. 별도 수신기가 실제 클릭down/up을 받는다. 복귀 시 시간 따라잡기 없이 기존 상태를 유지한다.
- 실제 소유 창: OFF 무이동, ON 이동, OFF 즉시 복원, 봉쇄 취소/복원, 사용자가 옮긴 위치를 덮어쓰지 않는 취소를 검증했다. 실제 사용자 작업 창을 효과 대상으로 사용하지 않았다.
- 종료 시 효과·핫키 스레드·등록·트레이·지표 소유자 정리. 재등록 성공. 아이콘 취득 세대/취소/캐시 수명은 [PLAN-012 최종 검사](../../PLAN-012/run-014/README.md)의 동일 서비스 검증과 이번 전체 봉쇄 연결 검사로 확인했다.

## 한계와 제외

- 임시049/173 본체·좀비 팔다리·열기/셔터/불안 표현. 정식 아트 적용 완료가 아니다.
- 트레이 OS 메뉴의 자동 마우스 클릭, 경보음 수동 청감, 실제 과부하, 장시간 CPU/메모리 측정, Explorer 재시작, 오디오 드라이버 영구 정지, 실제 혼합 DPI/모니터 탈착, 권한이 높은 외부 창, 배포용 내보내기는 수행하지 않았다.
- native 창 효과 대상은 제목 테두리 있는 일반 전경 창이며 최대화/최소화/자기 앱/사용자 조작 중 창을 제외한다. 강제 프로세스 종료 후 위치 복원은 보장하지 않는다.
- 제품 구현·명령·경계는 [TECH-014](../../../../decisions/TECH-014-system-options-and-lockdown.md), [제품 규칙](../../../../game-design/system-reactions.md).
