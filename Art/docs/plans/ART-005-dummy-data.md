---
id: ART-005
type: art
status: rejected
title: 가짜 임시 파일과 쓰레기 데이터
asset_id: DummyData
revision: 1
approved_revision: 0
updated: 2026-11-25
---

# ART-005 가짜 임시 파일과 쓰레기 데이터

## 취소 결정

- 2026-11-25 사용자 실제 바탕화면 아이콘 런타임 복제 방향 채택과 불필요한 PLAN 검토 요청에 따라 proposed → rejected. 고정 파일/쓰레기 그림은 [PLAN-012](../../../docs/plans/PLAN-012-desktop-icon-clones.md)의 아이콘 취득으로 대체한다.
- 원본·제출·승인본이 없어 삭제할 에셋은 없다. 취득 실패 시 스폰을 건너뛰므로 별도 대체 그림도 만들지 않는다. 공통 변이 효과는 ART-007, 좀비 부착 요소는 ART-006이 담당한다.
- 아래 범위·경로·체크리스트는 취소 당시 계획을 보존한 것이며 실행할 작업이 아니다.

## 목표와 범위

- AssetId DummyData, props. 32×32 셀, 피벗(16,28), 종이.tmp 아이콘/작은 휴지통 쓰레기2종. 실제 Windows 파일과 다른 게임 도트 형태, 반투명 빈 영역/1px 다크 윤곽.
- 2026-11-24 사용자 PLAN-006 실행 시 “필요한 아트는 따로 Art전용 Plan을 생성해” 요청으로 작성한 제작 후보. 제작 실행/활성화 승인은 아직 없다.
- 포함: Aseprite 원본·PNG 시트·프레임 JSON/계약·배경/재생 리뷰·handoff. 제외: 게임 규칙/물리/입력/실제 파일 작업.
- 최초 예정 r001, 현재 정식 원본/승인본 없음. PLAN-006 도형 임시 표현을 정식 아트로 간주하지 않는다.

## 기준 문서와 선행 조건

- [기획 원본](../../../SCP_site-desktop.md) 2·3절, [PLAN-006](../../../docs/plans/PLAN-006-scp-049.md), 요청 PLAN-006-SPR-002.
- [Art 작업 절차](../agents/workflow.md), [리비전](../agents/revisions.md), [애니메이션](../agents/animation.md), [조명](../agents/lighting.md), [전달](../agents/handoff.md).
- 태그 후보: tmp_idle, trash_idle, claimed, dissolve. 기본 두 종류·선택된 치료 대상 표시·회수 소멸 일회. 실제 파일명이나 사용자의 파일 아이콘을 취득하지 않음. 소멸/피벗과 클릭 윤곽 알파를 명시.
- RGBA/sRGB, 고정 셀/원본1배 PNG, nearest2배 제품 표시. trim/rotation/padding/중복병합 없이 프레임별 ms·pivot·상태 접점·반전 계약을 명시한다.
- 조명 후보: Unshaded/Mix/중립 부모, 고정 명암과 알파를 유지. 049 세트의 작은 형태/색 대비가 우선이며 동적 노멀/반사 맵은 현재 범위에 포함하지 않는다. 제작 활성화 때 확정한다.
- 정지/이동 또는 등장/소멸 대표 샘플을1배/2배·밝음/어두움/무늬 배경에서 먼저 검수한 뒤 전체로 확장한다. Godot 통합 검증은 별도 개발 계획에서 담당한다.

## 결과물 저장 위치

모두 C:/Dev/scp_desktop/Art 아래 예정이며 아직 생성하지 않는다.

- assets/props/DummyData/spec.md, handoff.md.
- assets/props/DummyData/source/DummyData.aseprite.
- assets/props/DummyData/revisions/r001/source·export·review 및 README.md.
- docs/plans/evidence/ART-005/run-001/README.md·results.json·execution.log.
- 제품 전달 예정: GodotProject/Assets/Sprites/props/DummyData/의 PNG/contract만, 개발 담당.

## 작업 목록

- [ ] 실행 승인과 최종 규격/팔레트/태그/조명 방식 확정
- [ ] 대표 샘플과 실제 크기 배경/겹침/반전 검수
- [ ] 전체 동작 제작과 프레임 시간/루프/접점 계약
- [ ] 원본/출력 픽셀·알파·경계·태그/시간 검사
- [ ] 사용자 r001 시각 확인과 피드백 기록
- [ ] 제출/전달/ART 목록 완료 갱신

## 완료 기준과 검증 방법

- 원본/제출/PNG·JSON 픽셀/시간/태그가 일치하고 마지막 소멸 프레임과 반복 경계가 의도대로 동작한다.
- 실제1배/2배·세 배경·반전/겹침 재생에서 실루엣·가독성·클리핑을 검수한다.
- 현재 리비전의 사용자 시각 확인을 기록한다. 기술 검사나 계획 실행 승인을 시각 승인으로 대신하지 않는다.
- Unshaded·알파·색 불변성/제품 입력/성능은 Godot 적용 담당과 전달 계약에서 구분한다.

## 현재 상황과 다음 행동

- rejected, 제작 미실행/승인0. 런타임 아이콘 취득으로 대체하며 제작 대기열에서 제외한다.
- 후속: [아이콘 복제 기획](../../../docs/game-design/desktop-icon-clones.md)과 PLAN-012를 따른다. 취소 문서와 ID는 보존한다.

## 검증 결과

- 계획 문서만 작성, 그림/원본/재생/제품 적용/사용자 시각 검수는 미실행.

## 관련 결정과 상태 전환

- [TECH-005](../../../docs/decisions/TECH-005-separate-art-plans.md)의 제작/개발 분리.
- 2026-11-24 사용자 별도 ART 계획 생성 요청에 따라 proposed 생성. 제작 활성화는 미승인.
