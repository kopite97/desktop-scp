---
id: ART-002
type: art
status: active
title: SCP-999 반응 효과와 본체 외곽선 조명 분리
asset_id: Scp999Reaction
revision: 1
approved_revision: 0
updated: 2026-11-24
---

# ART-002 SCP-999 반응 효과와 본체 외곽선 조명 분리

## 목표와 범위

- 목표: 타이핑에 신나 하는 SCP-999 위에 표시할 작은 하트·음표 효과를 제작한다.
- AssetId: Scp999Reaction, 분류 vfx. 하나의 반응 효과 세트 안에서 heart와 note 변형을 태그로 구분한다.
- 포함: 하트·음표 각각의 등장/강조/소멸 이미지, 편집 원본·출력·리뷰·전달 명세.
- 제외: 문자/입력 내용 표현, 사운드, 스트레스 UI·오라, 광원·발광 셰이더·실제 이동/스폰 로직.
- 요청: 2026-11-23 계획 작성 후 2026-11-24 사용자가 ART-002 실행을 명시적으로 승인했다. 본체 r004를 참조하는 r001을 제작한다.

## 추가 범위: 본체 외곽선 조명 분리

- 2026-11-24 사용자가 지침 보완·PLAN 반영 및 완료된 ART-001의 후속 작업을 ART-002에 포함해 실행하도록 명시했다. ART-001 completed/r004/approved_revision=4는 유지한다.
- 주 에셋 메타데이터는 Scp999Reaction r001/승인0이다. 아래 추가 에셋의 승인·완료는 별도로 판단한다.

| AssetId | 작업 리비전 | 최근 사용자 확인 | 이번 범위 |
| --- | --- | --- | --- |
| Scp999Reaction | r001 | 없음 | 전체 기호 Unshaded, 원본 알파와 단일 fade, 실제 Godot 검증 |
| Scp999 | r005 제출 | r004 | lit 몸체/표정과 unlit 외곽선 분리, r004와 비교, 50프레임 동기화 |

- 본체는 4장(원본 1/7/9/12)으로 분리·재합성·Godot 관문을 확인한 뒤 전체 50장으로 확장한다. 원래 형태·색·태그·시간·피벗·연결점과 r004 노멀/반사 데이터는 보존한다.
- 외곽선은 기본색에서 제거하고 Scp999_outline.png로 출력한다. 표정은 기존 lit 경로를 유지한다. 별도 원본 Scp999-outline-unlit.aseprite와 r005 제출본을 만든다.
- 효과 PNG는 변경하지 않고 Godot 재질/상속을 설정한다. 엔진 코드는 active PLAN-005 단계 A의 외곽선/VFX 시험으로 관리한다.
- 검수: 조명/CanvasModulate 변경에도 외곽선·효과 RGB/alpha 유지, 몸체 조명 반응 유지, 재합성 픽셀 일치·프레임/반전·이음새·1배/2배·소멸·추가 비용 확인.
- ART-002 완료에는 효과 r001과 본체 후속 리비전 각각의 최종 사용자 확인이 필요하다. 본체 r004 승인을 r005에 자동 적용하지 않는다.

## 기준 문서와 선행 조건

- 기준: [기획 원본](../../../SCP_site-desktop.md)의 타이핑 반응, [PLAN-005](../../../docs/plans/PLAN-005-scp-999.md).
- 요청 ID: PLAN-005-SPR-002. 본체는 [ART-001](ART-001-scp-999-character.md).
- 지침: [Art MAP](../../AGENTS.md), [작업 절차](../agents/workflow.md), [계획 관리](plan.md), [디자인](../agents/design.md), [애니메이션](../agents/animation.md).
- 운영: [Aseprite](../agents/aseprite.md), [구조](../agents/project-structure.md), [리비전](../agents/revisions.md), [전달](../agents/handoff.md), [검증](../agents/verification.md).
- ART-001의 채택된 기준 샘플·팔레트·효과 연결점에 맞춘다. 본체 전체 제작 완료는 필수가 아니지만 사용한 참조 리비전은 기록한다.
- Art active는 최대 1개다. ART-001 종료/보류 등 상태 정리와 이 ART에 대한 별도 실행 승인 후 착수한다.

### Art에 요구하는 모습·규격

| 항목 | 요구 |
| --- | --- |
| 모양 | 하트와 음표를 색뿐 아니라 실루엣으로 구분. SCP-999 얼굴과 겹쳐도 표정을 가리지 않을 크기 |
| 셀 | 제작안: 32×32 원본 픽셀 고정 셀, 1배 PNG. 캐릭터 64×64 규칙의 예외가 아닌 별도 VFX 규격안 |
| 그림 영역 | 셀보다 작은 도형으로 시작해 등장/강조 변형 여백 확보. 두 변형의 시각적 무게를 맞춤 |
| 외곽선·알파 | 원본 1px 다크 아웃라인, 투명 배경 RGBA. 소멸 중에도 계단형 윤곽과 의도한 알파 유지 |
| 색 | 하트/음표가 주황 몸체와 구분되되 같은 스타일 유지. 정확한 색 수·알파는 샘플 후 spec에 기록 |
| 기준점 | 제작안: 셀 중심 (16,16), 원점 좌상단/X 오른쪽/Y 아래. 본체 연결점으로부터 오프셋은 handoff에 기록 |
| 방향 | 화면에 읽히는 정방향 1종. 본체 방향 반전 시 음표까지 무조건 뒤집지 않음 |
| 레이어 | heart·note 변형과 가이드 분리. 본체 픽셀·그림자·배경은 포함하지 않음 |

