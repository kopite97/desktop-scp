# PLAN-005 run-007 — r003 조명·반복 재생 최종 확인

- 2026-11-24. Godot 4.7.2 .NET / Windows / Compatibility / NVIDIA RTX 5070. [PLAN-005](../../../PLAN-005-scp-999.md), [r003 제출](../../../../../Art/assets/characters/Scp999/revisions/r003/README.md).
- 실행: `./scripts/godot.ps1 -Action LightingProbe -Revision r003 -EvidenceDirectory docs/plans/evidence/PLAN-005/run-007 -LogFile docs/plans/evidence/PLAN-005/run-007/execution.log -Preview`.
- [로그](execution.log), [렌더/환경](results.json), [프레임 변화](transitions.json), [실제 재생 시간](playback-review.json), [창 상태](windows.json).

## 결과와 해석

- idle 6장 1040ms / move 6장 740ms. 12장 × 좌/우/위 × 낮/밤 × 정/반전 × 3모드 = 432조건과 원래 시안 12장을 실제 엔진에서 렌더했다.
- 알파 오류 0, 좌우 대응 평균 RGB 오차 0. 반사 ON/OFF 차이가 있고 반사·밝은 부분이 광원 방향을 따른다.
- [모드 비교](mode-comparison.png), [낮](lighting-day.png), [밤](lighting-night.png), [세 배경](backgrounds.png). 행/열 읽는 법은 run-004와 같다.
- 좌/우/위·정/반전 총 6조건에서 각 8초 반복 재생. 전환 318개, 최대 시간 오차 13.413ms·p95 13.330ms로 60Hz 한 프레임 이내다. 실제 시간 기록과 원래 duration을 대조했다.
- 시작 시 직전 캡처/분석 프레임 delta를 버려 run-005에서 발견한 첫 프레임 단축을 수정했다.
- [idle 재생](idle-lit.apng), [move 재생](move-lit.apng)은 실제 Godot 렌더 PNG의 IDAT를 그대로 묶고 원래 ms를 넣은 반복 APNG다. 화면 녹화는 아니며 실시간 실행은 playback-timing.json이 증명한다.
- 프레임 간 평균 휘도 차이는 idle .0098~.0354, move .0233~.0470. 이 값에는 윤곽 이동·눈 깜빡임이 포함되므로 반사 떨림의 자동 합격 기준으로 쓰지 않는다. 렌더 프레임과 실제 창의 연속 상태를 함께 확인했고 루프 경계의 추가 반사 섬광은 관찰되지 않았다.
- 실제 native 입력/숨김은 [run-006](../run-006/README.md), 윤곽 재사용 후 최종 애니메이션 비용은 [run-009](../run-009/README.md)에 연결한다. run-008은 할당 문제를 찾은 중간 기록이다.

## 적용 한계와 다음 단계

- CanvasTexture·주광원 하나·주변 밝기, 2배 실제 표시 및 4배 진단 확대 기준이다. 1배 원본 검사는 Art 자료이며 Godot 1배 실시간 표시와 모든 모니터/DPI 조합을 이번 시험에서 전수 검사하지 않았다.
- 부드러운 기본 조명을 사용했다. 단계화 셰이더·접지/투사 그림자·실제 로컬 시간·개별 창마다 다른 광원은 구현하지 않았다.
- 이 프로토타입은 같은 SubViewport 결과를 두 창에 공유한다. 다른 동작을 하는 개체 다수의 성능 보증은 아니다.
- r003은 기술 검증 후보이며 최종 사용자 시각 승인은 없다. 나머지 동작 38장의 맵·전체 게임 상태 연결은 다음 작업이다. PLAN-005/ART-001은 active 유지.
