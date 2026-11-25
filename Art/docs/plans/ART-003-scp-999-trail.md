---
id: ART-003
type: art
status: completed
title: SCP-999 이동 점액 자국
asset_id: Scp999Trail
revision: 1
approved_revision: 1
updated: 2026-11-24
---

# ART-003 SCP-999 이동 점액 자국

## 목표와 범위

- 목표: SCP-999 이동 뒤에 남았다가 수초 뒤 증발하는 작은 주황색 점액 자국의 원본을 제작한다.
- AssetId: Scp999Trail, 분류 vfx. 본체와 독립된 바닥 효과다.
- 포함: 점액 자국의 기본 형태 변형·증발 표현·RGBA 출력·리뷰·전달 명세.
- 제외: 발자국 충돌/클릭 영역, 실제 OS 창 변경, 무한 잔류 효과, Godot 생성/수명/부착 로직.
- 요청: 2026-11-23 PLAN-005용 상세 아트 계획 작성 후, 2026-11-24 사용자가 “Art-003도 진행해줘.”라고 제작 실행을 승인했다. r001을 최초 제작한다.

## 기준 문서와 선행 조건

- 기준: [기획 원본](../../../SCP_site-desktop.md)의 이동 점액, [PLAN-005](../../../docs/plans/PLAN-005-scp-999.md)의 요청 PLAN-005-SPR-003.
- 외형 참조: [ART-001](ART-001-scp-999-character.md)의 팔레트·알파·바닥 접점. 본체 기준 샘플의 실제 리비전을 고정한다.
- 지침: [Art MAP](../../AGENTS.md), [작업 절차](../agents/workflow.md), [계획 관리](plan.md), [디자인](../agents/design.md), [애니메이션](../agents/animation.md).
- 운영: [Aseprite](../agents/aseprite.md), [구조](../agents/project-structure.md), [리비전](../agents/revisions.md), [전달](../agents/handoff.md), [검증](../agents/verification.md).
- Art active 최대 1개 규칙과 이 ART의 명시적 실행 승인을 확인한다. 앞 ART 완료만으로 자동 착수하지 않는다.
- 점액 자국이 창 발판을 따라 움직일지·발판 소멸 시 제거할지는 개발 계약이다. 확정된 규칙을 handoff에서 참조한다.

### Art에 요구하는 모습·규격

| 항목 | 요구 |
| --- | --- |
| 형태 | 본체보다 작은 납작하고 불규칙한 주황 젤리 얼룩. 바닥에 붙은 액체로 읽히고 몸체 복제처럼 보이지 않음 |
| 셀 | 제작안: 32×16 원본 픽셀 고정 셀. 넓고 낮은 VFX 전용 규격이며 원본 1배 PNG |
| 변형 | 제작안: 넓은 얼룩·작은 방울군 2종. 같은 AssetId의 변형이며 색 변경만으로 구분하지 않음 |
| 외곽선·알파 | 1px 다크 아웃라인과 반투명 주황. 소멸 중 외곽선만 진하게 남지 않도록 함께 변화 |
| 팔레트 | 본체와 연결되는 색상, 몸체와 구분될 정도의 낮은 시각적 강조. 실제 색·알파 값은 spec에 기록 |
| 피벗 | 제작안: 중심 바닥 부착점 (16,8), 좌상단 (0,0), X 오른쪽/Y 아래. 그림의 접지 위치를 별도 명시 |
| 방향·여백 | 수평 발판용, 좌우 반전 가능 여부는 하이라이트 비대칭 확인 후 명시. 증발 입자도 셀 안에 유지 |
| 레이어 | 얼룩·작은 방울·가이드 분리. 본체·발판 그림·바탕화면 이미지는 출력하지 않음 |

### 재생·수명 계약

