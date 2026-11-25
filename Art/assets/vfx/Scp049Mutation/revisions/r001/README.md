# Scp049Mutation r001 제출 기록

- 날짜: 2026-11-25. 제작 [ART-007](../../../../../docs/plans/ART-007-scp-049-mutation.md), 요청 PLAN-006-SPR-004. 최초 제출, 사용자 시각 승인 완료.
- [현재 명세](../../spec.md), [개발 전달](../../handoff.md), 편집용 [원본](../../source/Scp049Mutation.aseprite), 보존용 [원본](source/Scp049Mutation.aseprite).
- 출력: [PNG](export/Scp049Mutation.png), [JSON 계약](export/Scp049Mutation.contract.json).
- Aseprite1.3.18.6/API41 Live, 직접 만든 도트 호·입자. 48×48 RGB/sRGB 원본, RGBA PNG192×192=4열×4행. Aura/Particles를 합성하며 패딩·trim·회전·중복병합 없음.
- 피벗(24,40), 제품 nearest2배/96×96. 전체 알파 경계x2..45/y2..45, 중앙24×24 완전 투명. 원본 아이콘/발/049 도구 그림 없음.
- treat 프레임1–8 각120ms 반복(960ms), transform9–16 각100ms 일회(800ms, 마지막100ms 완전 투명). 계층·반전·중단·봉쇄·종료는 JSON과 handoff에 명시한다.
- Unshaded/Mix/중립 부모, 고정 색·straight alpha. 추가 fade와 동적 노멀·반사 맵 없음. 조명·입력·성능 실제 검증은 후속 개발 범위.

## 검수

| 항목 | 방법·실제 결과 | 증거 |
| --- | --- | --- |
| 파일/프레임 | 두 Aseprite 원본을 재개방, PNG32프레임·시간 일치, 16개 고유 프레임 | [23개 기술 검사](../../../../../docs/plans/evidence/ART-007/run-001/results.json) PASS |
| 중앙/경계 | 모든 프레임24×24 alpha=0, 셀 가장자리 투명, 마지막 transform 완전 투명 | 같은 결과 |
| 아이콘 합성 | 여섯 자체 도형의 그림 픽셀 불변, ART-006 r001 발과 합성 | [치료](review/Scp049Mutation-treat.png), [변이](review/Scp049Mutation-transform.png) |
| 배경/배율/겹침 | 세 배경·1배/2배·양방향 발·8개 겹침 | [GIF](review/Scp049Mutation-review.gif), [리뷰 원본](review/Scp049Mutation-review.aseprite) |
| 시계·사건 | 모의 일반3000ms/과부하2000ms/변이800ms, 취소·재시작·삭제 | [모의 타임라인](../../../../../docs/plans/evidence/ART-007/run-001/mock-timeline.json) |
| 화면 경계 | 몸체/발을 영역 안에 두고 효과만 경계에서 마스크 | [모의 경계 그림](review/Scp049Mutation-boundaries.png) |
| 실제 재생 | LG Aseprite의 전체86프레임/9.72초 모의 영상을 순환 | [재생 기록](../../../../../docs/plans/evidence/ART-007/run-001/playback.json) |
| 사용자 시각 확인 | 2026-11-25 “ART-007 r001 시각 승인” | [피드백](review/feedback.md) |
| 제품 적용 | 미실행 | [handoff](../../handoff.md) |

## 사용자 확인

- r001의 치료 오라·변이 색감/속도·아이콘 가독성을 포함한 전체 시각 승인 완료. 남은 수정 요청 없음.
- 기술 검사와 모의 사건 재생을 실제 Godot 구현 완료 또는 사용자 승인으로 간주하지 않는다.
