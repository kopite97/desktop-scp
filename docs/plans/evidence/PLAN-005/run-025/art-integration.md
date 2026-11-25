# SCP-999 제품 적용 계약 — 2026-11-24

| AssetId | 적용 리비전·승인 | 런타임 위치 | 실제 사용 |
| --- | --- | --- | --- |
| Scp999 | r005·승인 | GodotProject/Assets/Sprites/characters/Scp999/ | color/normal/specular CanvasTexture + unlit outline, combined는 클릭 윤곽 생성용, contract는 태그/시간/anchor |
| Scp999Reaction | r001·승인 | GodotProject/Assets/Sprites/vfx/Scp999Reaction/ | PNG+contract, 32×32·8열×2행·heart/note, Unshaded/Mix |
| Scp999Trail | r001·시각 미승인 임시 | GodotProject/Assets/Sprites/vfx/Scp999Trail/ | PNG+contract, 32×16·9열×2행·puddle_a/b, Unshaded/Mix |

- 파일 10개는 [SHA256 대조](asset-hashes.json)로 제출 export와 동일. source/review/Aseprite native JSON은 제품에서 참조하지 않는다. 계약 JSON의 과거 engineValidation 문구를 수정하지 않고 파일 동일성을 유지한다. 현재 적용 판정은 이 문서와 각 handoff가 원본이다.
- 몸체 64×64·50열, nearest×2, 프레임별 ms, 좌우 반전은 body와 outline에 함께 적용한다. 알파 임계 .1의 프레임별 실루엣 입력 윤곽을 캐시한다.
- 본체 idle/move는 추적 대기/이동, typing_react는 타이핑 활동, pet/pet_recover는 왼쪽 쓰다듬기/해제. 오른쪽 드래그 중 idle, 놓을 때 바닥 스냅과 land를 사용한다.
- takeoff/fall/land/balance/balance_recover는 ApplyMotion 계약으로 연결했다. 실제 물리 이벤트 발생은 PLAN-004 이후이며 모의 명령 검사와 구분한다.
- 하트/음표: 간격650ms·상한4·수명850ms, 현재 프레임 reactionAnchor에 ±28px/-20px 오프셋, 생성 위치에서 위로36px 이동. 입력 폭주는 병합하고 진행 중 태그를 매번 재시작하지 않는다.
- 점액: 접지 이동 중 300ms 이상+이동48px마다, 상한12, 생성180ms+유지1800ms+증발720ms=2700ms. 생성 시 Foot(원본 trailAnchor32,56×2)에 고정한다. 반전 허용, body 뒤 native Z순서, 별도 fade 없음.
- 효과 전용 창은 MousePassthrough + Windows layered/transparent 스타일로 다른 프로세스에 클릭을 통과시킨다. 효과는 본체 클릭 윤곽에 포함하지 않는다.
- 숨김은 효과·gesture·활동 감시를 정리하고 스트레스/위치를 보존한다. 단, 진행 중 드래그는 취소/바닥 스냅 후 보존한다. 복원 시 대기 입력과 경과 시간을 몰아서 처리하지 않는다. 정상 프레임 delta 상한100ms로 긴 정지의 순간 이동도 제한한다.
- [제품 검사](README.md): 실제 렌더 캡처, native 입력 수신, 상태·시간·정리 검증. 기존 50프레임 광원/알파/색 행렬은 run-012~014의 증거를 재사용했으며 이번에 전체를 재측정하지 않았다.
- 창 발판 부착/이동/소멸 정책과 실제 중력 연결은 미완료. 점액 시각 승인도 별도 대기다.
