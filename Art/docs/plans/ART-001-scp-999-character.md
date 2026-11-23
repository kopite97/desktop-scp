---
id: ART-001
type: art
status: completed
title: SCP-999 본체와 행동 애니메이션
asset_id: Scp999
revision: 4
approved_revision: 4
updated: 2026-11-24
---

# ART-001 SCP-999 본체와 행동 애니메이션

## 목표와 범위

- 목표: PLAN-005의 추적·타이핑 반응·쓰다듬기와 기존 창 발판 물리에 연결할 정식 SCP-999 본체를 제작한다.
- AssetId: Scp999, 분류 characters. 반투명 주황 슬라임의 귀엽고 말랑한 형태와 감정 변화가 작은 화면에서도 읽혀야 한다.
- 포함: 정지·이동·타이핑 반응·쓰다듬기 및 도약/낙하/착지/균형 표현, 편집 원본·출력·리뷰·전달 명세. 조명용 기본색·노멀·반사 맵은 4장→idle/move 12장 관문 뒤 전체 50장으로 확장한다.
- 별도 제작: 하트/음표는 [ART-002](ART-002-scp-999-reaction.md), 점액 자국은 [ART-003](ART-003-scp-999-trail.md).
- 제외: SCP-173/049, 과부하로 녹는 모습(PLAN-008 범위), 정식 스트레스 UI, 효과음, Godot 코드·조명 구현.
- 요청 근거: 2026-11-23 계획 작성 후 사용자가 ART-001 실행을 승인했다. 실행 중 점액 팔과 바닥에 질질 끌리는 이동 표현을 요청했다.
- r001 고정 명암 시안, r002 4장, r003 idle/move 12장을 보존했다. 현재 r004는 전체 50프레임 조명용 맵이며 승인 리비전은 4다.

## 기준 문서와 선행 조건

- 기준: [기획 원본](../../../SCP_site-desktop.md)의 SCP-999·창 물리, [개발 PLAN-005](../../../docs/plans/PLAN-005-scp-999.md).
- 요청 ID: PLAN-005-SPR-001. 개발 적용 담당은 PLAN-005, 공통 물리 선행은 [PLAN-004](../../../docs/plans/PLAN-004-window-platforms.md).
- 필수 지침: [Art MAP](../../AGENTS.md), [작업 절차](../agents/workflow.md), [계획 관리](plan.md), [디자인](../agents/design.md), [애니메이션](../agents/animation.md).
- 도구·제출: [Aseprite](../agents/aseprite.md), [구조](../agents/project-structure.md), [리비전](../agents/revisions.md), [전달](../agents/handoff.md), [검증](../agents/verification.md).
- 제작은 PLAN-004 구현 완료와 별도로 승인받아 진행할 수 있다. 초기 상태 계약을 문서화하고 실제 물리 타이밍은 개발 통합에서 검증한다.
- 아래 확정 기준은 유지한다. r001의 형태·동작을 기준으로 조명용 샘플을 수정하고 spec에 채택 결과를 남긴다.
- 명세 양식은 [asset-spec](../../templates/asset-spec.md), 제출은 [revision](../../templates/revision.md), 적용 대응은 [handoff](../../templates/handoff.md)를 사용한다.

### Art에 요구하는 외형·규격

