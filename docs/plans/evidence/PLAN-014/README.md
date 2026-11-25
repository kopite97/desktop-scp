# PLAN-014 적용·검증 기록

- 날짜: 2026-11-25. 작업 디렉터리: `C:/Dev/scp_desktop`.
- 대상: 승인 ART-006/007 r001의 제품 통합. Godot4.7.2 .NET/Compatibility/OpenGL, Windows, NVIDIA RTX5070. LG는 `-MonitorId GSM5BDE`로 실제 연결된 모니터를 선택했다.
- C# Build:0오류/0경고. 배포용 내보내기는 실행하지 않았다.
- [실제 모니터 기록](run-010/monitor.json): LG GSM5BDE, 작업 영역 원점(-1920,1061)/크기1920×1032,96DPI. 주 모니터 설정은 변경하지 않았다.
- 승인 export와 제품 PNG/JSON은 바이트·SHA256 동일. [해시](run-019/asset-hashes.json). 사용자 바탕화면 아이콘을 저장소 아트/검수 이미지로 복사하지 않았다.

## 명령과 실행 결과

진단은 `./scripts/godot.ps1 -Action <아래 Action> -EvidenceDirectory docs/plans/evidence/PLAN-014/run-NNN -LogFile docs/plans/evidence/PLAN-014/run-NNN/execution.log`로 실행했다.
headless는 `-Headless`, LG는 `-MonitorId GSM5BDE`, 실제 자동 입력은 `-NativeInput`을 추가했다. run-001의 로그명만 `doctor-headless.log`다. 각 실행은 기존 증거를 덮어쓰지 않았다.

| 회차 | Action/환경 | 판정·범위 |
| --- | --- | --- |
| [001](run-001/results.json) | DoctorArtSmoke/headless | PASS68검사, 기존 본체 계약 |
| [002](run-002/execution.log) | ZombieArtSmoke/headless | FAIL: 드래그 해제 직후 착지를 기다리지 않은 재치료 진단 가정. 실제 Treating까지 제한된 대기를 적용 |
| [003](run-003/results.json) | ZombieArtSmoke/headless | PASS96검사 |
| [004](run-004/results.json) | ZombieArtSmoke/LG | PASS702검사 |
| [005](run-005/results.json) | ZombieArtSmoke/LG/NativeInput | PASS713검사. 사용자30초 입력 승인. 구멍/옛 손 자리/오라/잔상 외부 클릭 투과, 실제 드래그·단일 회수 |
| [006](run-006/results.json) | Scp049Smoke/headless | PASS, 예약·치료·좀비 게임 회귀 |
| [007](run-007/results.json) | StatueSmoke/headless | PASS18검사,173규칙·파손 회귀 |
| [008](run-008/results.json) | SystemSmoke/headless | PASS18검사 |
| [009](run-009/results.json) | DoctorArtSmoke/LG | PASS158검사, 기존049본체 RGBA/입력/상태 |
| [010](run-010/results.json) | OverlayStackSmoke/LG | PASS25검사,1800프레임의30초 겹침/순서. 실제 마우스 입력과 구분 |
| [011](run-011/results.json) | ZombieArtSmoke/LG | PASS703검사. 발 조명 불변성과 실제 측정 시간 기록 추가 |
| [012](run-012/execution.log) | OverlayStackSmoke/LG/NativeInput | FAIL:049시작점. 예전 도형 좌표와 수동 갱신 진단의 커서 이동 후 미갱신을 확인. 실제 알파 픽셀 선택 및 Publish 후 HWND 소유권 검사로 수정. Win32 소유권 단언은 유지 |
| [013](run-013/results.json) | SystemSmoke/LG | PASS28검사, 제품 봉쇄/복귀/종료 |
| [014](run-014/results.json) | Scp999Smoke/headless | PASS,999공통 물리·상태 회귀 |
| [015](run-015/results.json) | OverlayStackSmoke/LG/NativeInput | PASS197검사. 사용자 재검사 승인.999/049/일반 아이콘/좀비 실제 겹침 드래그·해제·최상단 순서·단일 회수·숨김 영역 외부 입력 |
| [016](run-016/live-preview.log) | 직접 Godot 실행 | FAIL: 실행 스크립트를 거치지 않아 DOTNET_ROOT 누락. SDK 신규 설치 없이 기존 실행 스크립트로 복구 |
| [017](run-017/live-preview.log) | Run/LG | READY, 실제 Shell 아이콘13개 확인. 사용자 피드백: 앞으로 이동하지만 뒤로 걷는 것처럼 보임. 제품 시각 승인 보류. Exit로 정상 종료 |
| [018](run-018/results.json) | ZombieArtSmoke/headless | PASS. r001 보행의 접지 구간 방향 검사와 프레임 시간 검사 추가 후 재검증 |
| [019](run-019/results.json) | ZombieArtSmoke/LG | PASS. 수정 보행 및 기존9종/30프레임/양방향 RGBA·입력 마스크·효과·소멸·정리 재검증 |
| [020](run-020/live-preview.log) | Run/LG | 수정 보행 정식 실행 READY. 사용자: 마우스와 무관하게 아이콘 복제본이 계속 깜빡임. 최종 시각 승인 보류, Exit 정상 종료 |
| [021](run-021/native-redraw.json) | ZombieArtSmoke/LG | FAIL 의도 재현: 정지 아이콘 Publish40회에서 native 위치 변경·배경 지우기·비클라이언트 그리기 각각40회 발생 |
| [022](run-022/results.json) | ZombieArtSmoke/LG | PASS717검사. 반복 빈 MousePassthroughPolygon 설정 제거 후 동일40회 모두0회. 기존 렌더·입력 마스크·효과·정리도 재검증 |

