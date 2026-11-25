---
id: PLAN-014
type: art
status: completed
title: 승인된 좀비 발과 치료·변이 효과 정식 적용
updated: 2026-11-25
---

# PLAN-014 승인된 좀비 발과 치료·변이 효과 정식 적용

## 목표와 범위

- 실제 바탕화면 아이콘 복제본에 승인된 ART-006 r001의 두 발을 붙이고, ART-007 r001의 치료·변이 효과를 실제 게임 상태에 연결한다. 임시 손·발·변이 원호를 정식 표현으로 교체한다.
- 포함: 승인 파일 수령, 아이콘 경계/부착/알파 입력, 7종 발 동작, 치료·변이 효과 창, 회수/만료 뒤 표시 전용 소멸 수명, 캐시 참조·봉쇄·종료 정리, 실제 LG 검증과 사용자 제품 화면 확인.
- 제외: 승인 아트 재제작·리비전 변경, SCP-173 아트, 실제 파일 생성/변경/삭제, 치료량·좀비 수명·보상 등 게임 규칙 변경, 동적 조명 신규 도입, 배포용 내보내기·고부하/장기 성능 측정.
- 요청: 2026-11-25 사용자 “ART-006·007 정식 적용을 하고 싶어. 기존 PLAN을 사용해서 할 수 있어? On hold거나 active중인 plan이 있는지 확인하고 그렇지 않다면 새 plan을 작성해줘.”에 따라 조사·계획 작성한다. 계획 작성 뒤 사용자 실행 승인으로 구현을 진행했다.

## 기준 문서와 선행 조건

- 작성 직전 개발 PLAN13개: completed12, rejected1, active0, on hold0. ART7개: completed6, rejected1, active0, on hold0. 목록뿐 아니라 각 문서의 메타데이터와 문서 검사기로 확인했다.
- [PLAN-006](PLAN-006-scp-049.md)은 임시 표현 기반 게임 기능, [PLAN-012](PLAN-012-desktop-icon-clones.md)는 실제 아이콘 복제, [PLAN-013](PLAN-013-scp-049-art-integration.md)은 ART-004 역병의사 본체 적용을 완료했다. 새 부착물/효과/소멸 수명은 그 완료 범위 밖이다.
- [PLAN 관리](plan.md)의 “완료된 development PLAN을 남은 아트 때문에 재활성화하지 않는다”, “completed·rejected 작업의 새 기능은 새 PLAN으로 작성한다”에 따라 기존 완료·취소 기록을 보존하고 후속 통합을 별도 관리한다.
- [ART-006](../../Art/docs/plans/ART-006-scp-049-2-zombie.md)·[ART-007](../../Art/docs/plans/ART-007-scp-049-mutation.md)은 각각 r001 제작·사용자 시각 승인 완료. 아트 원본 승인과 본 PLAN의 실제 제품 화면 승인을 구분한다.
- 기준: [아이콘 복제](../game-design/desktop-icon-clones.md), [발·효과 연결 계약](../game-design/icon-zombie-presentation.md), [로드맵](../game-design/roadmap.md).
- 필수 전달: [발 명세](../../Art/assets/characters/Scp0492/spec.md)·[handoff](../../Art/assets/characters/Scp0492/handoff.md), [효과 명세](../../Art/assets/vfx/Scp049Mutation/spec.md)·[handoff](../../Art/assets/vfx/Scp049Mutation/handoff.md).
- 지침: [아키텍처](../agents/architecture.md), [구조](../agents/project-structure.md), [Windows 연동](../agents/windows-integration.md), [실행 하네스](../agents/harness.md), [Godot 도구](../agents/godot-tools.md), [검증](../agents/verification.md), [Art 조명 계약](../../Art/docs/agents/lighting.md).
- [PLAN-011](PLAN-011-overlay-stacking.md)의 창 순서/겹침 수정과 [PLAN-008](PLAN-008-system-options.md)의 봉쇄/복귀/종료를 유지한다. 실행 시 LG hardware ID·작업 영역·DPI를 재확인하며 OS 주 모니터 설정을 변경하지 않는다.

## 수령 규격과 구현 계약