| 항목 | 요구와 확정 여부 |
| --- | --- |
| 캔버스 | 확정: 모든 본체 프레임 64×64 원본 픽셀. PNG도 1배 출력 |
| 표시 검수 | 확정: 1배와 nearest 2배(셀 기준 128×128) 확인. 확대 원본을 별도로 만들지 않음 |
| 실루엣 | 확정: 주황색 반투명 슬라임·SD 인상. 사람형 팔다리를 억지로 추가하지 않고 낮고 둥근 덩어리와 표정으로 전달 |
| 외곽선 | 확정: 원본 기준 1px 다크 아웃라인. 눌림/늘어남에서도 선 굵기와 윤곽 유지 |
| 알파·색 | 몸체의 반투명과 읽히는 표정/윤곽을 분리 조절. RGBA 출력, 사용 팔레트·알파 값·색상 프로파일을 명세에 기록 |
| 그림 영역 | 제작안: 중립 자세를 중앙 하단에 두고 위/좌우로 변형 여백 확보. 최댓값은 모든 동작의 불투명/반투명 픽셀 경계로 기록 |
| 피벗 | 제작안: 좌상단 (0,0), X 오른쪽/Y 아래, 바닥 기준 (32,56) 원본 픽셀. 샘플에서 확정하며 모든 태그에 동일 기준 적용 |
| 시점·방향 | 제작안: 얼굴이 읽히는 정면에 가까운 3/4 시점, 우측 이동 원본+좌측 반전 검토. 표정/광원 비대칭이면 별도 좌측 프레임 필요 |
| 레이어 | 기본색·윤곽·표정·노멀·반사 데이터를 분리. 고정 방향 하이라이트와 참조 레이어는 기본색 출력에서 제외 |
| 연결점 | 반응 효과 발생점과 바닥 점액 발생점을 원본 픽셀 좌표로 명시. 변형 중 이동하면 프레임별 좌표 제공 |

### 동작 요구와 개발 상태 대응

아래 태그는 요청 계약안이며 현재 구현된 상태 이름이 아니다. 개발 연결 시 실제 상태→태그 표를 작성한다.
프레임 수·시간은 동작별 샘플로 정하고 모든 프레임의 ms를 제출한다. 공통 FPS 하나로 일괄 고정하지 않는다.

| 태그안 | 필요한 모습 | 재생·종료 계약 |
| --- | --- | --- |
| idle | 미세한 호흡·젤리 출렁임, 안정된 표정 | 반복. 마지막→첫 프레임의 부피와 접점이 이어짐 |
| move | 진행 방향으로 눌렸다 늘어나며 이동하는 느낌 | 반복. 실제 위치 이동은 Godot 담당, 원본에 누적 수평 이동을 그리지 않음 |
| typing_react | 신난 표정·웅크림→위로 늘어남→회복 | 일회. 끝은 idle로 연결 가능, 연속 입력 재시작/묶음 처리는 개발 담당 |
| pet | 클릭/문지름에 웃으며 젤리처럼 눌리고 출렁임 | 입력 중 반복 가능한 구간과 해제 후 회복 구간을 태그/프레임으로 구분 |
| takeoff | 도약 전 압축과 떠오르는 신장 | 일회. 물리 도약 이벤트와 대응할 기준 프레임 명시 |
| fall | 공중에서 아래로 늘어난 모습 | 체공 동안 유지/반복 가능. 실제 낙하 궤적을 프레임에 굽지 않음 |
| land | 바닥 접촉→넓게 눌림→원형 회복 | 일회. 착지 이벤트 기준, 종료는 idle/move 연결 |
| balance | 발판 이동 시 기울고 흔들리며 균형 회복 | 시작·유지·해제 구간 명세. 실제 관성/미끄러짐은 개발 담당 |

- typing_react의 통통 튀기는 셀 내부 몸체 변형과 Godot 위치 이동의 분담을 명시해 이중 점프를 방지한다.
- pet의 눌림 방향, 반복/회복 구간의 정확한 태그명·프레임은 승인된 제작 범위에서 정한다.
- 방향 전환·pet 해제·land→move의 접점 흔들림과 부피 급변을 실제 재생으로 확인한다.
- 2026-11-23 사용자 수정: 짧은 점액 팔을 추가하고, move는 바닥에 붙어 후방 몸체가 늘어졌다 회수되는 표현으로 제작한다. 이동 바운스는 제거한다.
- 하트/음표·점액·바닥 그림자를 본체 PNG에 합치지 않는다. 본체 투명 여백을 클릭 영역으로 사용하지 않는다.