- 태그안 puddle_a, puddle_b는 형태 변형이며 게임 상태가 아니다.
- 각 태그는 생성 직후 형태→유지 가능한 프레임→쪼그라들며 증발하는 일회 구간을 구분한다.
- 유지 프레임과 소멸 구간을 별도 태그로 내보내거나 정확한 범위를 명시한다. 불필요한 동일 프레임을 수초치 복제하지 않는다.
- 기획의 “수초 뒤 증발”에 해당하는 총 수명·유지 시간·생성 간격·동시 개수 상한은 PLAN-005에서 확정한다.
- Art는 프레임별 ms·소멸 구간 길이·마지막 프레임 알파를 제출한다. 이미지 소멸과 Godot fade의 중복을 방지한다.
- 배경이 드러나는 RGBA 원본을 유지한다. 불투명한 검정/흰 사각형을 배경으로 합성하지 않는다.
- 조명·노멀맵·발광·광원 그림자는 필수 범위가 아니다. 바닥 접촉감은 형태·색·알파로 표현한다.
- [조명 지침](../agents/lighting.md)에 따라 본체와 점액의 시간대 색/밝기 일관성을 먼저 정한다. 기본색+엔진 색 조절 또는 조명 영향 없는 표현을 샘플 비교해 선택하고 이유를 남긴다. 노멀/반사 맵 필요 시 범위와 엔진 시험을 연결한 뒤 확장한다.
- 이동하면서 생성한 자국은 본체와 독립적으로 남아야 한다. 실제 좌표·발판 부착·소멸은 Godot 담당이다.

## 결과물 저장 위치

기준 루트 C:/Dev/scp_desktop/Art. 아래 r001 원본·출력·리뷰를 생성했다. 제품 복사만 예정이다.

| 산출물 | 경로 |
| --- | --- |
| 명세·전달 | assets/vfx/Scp999Trail/spec.md, handoff.md |
| 편집 원본 | assets/vfx/Scp999Trail/source/Scp999Trail.aseprite |
| 제출 원본 | assets/vfx/Scp999Trail/revisions/r001/source/Scp999Trail.aseprite |
| 출력 | assets/vfx/Scp999Trail/revisions/r001/export/Scp999Trail.png, Scp999Trail.json |
| 제출 기록 | assets/vfx/Scp999Trail/revisions/r001/README.md |
| 리뷰 | assets/vfx/Scp999Trail/revisions/r001/review/contact-sheet.png, trail-preview.apng, feedback.md |
| 실행 증거 | docs/plans/evidence/ART-003/run-001/README.md, results.json, execution.log |
| 제품 복사 대상 | 프로젝트 루트 GodotProject/Assets/Sprites/vfx/Scp999Trail/ — 개발 담당 |

- [명세](../../templates/asset-spec.md)·[리비전](../../templates/revision.md)·[전달](../../templates/handoff.md) 양식으로 현재값·제출값·적용값을 구분한다.
- 확정된 고정 셀 PNG와 JSON을 제공한다. 트리밍/회전/중복 병합 없이 셀 순서·태그·ms·변형 선택·접점·반전 조건을 제출한다.
- 리뷰는 본체 이동과 남은 자국·시간 경과를 모의 합성한다. 실제 Godot 발판 동작 검증과 혼동하지 않는다.
- 밝음/어두움/무늬 배경·1배/nearest 2배에서 비교하고, APNG 불가 시 실제 시간 재생 녹화로 대체한다.
- Aseprite 저장은 현재 Art 절대 경로, 제출 수정은 r002부터 증가, 기존 그림/리뷰 보존.

## 작업 목록

- [x] 실행 승인·본체 참조 리비전 확인, 크기·접점·알파·팔레트 명세
- [x] 두 형태 정지 샘플과 본체 합성으로 강조 정도·반투명 가독성 검토
- [x] 생성/유지/증발 구간 제작, 재생 계약과 변형 선택 명세
- [x] 원본 보존·PNG/JSON 출력, 배경별·연속 생성·소멸 타임라인/파일 검수 및 APNG 제공
- [x] 실제 Godot 제품 재생·전체18프레임 렌더 관찰 및 사용자 r001 시각 확인 (Aseprite GUI 자동 재생 성공은 별도 미확인)
- [x] 현재 리비전 사용자 시각 확인, handoff·ART 목록 갱신

## 완료 기준과 검증 방법

