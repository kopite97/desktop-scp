# Scp049 Godot 전달 기록

- 요청: [PLAN-006](../../../../docs/plans/PLAN-006-scp-049.md), PLAN-006-SPR-001. 제작 [ART-004](../../../docs/plans/ART-004-scp-049-character.md).
- [명세](spec.md), [r001 제출](revisions/r001/README.md). 정식 제품 수령/적용은 [PLAN-013](../../../../docs/plans/PLAN-013-scp-049-art-integration.md)에서 완료했다.
- 교체 대상: PlagueActorWindow의 임시 코드 도형. 이번 ART에서 제품 코드를 변경하지 않았다.
- 정식 적용 경로: `GodotProject/Assets/Sprites/characters/Scp049/Scp049.png`, `Scp049.contract.json`.

| 리비전 | 제출 export | 기술 확인 | 제품 수령/적용 | 사용자 시각 확인 |
| --- | --- | --- | --- | --- |
| r001 | [PNG](revisions/r001/export/Scp049.png), [계약](revisions/r001/export/Scp049.contract.json), [Aseprite JSON](revisions/r001/export/Scp049.json) | 42프레임·픽셀/알파/태그/시간·경계 검사 PASS | 2026-11-25 PLAN-013 적용·검증 완료 | 2026-11-24 “승인할게”, [기록](revisions/r001/review/feedback.md) |

- 표시: nearest 2배, Unshaded/Mix, 중립 부모, sRGB/straight RGBA, 피벗 (32,56). 고정 명암 포함.
- 셀64×64, 7열×6행. 태그별 시간·loop·도구 접점은 contract와 spec을 따른다. Aseprite 1-based와 contract 0-based를 구분한다.
- 현재 코드 도형의 입력 윤곽을 그대로 쓰지 말고 정식 스프라이트 알파에 맞춘 입력·클릭 투과를 검증해야 한다.
- 월드 접지·드래그·치료 취소·동적 창 순서는 제품 상태와 공통 관리자 소유다. 프레임 선택을 별도 상태 원본으로 만들지 않는다.
- 제품 검증 완료: 실제 Godot42프레임 양방향 표시/알파 입력, 낮/밤 색 불변성, 반전 접점,10태그 상태 연결, 실제 클릭 투과/999 겹침 드래그·숨김·정리·짧은 비용 비교. [증거](../../../../docs/plans/evidence/PLAN-013/run-001/README.md).
- 승인 리비전 r001로 고정하여 전달 준비 완료. ART-004 제작은 마감했으며 제품 수령·적용은 별도 개발 PLAN에서 기록한다.

- 제품 화면 확인: 2026-11-25 LG 실행 화면에 사용자 “정식 적용 화면 승인”. 원본 r001 시각 승인과 별도 기록이며, 좀비 팔다리/변이 효과는 이번 적용에 포함하지 않았다.