### 동작·합성 계약

| 태그안 | 형태 변화 | 재생 계약 |
| --- | --- | --- |
| heart | 작게 나타남→하트가 또렷해짐→작아지거나 사라짐 | 일회. 마지막 프레임의 투명도·종료 시 제거 조건 명시 |
| note | 작은 음표 등장→리듬감 있게 강조→소멸 | 일회. 글자 텍스트가 아닌 도트 도형 |

- 프레임 수·프레임별 ms·강조 프레임·끝 상태는 각 태그별로 정한다. 일회 효과를 무한 반복하도록 제출하지 않는다.
- 제작안: 셀 내부에는 형태 변화만 담고 떠오르는 궤적·분산·위치·총 표시 수명은 Godot에서 제어한다.
- 이미지 알파로 소멸시키는 구간과 Godot 추가 fade를 명세해 의도치 않은 이중 감쇠를 피한다.
- 반복 타이핑 때의 생성 빈도·최대 동시 개수·이벤트 병합은 PLAN-005가 정한다. Art는 세트 반복 예시를 리뷰로 제공한다.
- 본체와 독립된 투명 효과이며 클릭/드래그를 가로채지 않아야 한다. 효과 영역이 기존 128×128 본체 창 밖으로 나갈 수 있음을 개발에 전달한다.
- 노멀맵·발광 마스크·블룸은 필수 출력에 포함하지 않는다. 색만으로 반응을 읽을 수 있어야 한다.
- [조명 지침](../agents/lighting.md)의 조명 영향 없는 VFX 방식이 제작안이다. 하트/음표의 식별성을 이유로 명세하며, 낮/밤 합성 가독성을 확인한다. 모든 VFX에 보조 맵을 강제하지 않는다.

## 결과물 저장 위치

기준 루트 C:/Dev/scp_desktop/Art. 아래 r001 원본·출력·리뷰를 생성했다. 제품 복사만 예정이다.

| 산출물 | 예정 경로 |
| --- | --- |
| 명세·전달 | assets/vfx/Scp999Reaction/spec.md, handoff.md |
| 편집 원본 | assets/vfx/Scp999Reaction/source/Scp999Reaction.aseprite |
| 제출 원본 | assets/vfx/Scp999Reaction/revisions/r001/source/Scp999Reaction.aseprite |
| 출력 | assets/vfx/Scp999Reaction/revisions/r001/export/Scp999Reaction.png, Scp999Reaction.json |
| 제출 기록 | assets/vfx/Scp999Reaction/revisions/r001/README.md |
| 리뷰 | assets/vfx/Scp999Reaction/revisions/r001/review/contact-sheet.png, composite-preview.apng, feedback.md |
| 실행 증거 | docs/plans/evidence/ART-002/run-001/README.md, results.json, execution.log |
| 제품 복사 대상 | 프로젝트 루트 GodotProject/Assets/Sprites/vfx/Scp999Reaction/ — 개발 담당 |

- [명세](../../templates/asset-spec.md)·[리비전](../../templates/revision.md)·[전달](../../templates/handoff.md) 양식에 참조 본체 리비전과 실제 경로를 기록한다.
- PNG는 확정된 고정 셀, 트리밍·회전·중복 병합 없음. 시트 행/열·간격/여백·각 태그 프레임 범위·ms·피벗을 제공한다.
- JSON이 담지 않는 합성 순서·오프셋·알파 규약은 제출 README로 보완한다. 인덱스 변환과 Godot 태그 매핑은 개발에서 검증한다.
- 본체 합성 리뷰는 실제 제출 리비전 또는 명시한 임시 샘플을 사용하고 밝음/어두움/무늬 배경·1배/2배를 비교한다.
- APNG 출력 불가 시 실제 시간 재생 녹화와 환경으로 대체한다. 리뷰는 제품 PNG와 구분한다.
- Aseprite는 현재 Art 절대 경로를 사용한다. 그림 수정 제출은 r002부터, 이전 제출은 보존한다.

## 작업 목록

- [x] 실행 승인·본체 참조 리비전 확인, 셀·팔레트·피벗·합성 오프셋 명세
- [x] 하트/음표 정지 샘플과 본체 2배 합성으로 크기·가독성 검토
- [x] 두 태그의 등장/강조/소멸 제작, 프레임별 ms·합성 계약 기록
- [x] 원본 보존·PNG/JSON 출력, 반복 생성 예시 재생·배경별 리뷰
- [x] 본체 r004 보존, 외곽선 분리 4장과 Godot 조명 제외 관문
- [x] 본체 r005 전체 50프레임 분리·동기화·원본 대응 및 엔진 검사
- [x] 효과 전체 Unshaded/부모 색 변조/소멸 알파의 실제 Godot 검사, PLAN-005 증거 연결
- [ ] 최종 사용자 시각 확인과 전달 기록, 목록 메타데이터 갱신

