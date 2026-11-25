# Scp999Trail r001 제출

- 2026-11-24, [ART-003](../../../../../docs/plans/ART-003-scp-999-trail.md), PLAN-005-SPR-003. 최초 제출, 본체 참조 Scp999 r005.
- [작업 원본](../../source/Scp999Trail.aseprite), [제출 원본](source/Scp999Trail.aseprite). Aseprite1.3.18.6/API41 Lua 제작·출력. RGB/sRGB.
- [PNG](export/Scp999Trail.png), [Aseprite JSON](export/Scp999Trail.json), [계약 JSON](export/Scp999Trail.contract.json).
- 32×16 고정 셀·288×32 시트·9열×2행, padding/spacing0. 트리밍/회전/빈 프레임 생략/중복 병합 없음.
- Fill #F78A2178 / Outline #53301E94. 1px 내부 다크 윤곽, RGBA straight alpha. 본체보다 낮은 알파이며 방향성 명암·하이라이트 없음.
- 출력 레이어 PuddleFill/PuddleOutline/DropletsFill/DropletsOutline, Guide 숨김. 피벗(16,8), 좌상단 원점·원본 픽셀 좌표.
- puddle_a 출력0~8, puddle_b 출력9~17. 각각 80/100/200/140/140/140/120/100/80ms, 1100ms 일회. Aseprite UI는 +1.
- 유지 프레임은 출력2/11. 생성180ms와 증발720ms를 분리하며 제품의 유지 시간은 개발 담당이다. 마지막 프레임은 완전 투명·추가 엔진 fade 없음.
- 그림 영역 a최대 x2~29/y5~10, b최대 x4~28/y5~12. 피벗 y8은 바닥 부착 중심이며 실루엣 최하단이 아니다.
- 전체 Unshaded/Mix·중립 부모·원본 알파 유지. 보조 맵 없음, 좌우 반전 가능. 낮/밤 합성에서 색 변조 후보와 비교해 채택했다.

## 리뷰와 검수

- [접촉표](review/contact-sheet.png), [1배 배경](review/backgrounds-1x.png), [2배 배경](review/backgrounds-2x.png), [겹침/반전](review/overlap-and-flip.png).
- [puddle_a 일회 재생](review/puddle_a.apng), [puddle_b 일회 재생](review/puddle_b.apng), [5초 이동·증발 합성](review/trail-preview.apng), [Aseprite 합성 재생 원본](review/trail-preview.aseprite).
- 합성은 Scp999 r005의 PLAN-005 run-014 실제 Godot 본체 렌더에 새 점액을 Aseprite로 합성한 리뷰다. 유지1800ms/생성450ms/이동속도.06px/ms는 검토용 예시다.
- [파일 검사](../../../../../docs/plans/evidence/ART-003/run-001/results.json): 18프레임·2태그, 원본/PNG 차이·잘림·색/알파·윤곽·겹침·시간 오류0, 증발 중 면적/알파 증가0, 두 끝 프레임 완전 투명.
- [패키지 검사](../../../../../docs/plans/evidence/ART-003/run-001/package-result.json): JSON 사각형·시간·태그 일치, 작업/제출 source 해시 일치, 본체 r005 원본 해시 보존. 합성152프레임/5000ms, 각 태그9프레임/1100ms.
- [실행 기록·검증 한계](../../../../../docs/plans/evidence/ART-003/run-001/README.md). 제출 당시 Godot 적용은 미검증이었다. 이후 검증은 아래 정식 적용 기록을 따른다.

## 사용자 확인

- [피드백](review/feedback.md): 2026-11-24 F-004에서 r001 최종 사용자 시각 승인. 제작 실행 승인과 별개로 현재 표현의 정식 적용을 명시적으로 승인했다.
- [전달 기록](../../handoff.md). PLAN-010에서 r001 정식 제품 적용 완료. [실제 엔진/입력 검증](../../../../../../docs/plans/evidence/PLAN-010/run-001/README.md).