| 승인본 | 입력 | 제품 표시·시간 |
| --- | --- | --- |
| Scp0492 r001 | 좌/우 PNG 각각192×160, 6열×5행, 32×32셀, 30프레임/7태그, 공통 JSON | nearest2배, 뿌리(16,28). spawn500ms, idle1200ms 반복, move840ms 반복, fall320ms 반복, land340ms, collect360ms, expire660ms |
| Scp049Mutation r001 | PNG192×192, 4열×4행, 48×48셀, 16프레임/2태그, JSON | nearest2배/96×96 장식 창, 피벗(24,40). treat960ms 반복, transform800ms 일회(완전 투명 마지막100ms 포함) |

- 실제 프레임별 시간과 알파는 승인 JSON을 원본으로 삼는다. 균일 FPS로 치환하지 않으며 index/index0를 명시적으로 변환한다. 제품 PNG/JSON과 승인 export의 해시를 대조한다.
- Unshaded/Mix/중립 부모, sRGB·straight alpha·nearest를 유지한다. 발/효과의 고정 명암·구운 알파에 전체 fade를 다시 곱하지 않는다. 노멀/반사 맵은 제출 범위에 없다.

### 발 부착과 입력

- 아이콘의 alpha>0 가시 경계와 하단 지지 열을 한 번 계산해 캐시한다. 좀비의 가시 중심을x32의 가장 가까운 정수 위치, 가시 아래끝 다음 좌표를y56에 맞추고 그림을 정수 이동만 한다. 이미지/색/종횡비/로고를 보존한다.
- 발 뿌리 y56, 선호 반간격 `clamp(round(visibleWidth*.18),3,9)` 화면px. y52..55의 지지 열 중 목표에 가장 가까운 열을 택하며 동률은 중앙 우선. 가능하면 두 뿌리의 간격2px 이상, 지지 열 하나는 공유, 완전 투명 아이콘은 발도 생략한다. 상세는 승인 계약을 따른다.
- 뒤발→앞발→아이콘 순서로64×64 창에 합성한다. 발만 뿌리축x16에서 반전(`x'=32-x`)하고 방향 변경으로 클립 시간을 초기화하지 않는다. 몸체 반전·회전·바운스는 추가하지 않는다.
- alpha>=16인 실제 아이콘/발 픽셀의 합집합으로 입력을 판정한다. 임시 손 렌더링·손 입력, 발 셀 전체·발 사이·투명 구멍의 불필요한 입력을 제거한다. 렌더 위치와 마스크 위치는 같은 계산 결과를 사용한다.
- 일반 복제본과 SCP-173 파손 표현의 정렬/입력은 기존 계약을 유지한다. 이미지 취득 실패 시 기존 대체 경로도 조사해 손 사각형이나 잘못된 클릭 영역이 다시 나타나지 않게 한다.

### 사건·표시 수명과 캐시

- PlagueState가 치료/성공/회수/만료와 물리를 계속 소유한다. 표시 동작의 시계는 같은 업데이트 경로에서 한 번만 진행하고, Publish/창 순서 갱신은 시간을 진행하지 않는다.
- 실제 Treating 진입에 treat0초를 시작한다. 예약만으로 표시하지 않는다. 일반3초·과부하 약2초 치료 중에도 효과의120ms 프레임 속도는 일정하다. 게임 성공 사건이 발 spawn과 transform을 같은0초에서 시작한다.
- spawn/land는 공중·드래그·회수에 양보한다. 드래그는 fall 자세, 실제 착지 때만 land. 등장 중에도 이동을 멈추지 않는다. 성공/제거 사유를 명시적으로 전달해 창이 사라졌다는 이유만으로 소멸 종류를 추정하지 않는다.
- collect/expire는 논리 제거·예약/입력/생존 수·보상을 즉시 한 번만 처리한 후 비상호작용 표시 객체로360/660ms 재생한다. 정상 데이터 회수·파손·앱 종료를 좀비 회수/만료로 잘못 처리하지 않는다.
- 소멸 객체는 마지막 아이콘 이미지·위치·방향·발 자세를 확보한다. 표시만 남은 객체는 게임 대상에 재등록하지 않는다. 아이콘 bodyAlpha는 JSON 곡선대로 적용하고 발 알파는 PNG 그대로 사용한다.
- 적용 전 PlagueController.Publish는 제거된 개체의 창을 즉시 해제하고 캐시는 생존 개체만 유지했다. 적용 후에는 생존 객체와 표시 전용 소멸의 외형 ID 합집합을 Retain에 전달한다. 카탈로그 갱신으로 원본 아이콘이 빠져도 소멸이 끝날 때까지 텍스처가 살아 있어야 한다.
- 최대 표시 수명·중복 사건·창 등록/해제를 한 소유자에서 처리한다. 봉쇄는 표시를 숨기고 나이를 동결, 복귀는 남은 시간부터 재생, 종료는 즉시 전부 해제한다.