### 광원·재질 준비

- 기준은 [조명과 재질](../agents/lighting.md)이다. r001은 Body 고정 명암과 Highlights를 합친 형태/동작 시안이며 조명 대응 완료본이 아니다.
- 목표는 동적 조명 대응이다. r002는 Body의 고정 명암과 Highlights를 제거한 기본색·노멀·반사 맵 샘플이다. 발광은 요구하지 않는다.
- 대표 샘플: r001의 Aseprite 프레임 1(idle), 7·9·12(move), 총 4장. r002 시트에서는 인덱스 0~3이다. 빠진 중간 프레임이 있어 완전한 이동 루프로 사용하지 않는다.
- 기본색의 몸체는 #F78A21D3로 중립화했다. 표정·1px 윤곽·점액 팔·꼬리 실루엣은 보존했다. 눈의 작은 밝은 점은 표정 그래픽이며 몸체 재질 반사와 구분한다.
- 노멀은 실루엣 거리 기반 높이장의 기울기로 만들었다. 기존 그림 밝기를 변환하지 않았다. 팔·꼬리를 포함한 r004 전체 자세는 PLAN-005 run-010의 실제 조명 렌더에서 확인했다. 최종 사용자 시각 승인은 별도다.
- 엔진 시험 담당은 [PLAN-005 단계 A](../../../docs/plans/PLAN-005-scp-999.md), 현재 active다. PLAN-009의 독립 후보는 이 범위로 통합했다.
- 2026-11-23 사용자 지정 순서: r002 4장 광원/반사/반투명/반전 검증 → r003 idle 6장+move 6장 → 실제 시간 재생의 명암·반사 떨림 검증 → 나머지 동작 확장. r002는 보존한다.
- 시간대별 색·빛 이동·그림자·공유 시간 상태는 Godot 담당이다. 실제 로컬 시간 연동과 제품 적용은 이번 Art 수정에 포함하지 않는다.
- r001 원본·출력·검수 기록은 보존한다. 기존 형태 확인을 조명 완료나 최종 승인으로 확대하지 않는다.

## 결과물 저장 위치

기준 루트는 C:/Dev/scp_desktop/Art. r001은 전체 동작 시안, r002는 조명 샘플이다. 제품 복사는 아직 예정이다.

| 산출물 | 경로 |
| --- | --- |
| 현재 명세·전달 기록 | assets/characters/Scp999/spec.md, handoff.md |
| 편집 원본 | assets/characters/Scp999/source/Scp999.aseprite |
| r001 제출 원본 | assets/characters/Scp999/revisions/r001/source/Scp999.aseprite |
| 출력 | assets/characters/Scp999/revisions/r001/export/Scp999.png, Scp999.json |
| 제출 명세 | assets/characters/Scp999/revisions/r001/README.md |
| 리뷰 | assets/characters/Scp999/revisions/r001/review/contact-sheet.png, background-comparison.png, playback.apng, feedback.md |
| 기술 검사 기록 | docs/plans/evidence/ART-001/run-001/README.md, results.json, execution.log |
| r002 편집·제출 원본 | assets/characters/Scp999/source/Scp999-lighting-sample.aseprite, revisions/r002/source/Scp999-lighting-sample.aseprite |
| r002 출력 | assets/characters/Scp999/revisions/r002/export/Scp999_color.png, Scp999_normal.png, Scp999_specular.png, Scp999.contract.json |
| r002 제출·리뷰 | assets/characters/Scp999/revisions/r002/README.md, review/map-comparison.png, base-backgrounds.png, feedback.md |
| r002 검사 기록 | docs/plans/evidence/ART-001/run-002/README.md, results.json, execution.log |
| 제품 복사 대상 | 프로젝트 루트 GodotProject/Assets/Sprites/characters/Scp999/ — 개발 담당 |

