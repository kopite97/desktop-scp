# PLAN-016 실행 증거

- 날짜: 2026-11-25. Windows 11, Godot 4.7.2 stable mono, .NET 8.0.318/net8.0, Compatibility/OpenGL, NVIDIA RTX 5070.
- 작업 루트: `C:/Dev/scp_desktop`. native는 LG `GSM5BDE`, 물리 원점 (-1920,1061), 1920×1080, 작업 높이 1032, DPI 96. 다른 주 모니터 설정은 변경하지 않았다.
- 실행: `scripts/godot.ps1 -Action <진단> [-Headless | -MonitorId GSM5BDE] -EvidenceDirectory <회차> -LogFile <회차/로그>`; 새 활성화/native 장면은 TimeoutSeconds 120. 기존 좀비·의사 native는 승인 아트 렌더 회귀이며 NativeInput을 사용하지 않았다.
- 사용자 설정을 읽거나 덮어쓰지 않도록 진단 Main은 PersistScpSettings=false 또는 회차 내 SettingsPath를 사용했다. 실제 EXE도 `--scp-settings-file`로 격리했다.
- 이번 실제 UI 입력은 computer-use 스킬의 sky API로 제어창만 조작했다. 외부 효과는 별도 소유 InputProbe 창만 대상으로 했고 합성 키/드래그는 실행하지 않았다.

## 실행 목록

| 실행 | 대상 | 실제 결과 |
| --- | --- | --- |
| [run-001](run-001/results.json) | SystemSmoke headless | PASS 18개 |
| [run-002](run-002/results.json) | Scp049Smoke headless | PASS 43개 |
| [run-003](run-003/results.json) | StatueSmoke headless | PASS 18개 |
| [run-004](run-004/results.json) | ZombieArtSmoke headless | PASS 109개 |
| [run-005](run-005/results.json) | 초기 ScpActivationSmoke headless | PASS 64개 |
| [run-006](run-006/results.json) | 초기 ScpActivationSmoke Windows | PASS 69개 |
| [run-007](run-007/results.json) | SystemSmoke Windows | PASS 39개 |
| [run-008](run-008/results.json) | ZombieArtSmoke Windows | PASS 717개 |
| [run-012](run-012/results.json) | DoctorArtSmoke Windows 수정 후 | PASS 158개 |
| [run-013](run-013/results.json) | StatueSmoke Windows | PASS 29개 |
| [run-014](run-014/results.json) | ScpActivationSmoke Windows·실제 UI | PASS 70개 |
| [run-016](run-016/results.json) | 중간 EXE 저장 복원 관찰 | PASS: 240표본, 표시 개체 395표본, 예상 밖 제어창 0, 종료 0 |
| [run-018](run-018/results.json) | ScpActivationSmoke Windows·외부 효과 소유자 취소 | PASS 75개 |
| [run-019](run-019/results.json) | ScpActivationSmoke headless | PASS 65개 |
| [run-020](run-020/results.json) | 최종 SystemSmoke headless | PASS 18개 |
| [run-023](run-023/results.json) | 최종 EXE 선택 복원 관찰 | PASS: 237표본, 표시 개체 391표본, 예상 밖 제어창 0, 종료 0 |
| [run-024](run-024/results.json) | 최종 EXE 모두 OFF 관찰 | PASS: 232표본, 표시 개체 0표본, 예상 밖 제어창 0, 종료 0 |
| [run-025](run-025/results.json) | 최종 활성화 headless·추가 입력/아이콘 세대 검사 | PASS 70개 |
| [run-026](run-026/results.json) | Scp999Smoke headless | PASS 66개 |

| [run-027](run-027/results.json) | 최종 활성화 headless, 모두 OFF 입력 중단 조건을 명시적으로 결합한 검사 | PASS 70개 |

## 준비·실패·수정

- 최초 컴파일에서 분리 전 PlagueController.Icons를 사용하는 ZombieArtSmoke 구성이 남아 실패했다. 진단에도 공용 DesktopObjectController를 조립하도록 수정하고 다음 빌드가 통과했다. 이후 진단 null 경고 정리 중 중복 `!`로 1회 컴파일 실패했으며 즉시 수정했다. 이 두 컴파일 출력은 대화 도구 출력에 남아 있고 별도 로그 파일은 없다. run-009는 생성하지 않았다.
- [run-010 Import](run-010/import.log): 새 C# UID/리소스 스캔 성공. 일반 C# 빌드는 최종 경고 0·오류 0이었다.
- [run-011 실패](run-011/doctor.log): 치료·렌더 검사는 통과했으나 숨김 후 복원에서 본체가 공용 대상의 이전 숨김 상태를 참조해 남아 있었다. 본체 창은 Doctor.Visible, 복제 창은 World.Visible/소유자 활동을 판단하도록 수정했다. run-012의 동일 158개 검사가 통과했다.
- [run-017 환경 실패](run-017/activation.log): 외부 효과 수신기의 전경 포커스가 Windows에서 거부되었다. 사용자 창으로 대체하거나 입력을 보내지 않았다. run-018은 computer-use로 관찰한 소유 수신기만 활성화하고 동일 조건을 재검사했다. 실제 이동 후 소유 SCP OFF에서 즉시 취소·복원 확인.
- [run-015 export](run-015/export.log)는 중간 실행본, [run-022 export](run-022/export.log)는 최종 제품 코드의 로컬 EXE 내보내기 성공이다. 이후 변경은 진단 검사 추가와 주석/문서뿐이다.
- [run-021 환경](run-021/environment.log): READY·SMOKE_PASS·EXIT_REQUESTED·STOPPED 확인.