### 효과 창과 공통 순서

- 효과 창은96×96, 위치는 대상 foot에서(48,80)을 뺀 물리 좌표다. 이동·발판·드래그를 따라가고 아이콘 창64px에 잘라 넣지 않는다. 대상 방향으로 효과를 반전하지 않는다.
- 중앙24×24 원본 영역의 완전 투명을 보존한다. 작업 영역 밖 효과 픽셀만 마스크하고 아이콘/입력 위치를 이동시키지 않는다. 모니터 변경은 기존 좌표 경계를 재사용한다.
- 치료 취소·대상 회수/삭제/만료는 관련 효과를 즉시 제거한다. 재치료는 treat0초로 시작한다. 봉쇄·복귀·앱 종료에서도 무효 대상 효과가 살아나지 않는다.
- 효과 창과 소멸 창 전체는 외부 프로세스까지 클릭 투과시킨다. Main 소유 OverlayStackCoordinator에 등록하며 독립 반복 최상단 요청을 만들지 않는다. 본체/효과의 상대 순서와 겹침 드래그 우선순위를 함께 검증한다.

## 결과물 저장 위치

기준 루트 `C:/Dev/scp_desktop`. 아래 제품 파일·진단과 실행 증거를 생성·연결했다. 승인 Art 원본/리뷰는 제품에 복사하지 않는다.

| 산출물 | 경로 | 상태 |
| --- | --- | --- |
| 발 승인 입력 | `Art/assets/characters/Scp0492/revisions/r001/export/` | 기존, 변경하지 않음 |
| 효과 승인 입력 | `Art/assets/vfx/Scp049Mutation/revisions/r001/export/` | 기존, 변경하지 않음 |
| 제품 발 | `GodotProject/Assets/Sprites/characters/Scp0492/Scp0492_left-foot.png`, `Scp0492_right-foot.png`, `Scp0492.contract.json` | r001 복사·런타임 로드 완료 |
| 제품 효과 | `GodotProject/Assets/Sprites/vfx/Scp049Mutation/Scp049Mutation.png`, `Scp049Mutation.contract.json` | r001 복사·런타임 로드 완료 |
| 발 표현·시계 | `GodotProject/Scripts/Overlay/ZombieSprite.cs`, `GodotProject/Scripts/Simulation/ZombieAnimation.cs` | 생성 완료 |
| 장식 창·소멸 표현 | `GodotProject/Scripts/Overlay/PlagueEffectWindow.cs`, `PlaguePresentation.cs` | 생성 완료, 컨트롤러 소유 표시 수명 |
| 연결·캐시·게임 사건 | `GodotProject/Scripts/Overlay/PlagueActorWindow.cs`, `PlagueController.cs`, `DesktopIconAppearance.cs`, `GodotProject/Scripts/Simulation/PlagueState.cs` | 기존 파일 변경 완료 |
| 공통 순서·시작/종료 | `GodotProject/Scripts/Overlay/OverlayStackCoordinator.cs`, `GodotProject/Scripts/App/Main.cs` | 기존 인터페이스 재사용 |
| 진단 | `GodotProject/Scripts/Diagnostics/ZombieArtSmoke.cs`, `GodotProject/Scenes/Diagnostics/ZombieArtSmoke.tscn`, `scripts/godot.ps1` | 신규 진단·실행 경로 연결 완료 |
| 증거 | [실행 기록](evidence/PLAN-014/README.md), 각 run의 `results.json`, 로그, `asset-hashes.json`, 테스트 소유 검수 PNG | 실행 시 생성, run 번호 증가·이전 회차 보존 |
| 전달·진행 기록 | 본 PLAN/목록, 발·효과 handoff, `docs/game-design/roadmap.md` | 제품 적용·사용자 승인 및 완료 상태 갱신 |

## 작업 목록

