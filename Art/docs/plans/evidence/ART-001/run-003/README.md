# ART-001 run-003 — idle/move 조명 맵

- 날짜: 2026-11-24. 사용자 지정 순서의 두 번째 단계, [ART-001](../../../ART-001-scp-999-character.md).
- 선행: [PLAN-005 run-004](../../../../../../docs/plans/evidence/PLAN-005/run-004/README.md)의 r002 4장 광원·반사·알파·반전 확인.
- Aseprite 1.3.18.6 Live/API 41, 현재 Art 절대 경로를 사용했다.
- [build-idle-move.lua](build-idle-move.lua): r001 UI 1~12를 읽어 r002와 같은 비방향성 Body·표정/윤곽·형상 기반 노멀·반사 맵으로 확장했다.
- [audit-idle-move.lua](audit-idle-move.lua): 실제 픽셀 대조 오류 0. [results.json](results.json), [execution.log](execution.log).
- r003 원본: 64×64·RGB/sRGB·12프레임·6레이어. 768×64 맵 3종, idle 6장 1040ms / move 6장 740ms.
- 작업 원본과 제출 사본 SHA256 일치. r001/r002 제출 원본 해시도 이전 기록과 동일하다.
- [r003 제출](../../../../../assets/characters/Scp999/revisions/r003/README.md), Godot 재생/입력 검증은 [PLAN-005 증거](../../../../../../docs/plans/evidence/PLAN-005/run-007/README.md)에 연결한다.
- 최종 사용자 시각 승인 없음, 다른 행동 38프레임 조명 맵은 아직 제작하지 않았다.