## 실제 UI와 EXE

- run-014: 초기 모두 OFF 제어창에서 실제 마우스로 173 ON → 049 ON → 049 OFF → 999 ON. 체크·활성/비활성 상태·저장 안내 변화를 직접 확인했다. 사용자 입력 감지로 한 번 재관찰한 뒤 조작을 재개했다.
- 실제 제목 표시줄 X 클릭 후 제어창은 목록에서 사라지고 SCP-999·173 창만 남았다. [저장 파일](run-014/restart.json), [마지막 상태](run-014/ui-final-state.json), [로그](run-014/activation.log)에 선택 999=true/049=false/173=true와 controls=false를 대조했다. 이 진단은 자동 입력을 꺼 두므로 keyboard=false는 제품 일반 실행의 감시 상태 측정이 아니다.
- [제어창 렌더](run-018/controls.png)는 native 하네스의 173 단독 활성 상태 캡처다. 실제 UI 클릭의 스크린샷은 도구 결과로 관찰했으며 이 PNG를 그 클릭 시점의 캡처로 주장하지 않는다.
- 최종 EXE: `.artifacts/PLAN-016/windows-x64/SiteDesktop.exe`. PCK·data·IconWorker 폴더를 함께 유지한다.
- run-023: `observe-tray-startup.ps1 -Executable .artifacts/PLAN-016/windows-x64/SiteDesktop.exe -EvidenceDirectory docs/plans/evidence/PLAN-016/run-023 -SettingsFile docs/plans/evidence/PLAN-016/run-014/restart.json -ExpectedActors 'SCP-999','SCP-173'`.
- run-024: 같은 실행 파일, SettingsFile은 run-018/restart.json, ExpectedActors는 빈 배열. 표시 SCP 0, 제어창 노출/포커스 이상 0, 정상 종료.
- 모든 테스트 소유 프로세스는 종료했다. 트레이 메뉴의 실제 OS 클릭은 PLAN-015의 기존 사용자 확인을 재사용하며 이번에는 SystemSmoke의 등록된 콜백·메뉴 텍스트·숨김·실패 fallback 회귀를 확인했다. 새 실제 UI 검증은 SCP 선택·저장·X 경로다.

## 범위와 한계

- 게임 규칙: 8조합, 봉쇄 중 설정, 치료 보존/취소, 늦은 변이/공격 거부, 좀비만 시간 정지, 스트레스 제외, 20회 전환, 등록 확장, 설정 오류/복구, 모든 본체 드래그 해제, 아이콘 세대 폐기/복귀를 확인했다.
- Windows: 실제 본체/복제 창의 숨김·복귀, 독립 173, 신규 설정 UI, 소유 외부 창 효과 취소/복원, 최종 EXE 시작 복원을 확인했다.
- 기존 알파 입력/겹침 알고리즘과 아트 원본은 변경하지 않았다. native 아트 픽셀/입력 윤곽 검사는 실행했지만 기존 NativeInput 전체 합성 입력 묶음은 다시 실행하지 않았다. 과거 실제 클릭 투과 증거를 이번 새 실행으로 표현하지 않는다.
- 시작 관찰은 약 10ms 요청 간격의 표본이다. Godot/Windows가 1px 요청을 64px 투명 부트 창으로 보정하는 PLAN-015의 한계는 유지한다. 모든 순간의 HWND 부재를 보장하지 않는다.
- 다른 PC·혼합 DPI 전체 조합·장시간 성능·Explorer 재시작·외부 배포는 미검증이다. 기존 아트 native 진단의 짧은 비용 출력은 성능 무회귀 판정으로 쓰지 않는다.

마감 문서 검사: PLAN 16개·active 0개·TECH 16개, 구조·메타데이터·목록·로컬 링크 PASS. 기존 추적 파일의 diff 공백 오류 없음.