- [x] active/on hold 조사·기존 범위 검토·승인 아트/전달 계약 확인·proposed 계획 작성
- [x] 실행 승인·선행 조건 재확인 후 active 전환, 승인 파일 해시와 제품 수령 기록
- [x] 발 리소스·부착·양방향 알파 입력·7태그 상태 연결, 임시 손/발 제거
- [x] 제거 사유·표시 전용 소멸·아이콘 캐시 참조·중복 회수 방지 구현
- [x] 96×96 치료/변이 효과 창·동일 성공 사건·공통 순서·작업 영역 마스크 연결
- [x] 봉쇄/복귀/종료와 모니터 환경 변경 시 전체 표시·입력·리소스 정리
- [x] 로직·렌더·LG 실제 입력·회귀·짧은 비용 검증 및 증거 기록
- [x] LG 실제 제품 화면 사용자 확인, 수령/적용 리비전과 handoff/PLAN/로드맵 마감

## 완료 기준과 검증 방법

| 기준 | 검증 방법 | 남길 증거 |
| --- | --- | --- |
| 승인본 그대로 정식 적용 | export/제품 해시, 태그·셀·개별 ms·피벗·nearest·실제 viewport 색/알파 비교 | 파일 해시·렌더 검사 |
| 발 정렬·입력 일치 | 여섯 대표 형태, 폭1/2·완전 투명·비대칭 하단, 양방향/30프레임,64px 경계, 구멍/손 자리/발 사이 투과 | 소유 fixture 렌더·입력 로그 |
| 상태와 시간 연결 | 등장 중 이동, 공중/드래그/착지, 방향 변경 위상, 일반/과부하 치료·취소/즉시 재시작, 성공 시 spawn+transform 동시 시작 | 공통 제품 상태 경로 검사 |
| 논리 삭제와 표시 분리 | 회수/만료 즉시 카운트·예약·입력 제외, 중복 클릭·새 스폰·카탈로그 갱신,360/660ms 종료·캐시/창 해제 | 상태·참조/창 수·외부 수신 로그 |
| 효과 위치·클릭 투과 |96×96/800ms, 대상 이동·발판·드래그·네 모서리, 효과/소멸 창의 별도 프로세스 수신기 통과 | 실제 Windows 로그·필요 화면 |
| 기존 겹침/봉쇄 유지 | LG에서999·049·아이콘·173 및8개 아이콘 겹침, 드래그 깜빡임/순서, 봉쇄 중 입력 해제·복귀 시계·종료/재실행 | 실제 입력·회귀 결과 |
| 조명·비용 | Unshaded의 낮/밤·광원/부모 중립 색 불변, 같은 개체 수/조건의 효과0/표시 중·정리 후 짧은 비용·자원 비교 | 측정 조건/실측값, 미측정 한계 |
| 제품 최종 확인 | LG에서 크기·색감·보행·치료/변이·회수/만료 동작 사용자 확인, 문서 검사 | 사용자 의견·대상 리비전·검사 결과 |

- 헤드리스/모의 시나리오는 실제 Windows 입력 검사를 대신하지 않는다. 자동 입력은 실행 시 사용자 작업과 충돌하지 않는 시점을 확인하고 테스트 소유 창에서 진행한다.
- 실제 아이콘 취득은 기존 읽기 전용 경로를 사용한다. 저장소에 남길 픽셀 비교·화면 증거는 테스트 소유 도형을 우선하고 사용자 아이콘/문서 내용은 아트 에셋으로 저장하지 않는다.
- 049 본체(PLAN-013), 아이콘 복제/파손(PLAN-012/007), 창 순서(PLAN-011), 봉쇄/종료(PLAN-008)의 영향받은 검사를 수행한다. 혼합 DPI·탈착·장기 성능 등 실제 미실행 항목은 별도 한계로 기록한다.

## 현재 상황과 다음 행동