| [023](run-023/results.json) | ZombieArtSmoke/LG/NativeInput | PASS728검사. 사용자30초 입력 승인. 깜빡임 수정 뒤 외부 클릭 투과·실제 회수/드래그·소멸 투과 정상 |
| [024](run-024/live-preview.log) | Run/LG | READY. 사용자 “깜빡임·보행 수정 및 정식 적용 승인”. 정식 실행 유지 |

## 확인 범위와 수정 근거

- 9종 테스트 소유 아이콘: 사각·원·비대칭·구멍·큰 투명 여백·좁은 형태·폭1/2·완전 투명. 양방향30프레임 모두 실제 viewport RGBA/alpha16 입력과 비교했다. 효과16프레임·네 모서리 마스크·Unshaded 낮/밤·광원에서도 비교했다.
- 치료율1/1.5, 성공0초 일치, 등장 중 이동, 드래그/실제 착지, 방향 변경 위상, 정상 데이터와 좀비의 제거 구분, 중복 회수,360/660ms 투명 꼬리, 카탈로그에서 빠진 외형의 잔상 참조, 봉쇄 동결·복귀·종료를 검사했다.
- r001 move의 접지 발이 몸체 기준 앞으로 이동하고 들린 발이 뒤로 돌아가는 순서였음을 제작 스크립트와 실제 시트의 발바닥 픽셀로 확인했다. 사용자 피드백에 따라 제품의 move만16→9 역순으로 매핑했다. 각 원본 프레임 ms와 총840ms·좌우 위상·발끝 방향을 유지한다. 나머지 태그·승인 원본/제품 PNG/JSON은 변경하지 않았다.
- run-018/019는 양발·양방향 접지 픽셀이 진행 방향 반대로 이동하는 검사를 추가했다. run-005/015의 실제 외부 입력은 이 재생 순서 수정 전 결과다. 모든 동일 프레임의 마스크를 다시 확인했으며 OS 입력 구현은 변경하지 않았다.
- 스냅샷은 장식 전용이며 게임 객체 수·보상·예약에 재등록하지 않는다.8개 좀비/8개 잔상 및 종료 뒤 창 정리를 검사했다.
- 깜빡임 원인: PlagueActorWindow.Publish가 아이콘마다 빈 MousePassthroughPolygon을 반복 대입했다. [Godot Window setter](https://github.com/godotengine/godot/blob/4.7/scene/main/window.cpp#L1794-L1800)는 동일 값도 전달하고, [Windows 구현](https://github.com/godotengine/godot/blob/4.7/platform/windows/display_server_windows.cpp#L2049-L2063)은 SetWindowRgn(hwnd,null,TRUE)로 배경을 다시 그린다. 마우스 위치·이미지·발 프레임이 같아도 발생했다.
- 진단 소유 HWND의 Win32 메시지를 SetWindowSubclass로 관찰해 수정 전40회/수정 후0회를 확인했다. 프로브는 검사 직후 해제한다. 기존 viewport RGBA 검사는 이 native 배경 지우기를 관찰하지 못했으므로 새 검사로 보완했다. 제품의 빈 polygon은 기본 상태를 유지하고 실제 alpha16 Hit·MousePassthrough·외부 투과 경로는 그대로 사용한다.

## 비용과 한계

- run-011:8개 좀비+오라1개(9창)90프레임1.4884306초,CPU281.25ms. 동일8개 좀비/오라0개는1.4788502초,CPU234.375ms. 기록 working set228601856바이트. 같은8개 몸체 조건에서 오라1개의 추가 비용을 보는 짧은 관찰이며 동일 창 수 비교나 장기 무회귀 판정이 아니다.
- GPU·고부하·장기 실행·혼합 DPI·실제 모니터 탈착은 미측정. 화면 변경 정리는 상태 경로 검사다. SCP-173의 실제 입력 회귀는 이번에 추가 실행하지 않았으며 규칙/파손과 SystemSmoke를 검사했다.
- native 테스트는 소유 fixture를 사용한다. 실제 Shell 취득은 run-017 제품의 카탈로그13개로 확인했고, 사용자 이미지 캡처를 아트 산출물에 저장하지 않았다.
- 2026-11-25 run-024에서 사용자 “깜빡임·보행 수정 및 정식 적용 승인”. PLAN-014 completed. 문서 검사도 완료 상태로 실행했으며, 화면 승인은 별도 사용자 확인으로 기록한다.
