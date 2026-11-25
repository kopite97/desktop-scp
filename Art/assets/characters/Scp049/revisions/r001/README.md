# Scp049 r001 제출

- 제작일: 2026-11-24. [ART-004](../../../../../docs/plans/ART-004-scp-049-character.md), [명세](../../spec.md), [전달](../../handoff.md).
- 상태: 기술 검증 및 사용자 시각 확인 완료. 2026-11-24 “승인할게”로 r001 승인, 제품 적용 없음.
- 64×64 원본, 42프레임·10태그·8레이어, RGBA/sRGB, 1px 다크 윤곽, 128×128 표시 기준.
- 고정 명암/Unshaded 계약. 미세 텍스처를 줄이고 가면·망토·가죽·도구 구분을 우선했다.

## 원본과 전달본

- [편집 원본 보존본](source/Scp049.aseprite)
- [PNG 시트](export/Scp049.png), [Aseprite JSON](export/Scp049.json), [런타임 계약](export/Scp049.contract.json)
- 시트448×384 / 7열×6행, trim·rotation·padding·중복 병합 없음. 프레임별 시간/피벗/치료 접점은 contract에 보존했다.

## 시각 검수

- [레퍼런스 비교](review/reference-comparison.png), [전체 접촉표](review/contact-sheet.png)
- [1배 배경 비교](review/backgrounds-1x.png), [2배](review/backgrounds-2x.png), [4배](review/backgrounds-4x.png), [반전·겹침](review/flip-overlap.png)
- [실제 시간 GIF](review/animation.gif), [Aseprite 재생용 리뷰](review/Scp049-review.aseprite)
- 배경 비교의 행은 idle / move / treat_loop, 열은 밝음 / 어두움 / 무늬다. GIF는 같은 세 배경에서 원본·좌우 반전·2배를 함께 보여준다.
- [피드백과 사용자 승인](review/feedback.md). r001 형태·동작 시각 확인을 완료해 ART-004를 마감했다.

## 기술 결과

- [실행 기록](../../../../../docs/plans/evidence/ART-004/run-001/README.md), [검사 결과](../../../../../docs/plans/evidence/ART-004/run-001/results.json).
- 작업/제출 원본·PNG의 모든 합성 픽셀, 태그·프레임 시간·고정 셀 좌표 일치. 알파 오류0, 1px 외곽선 경계 오류0, 캔버스 경계 잘림0.
- 조명 없는 배경 합성과 Aseprite 재생은 제품 엔진 검증과 구분한다. Godot 적용·입력 윤곽·성능은 미검증이다.