- completed. r001 발/효과·소멸 통합, 보행·깜빡임 수정, 기술 검증과 LG 사용자 제품 시각 승인 완료. 최초 확인에서는 역보행 느낌을, 다음 확인에서는 복제본의 지속 깜빡임을 지적받아 각각 수정했다.
- move만16→9 역순 재생하도록 수정했다. 각 프레임 시간·840ms 주기·원본 픽셀·좌우 위상·방향 반전을 유지하고, 접지 발의 이동 방향을 실제 시트 픽셀로 검사했다. run-018/019 재검증 PASS.
- run-020에서 사용자가 마우스와 무관한 복제본 깜빡임을 보고했다. 빈 MousePassthroughPolygon 반복 대입이 Win32 배경 지우기를 유발함을 재현했다. 정지 Publish40회에서 배경 지우기40회(run-021)→반복 설정 제거 후0회(run-022), 렌더/마스크 회귀 PASS.
- run-023 외부 입력 재검증 PASS. run-024 LG 제품에서 사용자 “깜빡임·보행 수정 및 정식 적용 승인”을 확인했다. 필수 잔여 작업 없음. 승인된 실행 화면은 유지한다. 원본 ART-006/007 r001과 기존 제작 승인 기록은 보존한다.

## 구현 결정

- [TECH-015](../decisions/TECH-015-zombie-presentation-lifetime.md): 게임 제거 사건과 표시 전용 소멸·캐시 참조·게임 시계 책임을 분리했다.
- 좀비 창은 발바닥이 물리 foot에 닿도록 `foot-(32,64)`, 일반 데이터는 기존 `foot-(32,56)`을 유지한다. transform은 `foot-(48,80)`, treat는 일반 몸체 정렬에 맞춘 표시 기준점 `foot+(0,8)`에서 같은 피벗을 뺀다.
- 보행 순서 수정은 사용자 제품 피드백에 따른 런타임 매핑이다. 승인 export를 덮어쓰거나 새 아트 리비전으로 간주하지 않는다.

## 검증 결과

| 날짜 | 환경·대상·방법 | 실제 결과·판정 | 미검증 |
| --- | --- | --- | --- |
| 2026-11-25 | PLAN/ART 메타데이터·목록, 기존 코드/전달 문서 읽기 | active0/on hold0, 후속 통합 필요 확인 | 구현·제품 검증 미실행 |
| 2026-11-25 | `node scripts/validate-agent-docs.mjs` | 계획 작성·수정 후 구조/메타데이터/목록/링크 PASS. 완료 상태에서도 다시 검사 | 아트 품질·게임 기능 검사가 아님 |
| 2026-11-25 | LG run-024 사용자 제품 확인 | “깜빡임·보행 수정 및 정식 적용 승인” | GPU/장기 성능 등 아래 명시 범위 |

- 상세 명령·실패와 수정·검사별 범위는 [실행 기록](evidence/PLAN-014/README.md)에 보존했다. Build0오류/0경고, 로직·LG 렌더·실제 입력·회귀 PASS. run-016의 SDK 환경 누락은 기존 실행 스크립트로 복구했다.
- run-005 새 아트 실제 입력 PASS713, run-015 실제 겹침 입력 PASS197. 보행 수정 뒤 run-018/019의 접지 방향·렌더/마스크 검사를 통과했다. GPU·장기 비용·혼합 DPI·실제 모니터 탈착은 미검증이다.

## 관련 결정과 상태 전환

- [TECH-011](../decisions/TECH-011-plague-objects-and-reservations.md) 게임 상태, [TECH-012](../decisions/TECH-012-overlay-stacking.md) 창 순서, [TECH-013](../decisions/TECH-013-runtime-icon-appearance.md) 아이콘 취득/캐시, [TECH-014](../decisions/TECH-014-system-options-and-lockdown.md) 봉쇄 책임을 재사용한다. 실행 중 여러 기능에 영향을 주는 새로운 선택은 별도 TECH로 기록한다.
- 2026-11-25 proposed 생성: 사용자 요청에 따른 상태 조사와 후속 PLAN 작성. 완료된 PLAN-006/012/013 및 ART-006/007의 기존 상태/증거를 유지한다.
- 2026-11-25 proposed → active: 사용자 “PLAN-014를 진행해줘.”를 본 계획의 실행 승인으로 기록한다. 승인 r001 정식 적용·기술 검증·LG 제품 화면 확인 범위를 수행한다.

- 2026-11-25 active → completed: r001 정식 연결·보행 재생 순서·반복 창 영역 재설정 수정, 실제 LG 외부 입력/렌더/상태 회귀와 사용자 “깜빡임·보행 수정 및 정식 적용 승인” 충족. 문서·handoff·로드맵을 완료 상태로 갱신한다.