| 기준 | 방법 | 증거 |
| --- | --- | --- |
| 작은 점액 형태·반투명 가독성 | 본체와 합성, 1배/2배·세 배경 비교 | contact-sheet·trail-preview |
| 유지·증발 구간 사용 가능 | 각 변형의 실제 ms 재생, 마지막 픽셀/알파 확인 | 재생 리뷰·제출 명세 |
| 겹침 품질 | 여러 자국 겹침에서 과도하게 진한 얼룩/사각 경계가 생기는지 검토 | 합성 리뷰·피드백 |
| 전달 규격 일치 | 원본/PNG/JSON·접점·변형/태그 범위 대응 확인 | results.json·handoff |
| 사용자 시각 확인 | 현재 리비전·대상·날짜·의견 기록 | feedback.md, approved_revision=revision |

실제 수명·개수 제한·창 발판 이동/소멸·클릭 투과·숨김 시 정리는 PLAN-005의 제품 검증이다.

## 현재 상황과 다음 행동

- 2026-11-24 completed, revision=approved_revision=1. 사용자 “점액 아트도 승인할게. Plan-010을 진행해서 정식 적용까지 마무리해줘.”를 [F-004](../../assets/vfx/Scp999Trail/revisions/r001/review/feedback.md)에 기록했다.
- 원본/출력 변경 없이 기존 r001 승인. [PLAN-010 정식 적용](../../../docs/plans/PLAN-010-scp-999-trail-integration.md)에서 실제 Godot18프레임·조명/알파/태그 및 제품 입력/부착 검증을 완료했다. Aseprite GUI 자동 재생을 새로 확인했다고 주장하지 않는다.
- [명세](../../assets/vfx/Scp999Trail/spec.md), [r001 제출](../../assets/vfx/Scp999Trail/revisions/r001/README.md), [전달 계약](../../assets/vfx/Scp999Trail/handoff.md).
- 추가 제작은 없으며 후속 변경이 필요하면 새 ART/리비전으로 관리한다.

## 검증 결과

- 최종: [PLAN-010 run-001](../../../docs/plans/evidence/PLAN-010/run-001/README.md), native111개·18프레임 렌더/원본 알파/조명 불변성 검사 통과. 사용자 F-004로 시각 승인 완료. 아래 대기 문구는 제출 당시 기록이다.

- 2026-11-24: 원본/제출/PNG 픽셀 차이·윤곽·겹침·잘림·색/알파·시간 오류0. 증발 중 면적/알파 합 증가0, 두 끝 프레임 완전 투명. JSON 시간/태그/사각형 일치·원본 해시 보존 검사 통과.
- 낮/밤 unlit와 색 변조 후보·세 배경·1배/2배·겹침/반전 비교, 합성 시간별 PNG 확인. 152프레임·5000ms APNG와 Aseprite 합성 미리보기를 생성했다. 이후 Godot 적용·제품 입력/수명은 위 PLAN-005 증거에서 확인했고 최종 사용자 시각 확인은 대기 중이다.
- 2026-11-23 계획 문서 검사: 프로젝트 루트의 `node scripts/validate-agent-docs.mjs` 통과. 제작물 검증은 아니다.
- 2026-11-24 r001 제출 문서 검사: `node scripts/validate-agent-docs.mjs` 통과. 개발/ART active 각각1개, 신규 에셋·전달·증거 링크 확인. 사용자 최종 시각 승인은 아니다.

## 관련 결정과 상태 전환

- [TECH-005](../../../docs/decisions/TECH-005-separate-art-plans.md).
- 2026-11-23: 계획 작성 요청으로 proposed 생성. 활성화 승인 없음.
- 2026-11-24: “Art-003도 진행해줘.” 요청으로 proposed→active. ART-002 completed 이후 유일한 active ART로 r001 형태·증발·원본·출력·검수·전달 제작을 시작한다. 제품 스폰/발판/수명 로직 구현은 포함하지 않는다.

- 2026-11-24: F-004 r001 사용자 시각 승인 및 PLAN-010 실제 제품 검증 완료에 따라 active → completed. 원본 수정 없음.
