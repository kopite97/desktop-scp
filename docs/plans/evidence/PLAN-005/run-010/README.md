# PLAN-005 run-010 — r004 전체 동작 조명 검증

- 2026-11-24, [PLAN-005](../../../PLAN-005-scp-999.md), [r004 제출](../../../../../Art/assets/characters/Scp999/revisions/r004/README.md).
- Godot 4.7.2 .NET / Windows / Compatibility OpenGL / NVIDIA RTX 5070.
- 실행: `./scripts/godot.ps1 -Action LightingProbe -Revision r004 -EvidenceDirectory docs/plans/evidence/PLAN-005/run-010 -LogFile docs/plans/evidence/PLAN-005/run-010/execution.log -Preview`.
- r002·r003 관문 통과 뒤 50프레임·10태그로 확장했다. 진단기는 JSON 태그 범위/loop/ms를 읽어 반복 태그는 두 번, 일회 태그는 한 번 재생하고 다음 태그로 넘어간다. 제품 상태 전환 시스템은 아니다.
- [로그](execution.log), [렌더/환경/수치 검사](results.json), [프레임 변화](transitions.json), [실제 재생 시간 분석](playback-review.json), [원시 시간 기록](playback-timing.json), [창 상태](windows.json).
- 실제 입력은 run-006, 12프레임 반복 상태 비용은 run-009에서 확인했다. 이번 회차는 전체 자세의 조명·변형 대응 검사이며 새로운 성능 수치를 측정하지 않는다.

## 결과

- 50프레임 × 좌/우/위 광원 × 낮/밤 × 정/반전 × 기본색/노멀/노멀+반사 = 1800조건과 r001 비교 50장을 실제 Godot에서 렌더했다.
- 알파 오류 0, 좌우 대응 평균 RGB 오차 0. 반사 ON/OFF 차이(양의 평균 .0013116)가 있고 50자세 모두 광원을 오른쪽으로 옮기면 밝은 영역의 X 중심이 오른쪽으로 이동한다. 이 수치는 시각 품질 점수를 대신하지 않는다.
- [동작 대표 프레임](actions-overview.png), [낮](lighting-day.png), [밤](lighting-night.png), [밝음/어두움/무늬 배경](backgrounds.png). 동작 대표 그림은 위 행 idle/move/typing_react/pet/pet_recover, 아래 행 takeoff/fall/land/balance/balance_recover 순이다.
- 태그별 모드 그림은 위부터 r001 고정 명암 / 중립 기본색 / 노멀 조명 / 노멀+반사다. [idle](mode-idle.png), [move](mode-move.png), [타이핑](mode-typing_react.png), [쓰다듬기](mode-pet.png), [회복](mode-pet_recover.png), [도약](mode-takeoff.png), [낙하](mode-fall.png), [착지](mode-land.png), [균형](mode-balance.png), [균형 회복](mode-balance_recover.png).
- 눌림·늘어남·팔·꼬리·기울임에 맞춰 밝은 면이 변하며 윤곽 밖 반사나 프레임 경계의 별도 섬광은 관찰되지 않았다. 착지의 급격한 실루엣 변화와 눈 깜빡임은 원래 애니메이션이다. 픽셀별 조명은 부드러운 기본 모델이며 단계화 셰이더는 적용하지 않았다.
- 좌/우/위 × 정/반전 6조건에서 10태그를 실제 ms로 재생했다. 전환 486개, 최대 시간 오차 13.781ms·p95 13.235ms로 60Hz 한 프레임 이내다. idle 1040ms, move 740ms를 유지한다. 밤의 모든 자세는 렌더 검사에 포함했고 실제 시간 기록은 낮 조건이다.

## 재생 자료

- 반복: [idle](idle-lit.apng), [move](move-lit.apng), [pet](pet-lit.apng), [fall](fall-lit.apng), [balance](balance-lit.apng).
- 일회: [typing_react](typing_react-lit.apng), [pet_recover](pet_recover-lit.apng), [takeoff](takeoff-lit.apng), [land](land-lit.apng), [balance_recover](balance_recover-lit.apng).
- APNG는 실제 Godot 렌더 PNG의 IDAT를 유지하고 원래 프레임 ms로 묶은 자료다. 좌측 주광원·낮·정방향이며 화면 녹화가 아니다. 실시간 재생은 별도 시간 로그로 확인한다.

## 범위와 남은 확인

- [실제 native 입력/숨김 검사](../run-006/README.md), [idle/move 비용 비교](../run-009/README.md)를 선행 증거로 연결한다. 비용 6쌍 모두 예산 통과했으나 전체 50프레임이나 독립적으로 움직이는 여러 개체의 성능을 새로 측정한 것은 아니다.
- 실제 오버레이 nearest 2배·진단 확대 4배, 같은 SubViewport 결과를 두 창에 공유한다. Godot 1배 표시·모든 DPI·외부 앱 조합은 전수 검사하지 않았다.
- 자동 태그 순환은 진단용이다. 실제 입력/물리 이벤트에 따른 태그 간 전환, 행동·스트레스·그림자·실제 시간 연동·제품 통합은 미구현이다.
- ART-001 revision=4, approved_revision=0. 최종 사용자 시각 확인 대기이며 ART-001/PLAN-005는 active 유지한다. ART-002/003은 proposed다.