- PNG는 64×64 고정 셀, 트리밍·회전·중복 프레임 병합 없음. 시트 행/열·여백/간격·출력 순서를 명세한다.
- JSON은 프레임 사각형·프레임별 ms·태그 범위/방향을 제공한다. 피벗·접점·레이어·알파·표정 설명 등 누락 필드는 제출 README에 보완한다.
- Aseprite 편집 인덱스(1부터), 출력 JSON의 실제 인덱스, Godot 매핑을 구분한다. 자동 importer가 있다고 가정하지 않는다.
- 재생 리뷰는 알파를 보존하는 APNG 또는 동등한 실제 시간 재생 자료를 사용한다. 변환 도구가 없으면 Aseprite 재생 녹화와 환경을 기록한다.
- r002는 4열×1행, 256×64 PNG 3종과 전용 contract JSON을 제공한다. 전체 동작 재생 자료는 r001을 참조하며 샘플에 완전한 루프 영상을 붙이지 않는다.
- r003은 idle/move 12열×1행, 768×64 PNG 3종과 계약 JSON을 제공한다. r004는 전체 50열×1행, 3200×64다. 후속 제출은 r005부터 증가한다.
- spec은 최신값, 리비전 README는 제출 당시값을 보존한다. Aseprite 입력/출력은 현재 Art 절대 경로를 사용한다.
- 실제 자료: [명세](../../assets/characters/Scp999/spec.md), [r001 제출본](../../assets/characters/Scp999/revisions/r001/README.md), [전달 기록](../../assets/characters/Scp999/handoff.md), [실행 증거](evidence/ART-001/run-001/README.md).
- 조명 샘플: [r002 제출본](../../assets/characters/Scp999/revisions/r002/README.md), [run-002 증거](evidence/ART-001/run-002/README.md).
- idle/move 전체: [r003 제출본](../../assets/characters/Scp999/revisions/r003/README.md), [Art 검사](evidence/ART-001/run-003/README.md), [Godot 재생 검사](../../../docs/plans/evidence/PLAN-005/run-007/README.md).
- 전체 동작: [r004 제출본](../../assets/characters/Scp999/revisions/r004/README.md), [Art 검사](evidence/ART-001/run-004/README.md), [Godot 검사](../../../docs/plans/evidence/PLAN-005/run-010/README.md). 작업 원본 source/Scp999-lighting.aseprite, 출력 revisions/r004/export 아래 PNG 3종·계약 JSON.

## 작업 목록

- [x] 실행 승인·대상 원본 확인, Aseprite 상태 확인, spec 작성
- [x] 중립 모습+짧은 idle/move 샘플로 팔레트·알파·피벗·방향/반전·여백 검토
- [x] 사용자 피드백을 리비전과 연결하고 범위 내 수정
- [x] 전체 요청 동작·연결점 제작, 태그/프레임별 시간·반복/회복 계약 확정
- [x] 고정 셀 PNG·JSON과 제출 원본 보존, 기술 검사·재생 리뷰
- [x] r001 보존, idle 1장·move 3장의 r002 기본색·노멀·반사 맵 분리 제작
- [x] 샘플 3종의 셀·순서·피벗·알파 계약, 원본/출력 일치·윤곽/표정 보존 검사 및 리뷰
- [x] 엔진 시험 담당 PLAN-009 proposed 작성, handoff·spec·개발 요청 연결 갱신
- [x] PLAN-005 단계 A에서 r002 4장 엔진 샘플 결과 연결·수정
- [x] 4장 관문 후 r003 idle/move 12프레임 제작·실제 시간 반복 재생의 떨림 검증
- [x] 두 관문 품질/비용 확인 뒤 나머지 동작을 포함해 전체 50프레임 맵 확장
- [x] 전체 프레임·태그 내부 재생/반전 진단 검증, handoff 및 ART 목록 갱신
- [x] r004 최종 사용자 시각 확인. 남은 1배 Godot 표시·제품 상태 전환은 PLAN-005 담당이며 미실행으로 전달

