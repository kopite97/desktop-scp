# PLAN-010 Scp999Trail r001 정식 적용 — 2026-11-24

## 승인과 수령

- 사용자: “점액 아트도 승인할게. Plan-010을 진행해서 정식 적용까지 마무리해줘.” [F-004](../../../../../Art/assets/vfx/Scp999Trail/revisions/r001/review/feedback.md).
- 승인 대상은 직전 PLAN-005 완료 후 LG 제품에 표시하던 동일 r001이다. 픽셀·재질·배율·태그/시간을 바꾸지 않았으므로 이번 승인을 해당 제품 표현의 정식 적용 승인으로 연결했다. 기술 검사를 사용자 승인으로 대체한 것이 아니다.
- [수령 대조](asset-receipt.json): 작업 source와 r001 제출 source SHA256 일치. 런타임 PNG/contract는 승인 제출 export와 SHA256 일치하며 기존 임시 적용 파일에서 변경0. 불필요한 재복사/새 리비전은 만들지 않았다.
- 실제 위치: GodotProject/Assets/Sprites/vfx/Scp999Trail/Scp999Trail.png 및 Scp999Trail.contract.json. source/review를 제품에서 직접 읽지 않는다. 제출 contract의 당시 pending 문구는 과거 제출 데이터로 보존하고 현재 적용 상태는 handoff/이 증거에 기록한다.
- puddle_a=0~8, puddle_b=9~17, 32×16 셀/9열2행, 피벗(16,8), nearest2배. 생성180ms+유지1800ms+증발720ms=2700ms, 간격300ms+이동48px/상한12. Unshaded/Mix, 몸체 뒤 배치, 추가 fade 없음.

## 환경·명령

- C:/Dev/scp_desktop, Godot 4.7.2 .NET Compatibility/OpenGL, RTX5070, .NET8 SDK.
- LG FULL HD GSM5BDE, 물리 원점(-1920,1061), usable1920×1032, DPI96. [모니터](platform-monitor.json). 시스템 화면 설정 변경 없음.

```powershell
./scripts/godot.ps1 -Action Build
./scripts/godot.ps1 -Action Scp999Smoke -MonitorId GSM5BDE -NativeInput -EvidenceDirectory docs/plans/evidence/PLAN-010/run-001 -LogFile docs/plans/evidence/PLAN-010/run-001/execution.log
./scripts/godot.ps1 -Action Import -LogFile docs/plans/evidence/PLAN-010/run-001/import.log
```

## 결과

- Build 경고0/오류0. native+실제 입력 [111개 PASS](results.json), 종료0·[오류 없는 실행 로그](execution.log), Import 종료0.
- [18프레임 실제 Godot 렌더](trail-engine-frames.png), [수치](trail-render.json): frameErrors0/alphaErrors0/lightingErrors0. 각 프레임을 제품 ScpEffectWindow/FrameAt 경로에서 재생하고, 원본 alpha의 nearest2배 대응과 CanvasModulate/PointLight2D 변경 전후를 비교했다. CPU로 렌더를 흉내 낸 결과가 아니다.
- [실제 발판 합성](native-platforms.png)에서 밝은 본체와 낮은 알파의 점액, 발판 부착 경계·본체 뒤 배치를 관찰했다. 장식 캡처 외 화면은 소유한 시험 배경/창으로 제한했다.
- 생성/수명/상한, 실제 이동 창에 점액 동반, 가림/최소화/종료 시 제거, 숨김/복원/종료 정리 통과.
- 별도 소유 수신기 [down/up/키 활동 결과](receiver.json)로 몸체·빈 여백·반응·점액의 실제 입력 전달, 문지름·드래그·추적 확인. 플래그만으로 클릭 투과를 판정하지 않았다.
- 변경된 제품 코드는 시작 로그의 r001-approved 표시뿐이다. 효과 renderer/physics의 동작은 이전과 동일하다. 진단에 전체 점액 렌더 검사3개를 추가했다.

## 완료와 한계

- ART-003 r001 승인/완료, PLAN-010 정식 적용 완료. PLAN-005는 기존 completed를 유지한다.
- 이전 PLAN의 미승인/미적용 증거는 당시 사실로 보존한다. 이후 현재 상태는 본 증거와 최신 handoff를 따른다.
- 실제 혼합 DPI 장비·장기 전체 제품 비용·최대 효과 부하 성능·배포 내보내기는 이번에 측정하지 않았다.
