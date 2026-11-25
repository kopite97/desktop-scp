---
id: PLAN-012
type: development
status: completed
title: 실제 바탕화면 아이콘의 런타임 복제와 게임 연결
updated: 2026-11-25
---

# PLAN-012 실제 바탕화면 아이콘의 런타임 복제와 게임 연결

## 목표와 범위

- 사용자 근거: 2026-11-25 실제 바탕화면 아이콘 이미지를 런타임에 가져와 같은 모양의 게임 오브젝트를 만드는 방향으로 PLAN 수정 및 불필요한 개발/ART 계획 검토 요청. 당시 요청은 계획 정리였으며, 이후 사용자 “Plan을 차례대로 진행해줘”로 PLAN-012 → PLAN-007 → PLAN-008 순차 실행을 승인받았다.
- 목표: 고정 .tmp/쓰레기 도형 대신 Shell에서 취득한 실제 아이콘으로 게임 데이터를 생성하고, 기존 SCP-049 예약·치료·좀비 수명·회수와 연결한다.
- 포함: Desktop 항목 발견, 비동기 이미지 취득, 세션 캐시, Godot 텍스처와 알파 입력 영역, 실패/갱신/종료 처리, 복제본 이미지의 좀비 전환 후 유지, 실제 LG 화면 비교.
- 제외: 실제 파일·아이콘 교체/이동/삭제/숨김, 원래 위치에서 원본이 사라지는 착시, 스크린샷/OCR 인식, 이름 라벨/썸네일 복제, SCP-173 AI 구현, 정식 아트 신규 제작과 ART-004/006/007의 정식 적용.
- 공통 좀비 부착 위치·치료 효과 연결 계약은 이번에 마련한다. 정식 부착 아트가 미수령이면 기존 임시 도형을 몸체를 가리지 않는 부착 표현으로 사용하고 임시임을 기록한다. 취득 아이콘은 임시 도형으로 대체하지 않는다.

## 기준 문서와 선행 조건

- 제품 규칙 원본: [아이콘 복제 기획](../game-design/desktop-icon-clones.md), [기획 MAP](../game-design/README.md), [로드맵](../game-design/roadmap.md).
- 완료 기반: [PLAN-006](PLAN-006-scp-049.md)의 게임 상태·물리·치료, [PLAN-011](PLAN-011-overlay-stacking.md)의 공통 창 순서. 기존 completed 기록은 보존한다.
- 지침: [아키텍처](../agents/architecture.md), [Windows](../agents/windows-integration.md), [구조](../agents/project-structure.md), [하네스](../agents/harness.md), [검증](../agents/verification.md).
- 관련 제작: [ART-004](../../Art/docs/plans/ART-004-scp-049-character.md) 승인본 유지, [ART-005](../../Art/docs/plans/ART-005-dummy-data.md) 취소, [ART-006](../../Art/docs/plans/ART-006-scp-049-2-zombie.md) 부착 요소로 변경, [ART-007](../../Art/docs/plans/ART-007-scp-049-mutation.md) 공통 효과 유지. ART-006/007 완료는 런타임 취득 구현의 선행 조건이 아니다.

## 구현 계획과 책임

1. **환경 관찰**: Windows 경계에서 실제 Desktop 뷰/최상위 Shell 항목을 읽는다. 사용자·공용·리디렉션 경로, 가상 항목, 숨김·빈 목록을 구분한다. 폴더 내용을 재귀 탐색하지 않는다. Shell 네임스페이스 전체를 화면에 보이는 바탕화면 항목과 동일시하지 않는다.
2. **취득**: `IShellItemImageFactory.GetImage`의 아이콘 경로를 우선 검토한다. COM apartment를 명시한 전용 작업 경계에서 실행하고 Godot 메인 스레드를 막지 않는다. HWND 없는 열거 실패는 조용히 처리하고 인증·삽입 요청창을 띄우지 않는다. 배지·사용자 지정 아이콘 정확도가 부족하면 아이콘 추출 경로를 비교해 지원 수준을 확정한다.
3. **정규화**: HBITMAP의 채널 순서·알파/premultiplied 여부를 확인해 Godot RGBA로 변환한다. 필요한 표시 크기의 이미지와 원본 종횡비·여백을 보존한다. 무조건 32×32/nearest 2배를 적용하지 않는다. 정상 상태는 Unshaded·중립색으로 표시한다.
4. **캐시/소유**: Main이 취득 서비스와 세션 캐시를 소유한다. OS 식별자는 Windows 경계에 두고 게임 상태에는 경로 대신 불투명 appearance ID를 연결한다. PlagueState가 객체 ID·예약·치료·수명을 계속 소유하고, 표현 서비스가 이미지·텍스처·합성/입력 윤곽을 담당한다. 중복 취득을 합치고 살아 있는 복제본이 참조하는 이미지는 유지한다.
5. **비동기 수명**: 새로고침은 시작 시와 제한 주기로 수행한다. 주기·후보 수·동시 작업·캐시 바이트 상한은 활성화 시 시험값으로 명시하고 측정한다. 세션/세대 ID로 늦은 결과를 무시한다. Shell 호출이 멈추는 경우 단순 CancellationToken만으로 중단된다고 가정하지 않고 격리·제한 대책을 검증한다. 모든 HBITMAP/PIDL/COM 및 Godot 자원은 소유 지점에서 해제한다.
6. **게임 연결**: 기존 스폰·상한·수명 정책을 재사용하고 유효 이미지가 없으면 스폰을 건너뛴다. 데이터→좀비 전환에서 appearance ID를 유지한다. 아이콘 몸체는 반전하지 않고 팔다리 방향만 전환한다. 몸체 사각/정규화 부착점·알파 임계값·효과 영역을 ART-006/007 전달 계약과 대응시킨다.
7. **입력/정리**: 복제본 클릭만 회수하고 외부 앱 실행은 연결하지 않는다. 반투명 경계·아이콘 내부 구멍·부착 요소 입력을 실제 픽셀과 비교한다. 효과는 클릭 투과한다. 기존 OverlayStackCoordinator를 사용하며 봉쇄/복귀/모니터 변경/종료를 같은 경로로 검증한다.