## 완료 기준과 검증 방법

| 기준 | 확인 방법 | 증거 |
| --- | --- | --- |
| 외형·64×64·1px 윤곽·알파 충족 | 원본 정보/출력 크기/레이어 대조, 1배·2배 밝음/어두움/무늬 배경 비교 | 제출 README·background-comparison |
| 모든 요청 동작 제공 | 태그 목록과 위 표 대조, 실제 ms 재생·반복 경계·전환 확인 | contact-sheet·playback·검사 기록 |
| 접점·형태·창 여백 안정 | 최대 변형·방향 반전·착지 프레임 비교 | 명세 좌표·리뷰 |
| 출력으로 재구성 가능 | PNG 셀·JSON 태그/시간·피벗/연결점 대조 | results.json·handoff |
| 조명 샘플 품질·비용 확인 | PLAN-005 단계 A의 광원/배경/반전/창/비용 검사, 파라미터·채택 근거 기록 | PLAN-005 실제 실행 증거 |
| idle/move 시간 연속성 | 12프레임 전체와 원래 ms로 재생, 정/역방향 광원과 반전에서 루프 경계·명암/반사 변화 확인 | r003 제출·렌더/재생 자료 |
| 모든 변형에 대응하는 조명 맵 | 샘플 통과 후 전체 50프레임으로 확장, 같은 셀·순서·시간·피벗 검증, 실제 재생 중 명암 떨림/경계 확인 | 후속 제출본·전체 프레임 검증 기록 |
| 사용자 시각 확인 | 리비전·파일·날짜·실제 의견 기록 | feedback.md, approved_revision=현재 revision |

전체 Godot 제품 적용은 PLAN-005의 검증이며 이 ART의 선행 조건은 아니다. 조명용 리소스가 포함되므로 엔진 샘플 검증은 필수다. 파일 검사만으로 이를 통과 처리하지 않는다.

## 현재 상황과 다음 행동

- 현재: completed, revision=4, approved_revision=4. r004 파일·Art 1배/2배 리뷰·엔진 조명 샘플/전체 동작 검사와 사용자 시각 확인 완료.
- 다음: 사용자가 승인한 ART-002에서 r004를 참조한다. 1배 Godot 표시·제품 상태 전환·정식 제품 통합은 PLAN-005에 남아 있으며 완료로 처리하지 않는다. ART-003은 proposed 유지한다.
- 선행 협의: 개발 상태 이벤트·접점/효과 연결점 계약. PLAN-004 완료 대기 때문에 독립적인 승인된 제작을 멈출 필요는 없다.

## 검증 결과

