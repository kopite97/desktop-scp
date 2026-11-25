---
id: ART-006
type: art
status: completed
title: SCP-049-2 아이콘용 좀비 발과 보행
asset_id: Scp0492
revision: 1
approved_revision: 1
updated: 2026-11-25
---

# ART-006 SCP-049-2 아이콘용 좀비 발과 보행

## 목표와 범위

- AssetId Scp0492, characters. 실제 아이콘 아래에 붙는 보라/초록 발 두 개와 교대 보행. 손·팔·얼굴·표정·고정 몸체는 제외한다. 원래 아이콘의 식별성을 유지하고 발 동작만으로 작은 군집의 느린 보행을 표현한다.
- 2026-11-25 사용자 방향 변경에 따라 [아이콘 복제 기획](../../../docs/game-design/desktop-icon-clones.md)과 [PLAN-012](../../../docs/plans/PLAN-012-desktop-icon-clones.md)를 따른다. 아직 그림이 없으므로 proposed 범위를 수정하며 리비전은 r001을 유지한다.
- 2026-11-24 별도 아트 후보로 생성, 2026-11-25 순차 제작 승인 및 r001 시각 승인 완료.
- 포함: Aseprite 원본·PNG 시트·프레임 JSON/계약·배경/재생 리뷰·handoff. 제외: 게임 규칙/물리/입력/실제 파일 작업.
- r001 제작·시각 승인 완료. PLAN-006 도형 임시 표현은 별도이며 제품 정식 교체는 후속 개발 범위다.

## 기준 문서와 선행 조건

- [기획 원본](../../../SCP_site-desktop.md) 2·3절, [PLAN-006](../../../docs/plans/PLAN-006-scp-049.md), 요청 PLAN-006-SPR-003.
- [Art 작업 절차](../agents/workflow.md), [리비전](../agents/revisions.md), [애니메이션](../agents/animation.md), [조명](../agents/lighting.md), [전달](../agents/handoff.md).
- [아이콘 좀비 연결 계약](../../../docs/game-design/icon-zombie-presentation.md)을 부착·상태 전환의 원본으로 사용한다. 현재 제품에는 아직 없는 후속 통합 조건이다.
- 부착 기준: 좌/우 발별32×32 투명 셀, 뿌리(16,28), nearest2배. 그림은 하단의 작은 발만 담고 아이콘은 굽지 않는다. 보이는 아이콘 하단에 두 발을 정렬하며 정규화 손/얼굴 접점과 앞쪽 팔 레이어는 제거한다.
- 발별 PNG와 공통 계약을 출력한다. 아이콘 알파 경계에 따른 발 간격/최대 뻗음/좁은 형태 보정을 대표 샘플에서 수치로 고정한다. 기본 합성은 발 → 아이콘, 몸체 로고는 반전·회전·바운스하지 않는다.
- 태그: spawn, idle, move, fall, land, collect, expire. 발 등장/교대 보행/공중·착지/회수·만료의 일회 소멸을 제작한다. 논리 이동·성공·삭제는 게임이 소유하며 프레임으로 결정하지 않는다.
- collect/expire는 현재 즉시 삭제 코드에 연결할 수 없다. 후속 개발 PLAN에 표시 전용 소멸 객체·아이콘 캐시 참조·즉시 입력 해제·한 번만 회수/제거·봉쇄/종료 처리를 필수 범위로 전달한다. 임시 손 그림과 손 입력 영역 제거도 같은 통합 범위다.
- RGBA/sRGB, 고정 셀/원본1배 PNG, nearest2배 제품 표시. trim/rotation/padding/중복병합 없이 프레임별 ms·pivot·상태 접점·반전 계약을 명시한다.
- 조명 계약: Unshaded/Mix/중립 부모, 고정 명암과 알파를 유지. 049 세트의 작은 형태/색 대비가 우선이며 동적 노멀/반사 맵은 현재 범위에 포함하지 않는다. 대표 샘플에서 가독성을 확인한다.
- 정지/이동 또는 등장/소멸 대표 샘플을1배/2배·밝음/어두움/무늬 배경에서 먼저 검수한 뒤 전체로 확장한다. Godot 통합 검증은 별도 개발 계획에서 담당한다.

## 결과물 저장 위치

모두 C:/Dev/scp_desktop/Art 아래 생성했다. 제품 폴더 전달만 미실행이다.