## 결과물 저장 위치

기준 루트 C:/Dev/scp_desktop. 아래 제품/증거 파일은 예정이며 구현 시 실제 책임에 맞춰 확정한다. 빈 디렉터리나 런타임 아이콘 원본 에셋은 만들지 않는다.

| 산출물 | 예정 경로·처리 |
| --- | --- |
| Windows 취득 경계 | `GodotProject/Scripts/Platform/Windows/DesktopIconProvider.cs` 신규 |
| 엔진 이미지 수명/표현 | `GodotProject/Scripts/Overlay/DesktopIconAppearance.cs` 신규 |
| 기존 게임 연결 | `Scripts/App/Main.cs`, `Scripts/Simulation/PlagueState.cs`, `Scripts/Overlay/PlagueController.cs`, `PlagueActorWindow.cs` 변경 (GodotProject 아래) |
| 진단 | `GodotProject/Scenes/Diagnostics/DesktopIconSmoke.tscn`, `GodotProject/Scripts/Diagnostics/DesktopIconSmoke.cs`, `scripts/godot.ps1` 연결 |
| 실행 증거 | `docs/plans/evidence/PLAN-012/run-001/README.md`, `results.json`, `execution.log`, 테스트 소유 아이콘 비교 이미지. 회차 증가·기존 보존 |
| 런타임 이미지 | 세션 메모리 캐시. Art 또는 제품 Assets에 사용자 아이콘을 저장하지 않음 |

## 작업 목록

- [x] 실제 Desktop 열거·이미지 취득 대표 샘플과 지원 유형 확인
- [x] 크기/배율·배지·알파·부착점·캐시 상한·갱신/취소 시험값 확정
- [x] 비동기 취득·캐시·자원 수명 및 기존 게임 상태 연결
- [x] LG에서 복제 외형·치료·좀비·클릭 투과·겹침·숨김/복귀 검증
- [x] 문서·실행 증거·지원 한계·제품 화면 사용자 확인 기록

## 완료 기준과 검증 방법

| 기준 | 검증 |
| --- | --- |
| 실제 아이콘을 사용함 | 테스트 소유 일반 폴더·바로가기·사용자 지정 아이콘과 실제 Chrome 바로가기(존재 시)를 Shell/Windows 표시 및 게임 복제본과 나란히 비교. 없는 유형은 통과로 간주하지 않고 대체 fixture/한계를 기록 |
| 외형 보존 | 동일 크기 원본 RGBA→Godot 비교, 알파 테두리·채널·종횡비·밝음/어두움 배경. 라벨/선택 강조/썸네일 제외 및 배지 제한을 명시. 사용자 화면 확인 |
| 게임 상태 일관성 | 치료 전후 같은 appearance ID, 중복 예약·대상 소실·수명/클릭 회수·좀비 상한·폴더/로고 몸체 유지. 173은 후속 PLAN-007에서 검증 |
| 실패 복구 | 빈 Desktop/아이콘 실패/Explorer 재시작/열거 중 항목 변경/늦은 결과/원본 삭제를 테스트 소유 항목으로 재현. 기존 복제본 유지, 새 스폰 정책, 재시도 폭주 없음 |
| 입력·좌표 | LG 음수 원점, 사용 가능한 DPI 조합, 투명 구멍·효과 영역 아래 테스트 창의 실제 입력 수신, 드래그 겹침·모니터 변경. 미보유 환경은 미검증으로 기록 |
| 정상 종료·비용 | 취득 중 봉쇄/종료·재실행, 반복 갱신 중 GDI/COM/텍스처·메모리 수명, UI 응답·캐시 재사용을 유한 시간 측정. 무제한/장기 부하 시험으로 확대하지 않음 |
| 원본 불변 | 제품에 쓰기/실행 경로 없음 검토, 테스트 소유 원본의 내용·경로·아이콘 위치 전후 동일. 실제 파일 내용 열람을 게임 기능에 추가하지 않음 |