- 제작·파일 기술 검사: 50프레임·10태그, 원본/PNG 불일치 0, 경계 윤곽 누락 0, 50프레임 반투명 유지·경계 잘림 0. [결과](evidence/ART-001/run-001/results.json).
- move: 6프레임 740ms, 바닥 끝 y=56 고정. 점액 팔·후방 늘어짐 반영, 피드백 F-001/F-002 기록.
- 시각 검토: 접촉표·세 배경·1배/2배·좌우 비교, Aseprite Live 실제 재생 확인. 실제 게임 상태 전환 검증은 미실행.
- 최종 사용자 시각 확인: r004 확인 완료(아래 2026-11-24 전환 기록). 정식 제품 적용은 미실행이며 Godot 진단 적용과 구분한다.
- 2026-11-23 F-003(r001): 사용자는 큰 이질감은 없다고 평가하고 조명 처리 방식을 확인했다. 형태에 대한 의견이며 조명·전체 완료 승인으로 해석하지 않는다.
- 2026-11-23 r002: 4프레임 원본/출력 불일치·윤곽/표정 변경·커버리지 변경·잘림·비균일 Body·유효하지 않은 노멀/반사 값 모두 0. [검사 결과](evidence/ART-001/run-002/results.json). 엔진/최종 사용자 확인은 미실행이다.
- 2026-11-23 계획 문서 검사: 프로젝트 루트의 `node scripts/validate-agent-docs.mjs` 통과. 제작물 검증은 아니다.
- 실제 채택한 제작값과 프레임별 시간은 에셋 명세·r001 계약에 기록했다. 제품 구현 완료를 뜻하지 않는다.
- 2026-11-24 r003: Art 픽셀 검사 오류 0, Godot 12프레임·좌/우/위·낮/밤·정/반전의 알파 오류 0, 좌우 대응 평균 RGB 오차 0. 반사 ON/OFF 차이 확인.
- PLAN-005 run-009의 idle/move 애니메이션 비용 6쌍 모두 예산 통과. 이 관문 뒤 r004를 확장했다. 전체 50프레임의 별도 비용 측정은 하지 않았다.
- 6개 광원/반전 조건에서 실제 반복 재생 전환 318개 확인. 시작 프레임 시간 누적 오류 수정 후 최대 시간 오차 13.413ms(60Hz 1프레임 이내). 렌더 프레임과 실제 창 상태 비교에서 루프 경계의 추가 반사 섬광은 관찰되지 않았다. 변형·눈 깜빡임은 의도된 변화로 분리했다.
- r004: 50프레임·10태그 파일 검사 오류 0, 엔진 1800조건의 알파 오류·좌우 대응 RGB 오차 0. 실제 재생 전환 486개, 최대 시간 오차 13.781ms. 모든 태그의 렌더/재생 자료는 PLAN-005 run-010에 있다. 제품의 입력/물리 이벤트에 따른 태그 간 전환은 미검증이다.

## 관련 결정과 상태 전환

- [TECH-005](../../../docs/decisions/TECH-005-separate-art-plans.md): 제작 ART와 개발 PLAN 분리.
- [TECH-006](../../../docs/decisions/TECH-006-lighting-aware-art.md): 조명 방식 명세·샘플 검증 후 전체 맵 확장. 2026-11-23 지침 수정 반영, status/승인 리비전 변화 없음.
- 2026-11-23: 사용자의 PLAN-005 관련 아트 계획 작성 요청으로 proposed 생성.
- 2026-11-23: 사용자의 “Art플랜 001 부터 실행해줘. Aseprite는 실행시켜 두었어” 지시로 proposed → active. ART-001 본체·동작·출력·검수 범위 실행 승인.
- 2026-11-23: “Art-001에 수정이 필요한 부분을 작업해줘” 요청으로 조명 지침을 반영한 r002 샘플 제작·계획 수정. active 유지, revision 1→2, approved_revision=0 유지. 개발 PLAN 활성화는 없음.
- 2026-11-23: 사용자가 PLAN-005 활성화와 4장→idle/move 전체 프레임 검증을 지정했다. ART active 유지, 조명 전체 확장 전에 12프레임 재생 관문 추가. 최종 시각 승인은 별개다.
- 2026-11-24: r003 idle/move 12프레임 제출·엔진 검증 결과 연결. revision 2→3, approved_revision=0·active 유지.
- 2026-11-24: 두 관문 통과 후 요청 순서대로 r004 전체 동작 확장. 50프레임·10태그 검사, 1800 조명 조건과 6개 재생 조건 확인. revision 3→4, approved_revision=0·active 유지.
- 2026-11-24: r004 결과 제시 및 다음 순서 안내 뒤 사용자가 “Art-002를 승인할게 작업 시작해줘. 일단 Art-001은 괜찮아 보여.”라고 확인했다. 현재 r004 시각 확인으로 기록, approved_revision 0→4, active→completed. 기존 파일/엔진 증거와 결합한 제작 완료이며 미실행 제품 검증을 통과로 바꾸지 않는다.
