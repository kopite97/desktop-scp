# PLAN-013 승인된 역병의사 r001 정식 적용 검증

## 환경과 대상

- 2026-11-25 Windows / Godot4.7.2 .NET / OpenGL Compatibility / RTX5070. LG GSM5BDE, 원점(-1920,1061), 1920×1080, 작업 영역1920×1032, DPI96.
- ART-004 승인 r001 PNG·contract를 `GodotProject/Assets/Sprites/characters/Scp049/`에 복사했다. SHA256 동일성은 진단에서 검사했다. Art 원본/제출 리비전은 수정하지 않았다.
- nearest2배,128×128 창, 피벗(64,112), Unshaded/Mix/중립 부모. 7열6행42프레임10태그. 고정 명암·straight RGBA 유지.
- DoctorAnimation은 PlagueState가 소유하고 게임 tick/명령에서만 갱신한다. Publish는 프레임을 표시하며 시간을 추가 진행하지 않는다. 치료의 논리 완료 시점을 아트 재생 때문에 지연시키지 않는다.
- 본체는 원본 알파를 사전 계산해 도구·가방과 투명 구멍까지 픽셀 단위로 판정한다. 원본1px를2px로 표시하며 반전 입력은63-x, 연속 접점 좌표는64-x다. OS 전달은 기존 layered/transparent 경로, 창 순서는 기존 공통 관리자다.

## 실행과 판정

`scripts/godot.ps1 -Action Build`: 경고0/오류0.

`-Action DoctorArtSmoke -Headless -EvidenceDirectory docs/plans/evidence/PLAN-013/run-001 -LogFile docs/plans/evidence/PLAN-013/run-001/execution.log`.
native는 `-Headless` 대신 `-MonitorId GSM5BDE`, run-003은 `-NativeInput` 추가. 사용자 “지금 20초간 입력 검사 진행” 승인 후 실행했고 종료 시 수신기 정리·커서/포커스를 복원했다.

| 회차 | 실제 판정 | 증거 |
| --- | --- | --- |
| run-001 | headless68개 PASS: 승인 해시, 계약/각 프레임 시간/루프,10태그 게임 연결, 치료 취소·좌우 접점·봉쇄 시간·과부하·물리 | [결과](results.json), [로그](execution.log) |
| run-002 | LG158개 PASS:42프레임×양방향 RGBA/알파/입력 마스크, 낮/밤 불변성, 접지, 실제 치료, 숨김/복귀/종료 | [결과](../run-002/results.json), [로그](../run-002/execution.log) |
| run-003 | LG 실제 입력 포함166개 PASS | [결과](../run-003/results.json), [로그](../run-003/execution.log), [수신 집계](../run-003/receiver.json) |
| run-004 | 기존049 규칙·물리43개 PASS | [결과](../run-004/results.json) |
| run-005 | 기존173 규칙18개 PASS | [결과](../run-005/results.json) |
| run-006 | 시스템/봉쇄 LG28개 PASS | [결과](../run-006/results.json), [로그](../run-006/execution.log) |
| run-007 | 최신 제품 LG 실행·사용자 화면 승인 | [로그](../run-007/live-preview.log) |

## 실제 화면·입력·조명

- 원본 프레임별128×128 실제 viewport를 원본 PNG와 비교했다. 42×2 렌더 모두 색/알파·nearest 및 클릭 마스크 불일치0. [대기](../run-003/frame-00.png), [치료](../run-003/frame-17.png), [종료](../run-003/frame-23.png)를 시각 확인했다.
- 실제 태그는 대기→접근→치료 시작/반복/종료, 드래그/낙하/착지, 발판 이동/균형/회복, 인접 발판 점프 시나리오에서 모두 도달했다. 반대편 대상 치료 시 속도가0이어도 얼굴·도구를 반전한다.
- CanvasModulate 어두운 푸른색과 강한 따뜻한 Light2D를 넣어도 본체 색/알파가 원본과 동일했다. 동적 노멀/반사 맵을 추가하지 않았다.
- 별도 소유 수신기가 투명 모서리와 숨긴 본체 위치의 실제 클릭down/up을 받았다. 실제 오른쪽 드래그/해제가 제품 위치에 반영됐다. 999와 겹치는 드래그에서 공통 창 순서 변경이 검사 구간4회 이내로 유지됐다.
- 비용은 동일128×128 본체 및 같은 창 수로 조건별60프레임 관찰했다. run-003 광원 조건0.994초/프로세스 CPU78.125ms, 중립0.9999초/CPU78.125ms, working set245,653,504→245,518,336bytes. 짧은 표본이며 GPU·장기 누수·완전한 비용 동등성은 미측정이다.

## 사용자 확인과 남은 범위

- 2026-11-25 LG의 실제 제품 크기·색감·이동·치료 동작 확인 요청에 사용자 **“정식 적용 화면 승인”**. 원본 r001 승인과 별도로 제품 적용 화면 확인을 기록했다.
- run-007 실제 GUI 프로세스와 `SCP049_STARTED body=ART-004-r001`·SITE_DESKTOP_READY 확인. 완료 보고 시 LG에서 계속 실행하도록 남긴다. 강한 연출은 기본OFF다.
- 좀비 팔다리·변이 효과와173 본체 정식 아트는 제외. 실제 혼합DPI/모니터 탈착·장기 성능·배포용 내보내기는 이번에 검증하지 않았다.
- 한 번의 클릭 직전에 커서가 이동하는 극단 조건에는 기존 픽셀 투과의 프레임 갱신 지연 한계가 유지된다. 테스트는 실제 입력 전 프레임 반영을 기다렸으며 모든 이동 속도를 보장하지 않는다.