- assets/characters/Scp0492/spec.md, handoff.md.
- assets/characters/Scp0492/source/Scp0492.aseprite.
- assets/characters/Scp0492/revisions/r001/source·export·review 및 README.md.
- docs/plans/evidence/ART-006/run-001/README.md·results.json·execution.log.
- 제품 전달 예정: GodotProject/Assets/Sprites/characters/Scp0492/의 Scp0492_left-foot.png·Scp0492_right-foot.png·Scp0492.contract.json, 개발 담당.

## 작업 목록

- [x] 실행 승인과 샘플 세부 규격/팔레트/태그 확정, Unshaded 계약 확인
- [x] 원형·사각·비대칭·투명 구멍/큰 여백·좁은 테스트 아이콘의 발 접점/간격 확정과 8개 겹침/방향 검수
- [x] 전체 발 동작과 프레임 시간/루프/접점·삭제 후 표시 수명·취소 계약 및 추가 개발 항목 전달
- [x] 원본/출력 픽셀·알파·경계·태그/시간 검사
- [x] 사용자 r001 시각 확인과 피드백 기록
- [x] 제출/전달/ART 목록 완료 갱신

## 완료 기준과 검증 방법

- 원본/제출/PNG·JSON 픽셀/시간/태그가 일치하고 마지막 소멸 프레임과 반복 경계가 의도대로 동작한다.
- 실제1배/2배·세 배경·반전/겹침 재생에서 실루엣·가독성·클리핑을 검수한다.
- 여러 형태의 테스트 아이콘 몸체가 가려지거나 잘리지 않고 발 부착점/입력 윤곽이 전달 계약과 일치한다. 사용자 실제 아이콘을 Art 원본에 굽지 않는다.
- 현재 리비전의 사용자 시각 확인을 기록한다. 기술 검사나 계획 실행 승인을 시각 승인으로 대신하지 않는다.
- Unshaded·알파·색 불변성/제품 입력/성능은 Godot 적용 담당과 전달 계약에서 구분한다.

## 현재 상황과 다음 행동

- completed, r001 승인 완료. 다음은 순차 승인된 ART-007 제작이다.
- [제출본](../../assets/characters/Scp0492/revisions/r001/README.md), [개발 전달](../../assets/characters/Scp0492/handoff.md). 정식 적용은 별도 개발 PLAN에서 발 합성/입력·소멸 수명을 구현한다.

## 검증 결과

- 2026-11-25 [run-001](evidence/ART-006/run-001/README.md): 기술34개 PASS, 실제 Aseprite30프레임 재생 확인, 사용자 “ART-006 r001 시각 승인”. 제품 적용은 미실행.

- 2026-11-25 현재64×64 창·최대48px 아이콘·즉시 삭제 구현과 대조해 계획을 수정했다. 그림/재생/제품 적용/사용자 시각 검수는 미실행.
- 계획 수정 후 문서 구조·메타데이터·목록·로컬 링크 검사 PASS. 이 결과는 아트 제작/제품 기능 검증이 아니다.

## 관련 결정과 상태 전환

- [TECH-005](../../../docs/decisions/TECH-005-separate-art-plans.md)의 제작/개발 분리.
- 2026-11-24 사용자 별도 ART 계획 생성 요청에 따라 proposed 생성. 제작 활성화는 미승인.
- 2026-11-25 proposed 유지: 사용자 방향 변경에 따라 고정 좀비 파일 몸체를 아이콘용 공통 부착 요소로 변경. 제작은 미실행이다.

- 2026-11-25 proposed 유지: 사용자 “손은 제외하고 그냥 발만 나와서 걸어다니는” 방향 및 추가 애니메이션 개발 지침 요청을 반영했다. 계획 수정이며 제작·제품 구현을 시작한 것은 아니다.

- 2026-11-25 proposed → active: 사용자 “Art작업을 승인할게 차례대로 진행해줘. Art작업 하나가 끝나면 시각적으로 확인을 차례대로 할게”에 따라 ART-006 → 사용자 시각 확인 → ART-007 → 사용자 시각 확인 순서로 실행한다. 제품 정식 적용은 이번 제작 승인과 구분한다.
- 2026-11-25 active → completed: r001 산출물·기술/재생 검사와 사용자 “ART-006 r001 시각 승인”을 충족했다. revision=approved_revision=1.
