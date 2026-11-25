# ART-003 run-001 — Scp999Trail r001

- 날짜 2026-11-24, [ART-003](../../../ART-003-scp-999-trail.md), [제출본](../../../../../assets/vfx/Scp999Trail/revisions/r001/README.md).
- Windows / Aseprite1.3.18.6 Lua API41 / Node26.13.0. 본체 참조는 사용자 승인 Scp999 r005, 기존 PLAN-005 run-012/014 렌더.
- 요청: ART-003 제작 실행, Aseprite에서 작업 화면 표시. 중간 Computer Use 중단 후 재개 요청을 받아 이후 Aseprite Lua/CLI만 사용했다.
- MCP Live는 port41234 점유로 연결되지 않았다. 다른 인스턴스·전역 설정을 변경하지 않았다. 직접 실행한 Aseprite GUI에 샘플/작업 원본/합성 미리보기 파일을 열었으며 파일 검사에는 batch를 사용했다.

## 제작·선택

- [제작 Lua](build-trail.lua): 32×16, 넓은 얼룩·방울군, 각9프레임. Fill(247,138,33,120)와 Outline(83,48,30,148), 숨김 Guide. 생성180ms·유지200ms·증발720ms, 각1100ms 일회.
- [샘플](samples.png), [조명 후보1배](lighting-candidates-1x.png), [2배](lighting-candidates-2x.png). 위 두 행 낮/밤 unlit, 아래 두 행 낮/밤 색 변조 후보. 실제 Aseprite 합성이며 엔진 비교 시험이 아니다.
- 선택: unlit 낮은 알파. 야간 RGB .45/.50/.62 후보는 회색으로 보이므로 채택하지 않았다. 낮/밤 모두 주황 재질이 읽히며 본체보다 강조가 낮다. 방향성 명암·반짝임·보조 맵 없음.
- [리뷰 Lua](review-trail.lua): 밝음/어두움/무늬·1배/2배·겹침/반전과 독립된 자국을 남기는 이동 합성. 본체는 기존 Godot 렌더, 새 점액은 Aseprite 이미지다.
- [합성 타임라인](composite-review.json): 152프레임·5000ms. 유지1800ms·총2700ms·450ms 생성 간격은 검토용 예시이며 제품 규칙을 확정하지 않는다.

## 검사와 실제 결과

| 항목 | 실행·기대 | 결과 |
| --- | --- | --- |
| 저장 원본·시트 | [audit-trail.lua](audit-trail.lua)로 작업/제출 원본을 재개방, 모든 픽셀 비교 | [results.json](results.json): 차이0, 18프레임·2태그·32×16·288×32 |
| 윤곽·팔레트·알파 | 실루엣 경계 1px, fill/outline 겹침 없음, 허용 RGB/alpha, 셀 경계 분리 | 윤곽·색·알파·잘림·겹침 오류0 |
| 증발 | 유지 이후 픽셀 수·알파 합 감소, 양쪽 끝 완전 투명 | 증가0, 끝 유효 픽셀0 |
| 시간·태그·APNG | [package-review.mjs](package-review.mjs), Aseprite JSON 대조, PNG CRC·RGBA·APNG 타이밍 구성 | [package-result.json](package-result.json): PASS, 각각9프레임/1100ms·일회, 합성152프레임/5000ms |
| 원본 보존 | 작업/제출 SHA256 동일, 참조 본체 r005 이전 해시 대조 | b95919df…cfff3 일치, 본체 cf6fa4cb…b290 보존 |
| 시각 검토 | 후보·접촉표·배경·겹침/반전·합성 시간별 PNG를 직접 열어 비교 | 두 형태 구분, 배경 사각형·셀 잘림 없음. 낮/밤 낮은 강조 확인 |
| GUI 표시 | Aseprite 프로세스의 열린 파일 제목 확인 | Scp999Trail.aseprite 및 trail-preview.aseprite 확인 |

- [합성 생성 결과](review-build.json), [실행 기록](execution.log).
- [GUI 재생 스크립트](show-preview.lua)로 미리보기를 열고 자동 재생/프레임 진행 기록을 시도했다. `gui-playback.json`이 생성되지 않아 자동 재생 성공이나 실시간 프레임 관측 완료로 판정하지 않는다. 재생 자료는 APNG와 편집 가능한 Aseprite 파일로 제공한다.
- 이번 턴에서 실제 GUI 애니메이션을 Computer Use로 재관찰하지 않았다. 타이밍/소멸 파일 검증과 시간별 합성 PNG 검토를 실제 화면 재생 관찰로 대체했다고 보고하지 않는다. 사용자 재생·시각 확인이 남아 있다.
- 새 점액의 Godot 재질/조명 제외·생성/수명/발판·native 창·입력·성능 검증은 미실행이다. PLAN-005 전달 계약에 남겼다.
- 최종 사용자 시각 확인은 대기하며 ART-003 active/approved_revision0을 유지한다. 문서 검사 결과는 ART 본문에 기록한다.