## 완료 기준과 검증 방법

| 기준 | 방법 | 증거 |
| --- | --- | --- |
| 하트·음표 식별·본체 표정 가독성 | 실루엣 및 1배/2배 합성, 밝음/어두움/무늬 배경 비교 | contact-sheet·composite-preview |
| 시작/끝·반복 생성 품질 | 실제 ms 재생과 연속 생성 예시 관찰 | 재생 리뷰·검사 기록 |
| 전달 규격 일치 | 셀 크기·RGBA·태그·시간·오프셋·원본 대응 확인 | 제출 README·results.json |
| 사용자 시각 확인 | 현재 리비전·날짜·대상 파일·의견 기록 | feedback.md, approved_revision=revision |

실제 클릭 투과·스폰 제한·창 경계 처리는 PLAN-005에서 검증한다. ART 완료는 제품 적용 완료를 뜻하지 않는다.

## 현재 상황과 다음 행동

- 현재 active, 주 에셋 Scp999Reaction revision=1/approved_revision=0. 추가 본체 Scp999 r005 제출/최근 승인r004. 파일·외곽선/VFX 조명 제외·실제 시간 재생·추가 비용 검증을 마쳤다. 효과 r001과 본체 r005의 최종 사용자 시각 확인은 각각 대기한다.
- 다음: 효과 r001 및 본체 r005의 시각 피드백을 각각 기록한다. 정식 제품 통합은 PLAN-005 후속, ART-003은 proposed 유지.
- 실제 자료: [명세](../../assets/vfx/Scp999Reaction/spec.md), [r001 제출](../../assets/vfx/Scp999Reaction/revisions/r001/README.md), [handoff](../../assets/vfx/Scp999Reaction/handoff.md), [검수 증거](evidence/ART-002/run-001/README.md).
- 본체 후속: [r005 제출](../../assets/characters/Scp999/revisions/r005/README.md), [4장 관문](evidence/ART-002/run-002/README.md), [50프레임 파일 검사](evidence/ART-002/run-003/README.md), [엔진 행렬](../../../docs/plans/evidence/PLAN-005/run-012/README.md).

## 검증 결과

- 2026-11-24: 32×32 셀, 8열×2행 시트, heart/note 각 8프레임·850ms 일회 재생. 마지막 프레임 완전 투명, 기본색 소멸 알파·Godot 상승/스폰 책임 분리.
- 원본/제출/PNG 픽셀 불일치 0, 1px 윤곽 오류·잘림·미허용 색/알파 0. JSON 프레임 사각형·시간·태그 일치, source 해시 동일, 본체 r004 변경 없음.
- 낮/밤·3배경·1배/2배 합성에서 구분·표정 가독성 확인. 본체 50자세의 생성 위치에서 효과/Face 겹침 0. 97프레임·2600ms 반복 생성 리뷰를 Aseprite에서 실제 재생했다.
- 위 r001 초기 검사는 제작·합성 검수다. 후속 실제 엔진 시험은 아래와 구분한다. 사용자 최종 시각 확인 대기.
- 본체 r005: 50프레임 재합성·원본 대응 오류 0, 1200개 외곽선 렌더 조건의 색/알파/동기화 오류 0. 효과 16프레임×4조건 색/알파·끝 투명도 검사 통과(8bit 알파 합성 반올림 허용).
- [최종 재생](../../../docs/plans/evidence/PLAN-005/run-014/README.md): 본체 전환 486개, 최대 시간 오차 16.757ms/p95 13.172ms. 효과 850ms 논리 수명에 숨김 850.814ms 관측. [추가 비용](../../../docs/plans/evidence/PLAN-005/run-013/README.md): 6쌍 모두 예산 통과. 제품 스폰·입력·물리 통합 시험은 아니다.
- 2026-11-23 계획 문서 검사: 프로젝트 루트의 `node scripts/validate-agent-docs.mjs` 통과. 제작물 검증은 아니다.

## 관련 결정과 상태 전환

- [TECH-005](../../../docs/decisions/TECH-005-separate-art-plans.md).
- 2026-11-23: 계획 작성 요청에 따라 proposed 생성. 활성화 승인 없음.
- 2026-11-24: “Art-002를 승인할게 작업 시작해줘”에 따라 proposed→active. 본체 r004 확인으로 ART-001을 완료한 후 전환했다. ART-003은 proposed 유지.
- 2026-11-24: 사용자가 외곽선 조명 제외 지침/PLAN 보완과 ART-001 후속을 ART-002에 포함한 실행을 지시했다. 주 에셋 메타데이터를 유지하고 추가 Scp999 r005/최근 승인r004를 별도 표로 추적한다. ART-001 메타데이터와 r004 원본은 변경하지 않는다.
