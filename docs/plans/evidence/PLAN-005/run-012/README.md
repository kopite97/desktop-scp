# PLAN-005 run-012 — 전체 50프레임 외곽선 분리

- [PLAN-005](../../../PLAN-005-scp-999.md), [Scp999 r005](../../../../../Art/assets/characters/Scp999/revisions/r005/README.md), Scp999Reaction r001.
- 실행: `./scripts/godot.ps1 -Action LightingProbe -Revision r005 -EvidenceDirectory docs/plans/evidence/PLAN-005/run-012 -LogFile docs/plans/evidence/PLAN-005/run-012/execution.log -Preview`.
- 샘플 run-011 통과 후 같은 분리·재질 경로를 50프레임에 적용했다. 이전 증거를 덮어쓰지 않는다.
- Windows / Godot 4.7.2 .NET / Compatibility / NVIDIA RTX 5070. [로그](execution.log), [기본 렌더 결과](results.json), [외곽선 결과](outline-results.json), [VFX 결과](reaction-results.json), [창 속성](windows.json).
- 50프레임 × 2배율 × 좌/우/위 × 낮/밤 × 정/반전 = 1200 외곽선 검사 조건. RGB 오류 0, alpha 오류 0, frame/flip/scale 동기화 오류 0. 기존 기본색/노멀/반사 비교 1800조건도 검사했다.
- lit 몸체는 광원 방향에 반응하고 외곽선은 원본 색을 유지한다. 의도적으로 부모 modulate를 바꾼 대조군은 RGB가 달라졌으며, 중립 부모+몸체 self_modulate 구성에서는 외곽선 차이 0.
- [이전/이후](outline-before-after.png), [1배](outline-1x.png), [전체 동작](actions-overview.png), [밝음/어두움/무늬](backgrounds.png), [낮](lighting-day.png), [밤](lighting-night.png), [효과](reaction-unshaded.png).
- [재생 시간](playback-review.json): 487전환, p95 13.258ms. 첫 프레임만 220ms 기대에 145.295ms로 짧아 최대 오차 74.705ms였다. 이 회차의 재생 시간 관문은 실패로 남긴다. 다른 프레임 최대 오차는 13.798ms다.
- 긴 분석 뒤 엔진 delta 누적 대신 단조 경과 시간 기반 진단 시계로 수정했다. 최종 재생/비용 결과는 [run-013](../run-013/README.md)에서 별도로 검증한다. 이 회차의 렌더 픽셀 검사는 유효하다.
- [효과 실제 재생](reaction-playback.json): 16장 두 효과의 표시 단계와 마지막 숨김을 기록했다. 초기화 지연을 포함하므로 이 로그를 정밀 850ms 보증으로 사용하지 않는다.
- 원래 ms로 묶은 [idle](idle-lit.apng), [move](move-lit.apng) 등 10태그 APNG는 렌더 리뷰이며 위 실시간 시작 오류를 대신하지 않는다.
- native 두 창 숨김/복원·같은 렌더 텍스처 확인. 클릭 윤곽은 r004와 동일한 combined 커버리지에서 생성한다. 실제 신규 클릭 수신 시험은 하지 않았다.
- 사용자 r005 시각 확인·정식 제품 행동/스폰/입력 통합은 미완료다.
