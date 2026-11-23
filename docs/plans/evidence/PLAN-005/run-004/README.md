# PLAN-005 run-004 — r002 네 장 샘플 관문

- 실행: `./scripts/godot.ps1 -Action LightingProbe -Revision r002 -EvidenceDirectory docs/plans/evidence/PLAN-005/run-004 -LogFile docs/plans/evidence/PLAN-005/run-004/execution.log -Measure -TimeoutSeconds 180`.
- 환경: Godot 4.7.2 .NET / Windows / Compatibility OpenGL 3.3 / NVIDIA RTX 5070, 드라이버 621.01.
- 실제 [로그](execution.log), [렌더 관찰값](results.json), [창 상태](windows.json), [비용](performance.json).
- 좌/우/위 × 낮/밤 × 정/반전 × 기본색/노멀/반사, 4장으로 144조건을 렌더했다. Aseprite 합성 결과로 대체하지 않았다.
- [모드 비교](mode-comparison.png): 위부터 기존 시안/중립 기본색/노멀 조명/반사 포함. [낮](lighting-day.png)·[밤](lighting-night.png): 위 3행 좌/우/위, 아래 3행 같은 순서 반전. [세 배경](backgrounds.png): 위 낮/아래 밤.
- 광원에 따라 명암·반사 위치가 바뀌고 알파 오류 0·좌우 대응 평균 RGB 오차 0. 몸체 고정 반짝임을 다시 넣지 않았다. 두 native 창·숨김·복원 확인; 실제 클릭 증거는 run-006에서 보완했다.
- 색상 오류 수정: Viewport premultiplied RGB를 표시할 때 PremultAlpha, 증거 PNG는 straight RGBA로 한 번 복원한다. 밤 주변 RGB는 (.40,.40,.48)다. results.json의 nightAmbient=.28은 당시 하드코딩한 메타데이터 잔여 오류이며 실제 코드/이미지에는 수정값을 사용했다. run-007에서 기록도 수정했다.
- 1개/2개 표시 창, 같은 128×128 렌더 텍스처·정지 프레임·60FPS 제한, 모드별 준비 2초+측정 5초×3. CPU/GPU 렌더 p95 추가 비용 1ms 미만, working set 증가 32MiB 미만. 프레임 간격은 VSync/스케줄링을 포함하므로 16.67ms 고정값과 다르다.
- 판정: 작은 대표 샘플의 재질 연결 확인 완료, idle/move 12장 시험으로 진행 가능. 제품 통합/전체 애니메이션/사용자 시각 승인은 아니다.
- API 참고: [CanvasTexture](https://docs.godotengine.org/en/stable/classes/class_canvastexture.html), [PointLight2D](https://docs.godotengine.org/en/stable/classes/class_pointlight2d.html). 문서상 지원과 이 실행의 실제 관찰을 구분했다.