## 기존 PLAN/ART 검토 결과

| 대상 | 처리와 이유 |
| --- | --- |
| PLAN-001~006, 010~011 | completed 보존. 구현된 기능은 재사용하며 과거 증거를 새 기능 완료로 해석하지 않음 |
| PLAN-007 | 유지·수정. 파손 대상은 PLAN-012 복제본으로 연결, 049와 같은 예약 상태 사용 |
| PLAN-008 | 유지·수정. 봉쇄/복귀/종료 시 새 취득 서비스와 복제본 수명 포함 |
| PLAN-009 | 기존 rejected 유지. 아이콘 방향 변경과 무관 |
| ART-001~004 | completed/승인본 보존. 049 본체는 여전히 필요 |
| ART-005 | rejected. 고정 종이/휴지통 그림은 실제 아이콘 취득으로 대체. 제작물 없음, 삭제할 자산 없음 |
| ART-006 | proposed 유지·범위 수정. 고정 파일 몸체 대신 임의 아이콘에 붙이는 좀비 팔다리·표정·동작 |
| ART-007 | proposed 유지·계약 수정. 아이콘마다 다시 그리지 않는 공통 치료·변이 효과 |

권장 개발 순서는 PLAN-012 → PLAN-007 → PLAN-008이다. ART-006/007은 부착 계약을 확인한 후 별도 실행하며, ART-004/006/007 정식 적용은 승인본 수령 시 별도 art 타입 개발 PLAN으로 묶을 수 있다. 어느 후속 작업도 자동 활성화하지 않는다.

## 현재 상황과 다음 행동

- 2026-11-25 completed. 실제 아이콘 취득·기존 게임 연결·LG 입력·갱신/자원 수명 검사 및 사용자 “현재 아이콘 복제 동작 승인” 확인을 마쳤다.
- 구현: DesktopIconProvider/DesktopIconAppearance, 독립 STA tools/DesktopIconWorker, WorkerJob, Main/PlagueState/Controller/Actor 연결. 상세 계약은 [TECH-013](../decisions/TECH-013-runtime-icon-appearance.md).
- 다음: 같은 순차 실행 승인으로 PLAN-007을 활성화한다. ART-004/006/007의 정식 적용과 아트 제작은 이번 완료 범위 밖이다.

## 검증 결과

- [최종 run-014](evidence/PLAN-012/run-014/README.md), [실제 입력 run-010](evidence/PLAN-012/run-010/results.json), [049 회귀 run-006](evidence/PLAN-012/run-006/results.json). 빌드 경고/오류0.
- 실제 viewport RGB/알파, 전체 RGBA roundtrip, 치료 후 동일 이미지, 클릭 투과/회수/드래그, 실패/늦은 세대 폐기, 제한 반복 취득 후 worker0 PASS.
- 실제 Explorer 재시작은 사용자 세션 영향 때문에 수행하지 않았으며 같은 취득 경계의 실패/지연 주입으로 복구를 검증했다. 혼합 DPI·배지/썸네일 완전 재현·장기 비용·배포는 미검증이다. 실제 LG 화면은 사용자 승인으로 확인했다.

## 관련 결정과 상태 전환

- [TECH-011](../decisions/TECH-011-plague-objects-and-reservations.md)의 내부 객체/예약 소유를 유지하고 외형 공급만 확장한다. 구현에서 확정한 OS 취득·캐시 계약은 별도 TECH로 기록한다.
- 2026-11-25 새 proposed: 사용자 방향 변경·계획 검토 요청. 구현 활성화 미승인.

- 2026-11-25 proposed → active: 사용자 “Plan을 차례대로 진행해줘”. PLAN-012부터 시작하며 후속 007/008은 선행 완료 후 같은 승인으로 순차 활성화한다.

- 2026-11-25 active → completed: 위 기술 검사와 사용자 화면 승인을 기록했다. 실제 환경에서 재현하지 않은 조건은 한계로 보존한다.
