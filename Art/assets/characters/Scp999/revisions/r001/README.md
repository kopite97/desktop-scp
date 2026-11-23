# Scp999 r001 제출 기록

- 제출일: 2026-11-23. 제작 [ART-001](../../../../../docs/plans/ART-001-scp-999-character.md), 요청 PLAN-005-SPR-001.
- 이번 변경: 최초 본체·점액 팔·바닥 끌림 이동·행동 50프레임, 10태그.
- 사용자 최종 시각 확인: **미확인**, 승인 리비전 0. 제작 후보 제출이며 제품 적용본이 아니다.
- 편집 원본: [현재 source](../../source/Scp999.aseprite), 제출 보존본: [r001 source](source/Scp999.aseprite).
- 출력: [PNG](export/Scp999.png), [Aseprite JSON](export/Scp999.json), [좌표/재생 계약](export/Scp999.contract.json).
- 도구: Aseprite 1.3.18.6/API 41, MCP Live. 생성/편집·팔레트·출력은 Aseprite에서 수행했다.
- 시트: RGBA/sRGB, 512×448, 64×64 고정 셀, 8열×7행의 앞 50셀, 행 우선, 간격/패딩 0, 트리밍/회전/중복 병합 없음.
- 레이어: Body·Outline·Face·Highlights, normal·opacity 255. 12칸 팔레트(투명 포함).
- 피벗·바닥 접점 (32,56), 좌상단 원점·X 오른쪽/Y 아래. 각 프레임 효과 연결점은 contract JSON 참조.
- 원본 1px 다크 윤곽 #432227FF, 몸체 기본 #F78A21D3. 반투명·명암·재질 반사를 독립 레이어로 보존.
- 제출 당시 규격을 보존한다. 최신 적용 상태는 [handoff](../../handoff.md)를 참조한다.

## 태그와 리뷰

| 태그 | Aseprite 프레임 | JSON 인덱스 | ms 순서 | 반복 |
| --- | --- | --- | --- | --- |
| idle | 1–6 | 0–5 | 220, 180, 100, 160, 180, 200 | 반복 |
| move | 7–12 | 6–11 | 120, 120, 100, 100, 140, 160 | 반복 |
| typing_react | 13–19 | 12–18 | 90, 80, 100, 90, 90, 110, 140 | 일회 |
| pet | 20–25 | 19–24 | 130, 140, 140, 140, 130, 140 | 반복 |
| pet_recover | 26–28 | 25–27 | 100, 110, 170 | 일회 |
| takeoff | 29–32 | 28–31 | 90, 110, 70, 80 | 일회 |
| fall | 33–36 | 32–35 | 140, 140, 140, 140 | 반복 |
| land | 37–41 | 36–40 | 60, 80, 100, 100, 160 | 일회 |
| balance | 42–47 | 41–46 | 120, 140, 120, 140, 120, 150 | 반복 |
| balance_recover | 48–50 | 47–49 | 100, 120, 160 | 일회 |

- [전체 프레임 접촉표](review/contact-sheet.png): 10열×5행, 프레임 1–50 행 우선, nearest 2배.
- [밝음/어두움/무늬 배경](review/background-comparison.png): 열은 배경, 행은 idle·move·pet 2배와 idle 1배.
- [좌우 비교](review/direction-comparison.png): move 프레임9, 우측 원본/좌측 반전.
- [전체 APNG](review/playback.apng): 저장된 순서의 50프레임, 정확한 프레임 ms. 게임의 상태 전환 시나리오가 아니다.
- 개별 재생: [idle](review/idle.apng), [move](review/move.apng), [typing](review/typing_react.apng), [pet](review/pet.apng), [pet 회복](review/pet_recover.apng).
- 물리 표현: [takeoff](review/takeoff.apng), [fall](review/fall.apng), [land](review/land.apng), [balance](review/balance.apng), [balance 회복](review/balance_recover.apng).
- APNG는 Aseprite 출력의 픽셀을 변경하지 않고 Node로 nearest 확대·알파 보존·프레임 시간 포장했다. 제품 파일은 원본 크기 PNG다.

## 검수

| 확인 | 실제 결과 | 증거 |
| --- | --- | --- |
| 원본/PNG 대응 | 50프레임 전체 픽셀 불일치 0 | [픽셀 검사](../../../../../docs/plans/evidence/ART-001/run-001/pixel-audit.json) |
| 윤곽·투명 | 모든 프레임에 반투명 픽셀 존재, 실루엣 경계의 불투명 윤곽 누락 0 | 같은 픽셀 검사 |
| 클리핑·이동 접지 | 가장자리 접촉 0, move 6프레임 모두 하단 y=55 | [결과](../../../../../docs/plans/evidence/ART-001/run-001/results.json) |
| 태그/시간/보존본 | 10태그 범위·프레임별 ms 일치, 편집/보존 source 바이트 동일 | 같은 결과·실행 로그 |
| 시각 검토 | 접촉표·세 배경·좌우 비교 검토, Aseprite Live에서 재생 상태 확인 | 리뷰 파일·[실행 기록](../../../../../docs/plans/evidence/ART-001/run-001/README.md) |
| 최종 사용자 확인 | 미확인 | [피드백](review/feedback.md) |
| Godot/Windows 입력 | 미실행 | [전달 기록](../../handoff.md) |

팔·끌림 반영은 요청 이행이며 최종 외형/재생 승인으로 처리하지 않았다.
