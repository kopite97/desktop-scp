# Scp0492 r001 제출 기록

- 날짜: 2026-11-25. 제작 [ART-006](../../../../../docs/plans/ART-006-scp-049-2-zombie.md), 요청 PLAN-006-SPR-003.
- 최초 제출. 손 없이 초록·보라 발만 붙이는 방향. [현재 명세](../../spec.md)와 [전달 계약](../../handoff.md).
- 편집용 [원본](../../source/Scp0492.aseprite), 보존용 [원본](source/Scp0492.aseprite).
- 출력: [왼 발](export/Scp0492_left-foot.png), [오른 발](export/Scp0492_right-foot.png), [기계 판독 계약](export/Scp0492.contract.json).
- Aseprite1.3.18.6/API41 Live. Lua 픽셀 클러스터로 제작, 레이어별 고정 셀 PNG 조립. RGBA/sRGB/straight alpha, 원본32×32, 시트192×160=6열×5행. 패딩·trim·회전·중복병합 없음.
- 뿌리(16,28), 최대 그림 경계x12..22/y26..31, nearest2배. 두 레이어는 좌/우 슬롯이며 실제 아이콘은 원본/출력에 포함하지 않는다.
- 30프레임/7태그. 프레임별 ms·반복·몸체 fade·각 파일·부착 수치는 위 JSON과 명세에 보존한다.
- Unshaded/Mix, 중립 부모, 고정 명암. 노멀·반사 맵 없음. 제품 조명/입력/성능 검증은 후속 개발 범위.

## 검수

| 항목 | 방법·실제 결과 | 증거 |
| --- | --- | --- |
| 원본/제출/PNG | 120개 레이어 프레임 픽셀 일치, 60개 원본/보존 시간 일치 | [기술 결과](../../../../../docs/plans/evidence/ART-006/run-001/results.json) |
| 외곽선/알파 | 어두운1px 외곽선, 투명 RGB=0, 마지막 소멸 프레임 alpha=0 | 같은 결과34개 검사 PASS |
| 합성 | 여섯 도형×양방향×30프레임이64px 안에 들어감, 구멍/로고 보존 | [전체 화면](review/Scp0492-overview.png), [상세](review/Scp0492-details.png) |
| 배경/겹침/배율 | 밝음·어두움·무늬, 8개 겹침, 원본1배/제품2배 | [애니메이션](review/Scp0492-review.gif), [리뷰 원본](review/Scp0492-review.aseprite) |
| 실제 재생 | LG Aseprite에서10초 동안30프레임 전부 관찰 후 반복 재생 | [재생 기록](../../../../../docs/plans/evidence/ART-006/run-001/playback.json) |
| 사용자 시각 확인 | 2026-11-25 “ART-006 r001 시각 승인” | [피드백](review/feedback.md) |
| 제품 적용 | 미실행. 소멸 수명·발 합성·알파 입력 등의 개발 필요 | [handoff](../../handoff.md) |

## 사용자 확인

- 형태·색감·보행·소멸을 포함한 r001 전체 시각 승인 완료. 독립적인 동적 조명 승인을 뜻하지 않는다.
- 사용자 요청대로 ART-006 검수 뒤 ART-007로 진행한다. 별도 수정 요청 없음.
