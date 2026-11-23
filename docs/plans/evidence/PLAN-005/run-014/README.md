# PLAN-005 run-014 — 실제 시간 재생 최종 확인

- [PLAN-005](../../../PLAN-005-scp-999.md), Scp999 r005/Scp999Reaction r001, Windows Godot 4.7.2 .NET Compatibility.
- 실행: `./scripts/godot.ps1 -Action LightingProbe -Revision r005 -EvidenceDirectory docs/plans/evidence/PLAN-005/run-014 -LogFile docs/plans/evidence/PLAN-005/run-014/execution.log -PlaybackOnly -Preview`.
- 진단 시계를 생성/컨텍스트 시작 시점에 시작하고 frame/visible 상태 변경 직후 시간 기록을 남긴다. ProcessFrame 신호 관측의 한 프레임 지연을 재생 시간과 혼동하지 않는다.
- 전체 조명/알파/1배·2배 행렬은 run-012, 비용은 run-013을 참조한다. 이번 회차는 새 좌측/낮 렌더와 6조건 실제 재생·효과 검사다.
- [실행 로그](execution.log)에 LIGHTING_CAPTURE_COMPLETE 확인. Preview 창은 사용자 검토용으로 열어 두었다. [시험 범위](playback-scope.json), [창 정보](windows.json).
- [실제 재생 분석](playback-review.json): 50프레임·10태그, 6개 광원/반전 조건, 실제 전환 486개. 최대 프레임 시간 오차 16.757ms, p95 13.172ms. 60Hz 표시/스케줄링 오차를 포함하며 엄밀한 16.667ms 이내 보증은 아니다. run-012의 첫 프레임 74.705ms 단축은 재현되지 않았다.
- [효과 시간](reaction-playback.json): 원본 8단계 합 850ms, 마지막 프레임은 완전 투명. 숨김 전환 850.8135ms, 첫 전환 84.1708ms(논리 70ms). 표시 프레임 단위 지연을 기록했다. 진단 반복 간격 1200ms는 제품 스폰 정책이 아니다.
- [효과 렌더 검사](reaction-results.json): 16프레임×4조명/주변색 조건, 색·알파 오류 0, 끝 프레임 유효 픽셀 0. 저알파 RGB 역변환 최대 차이 .006128은 8bit premultiplied readback의 한 단계/alpha 허용 범위이며 완전 무오차 RGB라고 보고하지 않는다.
- [idle](idle-lit.apng), [move](move-lit.apng) 및 나머지 8태그 APNG는 이번 실행의 실제 Godot 렌더 PNG와 원본 ms로 묶은 자료다. 화면 녹화나 실제 전환 시간 측정 대체물이 아니다.
- 전체 렌더 행렬은 [run-012](../run-012/README.md), 비용 6쌍은 [run-013](../run-013/README.md)의 결과를 사용한다. 최종 코드 C# 빌드 경고/오류 0.
- 본체 native overlay와 진단 메인 창의 반응 효과를 확인했다. 효과는 메인 창 진단 표시이며 제품 native VFX 창·입력·스폰 상한·물리 상태 전환을 검증한 것은 아니다. 본체 r005와 효과 r001의 사용자 최종 시각 확인은 별도다.
