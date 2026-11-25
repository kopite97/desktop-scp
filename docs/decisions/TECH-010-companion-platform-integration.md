---
id: TECH-010
status: accepted
title: SCP-999 공통 물리 위치와 지지면 부착 효과
updated: 2026-11-24
---

# TECH-010 SCP-999 공통 물리 위치와 지지면 부착 효과

- 결정일/위임: 2026-11-24 사용자 PLAN-005 마무리와 밝은 색감 요청.
- [PLAN-005](../plans/PLAN-005-scp-999.md), [TECH-008](TECH-008-companion-state-and-input.md), [TECH-009](TECH-009-window-platform-physics.md).
- TECH-008의 행동/입력/효과 소유는 유지한다. 임시 바닥 위치 계산·ApplyMotion 계약만 공통 물리 어댑터로 대체한다. TECH-009의 solver는 변경하지 않는다.

## 문제와 제약

행동과 창 물리가 서로 다른 위치를 소유하면 표시·입력·효과가 어긋난다. 승인된 원본의 알파/윤곽을 보존하면서 어두운 제품 색감을 개선해야 한다.

## 결정

- [Scp999State](../../GodotProject/Scripts/Companions/Scp999State.cs)의 Physics가 PlatformWorld를 소유하고 Foot/Position을 제공한다. AI와 입력은 Walk/BeginDrag/EndDrag로 요청하며 실제 충돌 상태를 애니메이션 태그로 변환한다. 별도 중력·충돌 상태를 복제하지 않는다.
- 원본 접점(32,56)×2, 보수적 충돌 폭128·높이112. 일반 진단 마커48px의 폭을 재사용하면 넓게 변형된 몸체가 창 가장자리 밖에서 지지될 수 있어 전체 셀 폭을 사용한다. 좁은 ledge 접근은 제한되는 선택이다.
- land/balance_recover의 계약 재생 시간을 보존한다. 지지 소실/공중/균형은 쓰다듬기·타이핑보다 우선하며 중력은 계속 진행한다. 드래그 취소/숨김은 현재 위치에서 멈추고 복원 후 중력을 진행한다.
- 점액은 지지면 ID와 생성 당시 창 원점을 저장한다. 몸체를 따라다니지 않으며 같은 창의 변위를 따른다. 소실/가림/최소화/종료/큰 순간 이동 시 제거한다. 바닥 자국은 고정한다.
- [컨트롤러](../../GodotProject/Scripts/Overlay/Scp999Controller.cs)는 창50ms/모니터500ms 관찰을 공유한다. 지정 화면은 시작 시 정확히 일치해야 하며 실행 중 분리되면 남은 화면으로 회복한다. 실제 분리 장비 시험은 하지 않았다.
- 표시 후 NOACTIVATE로 본체·앞 반응·뒤 점액의 순서를 복원한다. 숨겨진 Godot Window는 유효한 native handle이 없으므로 표시 전에 조작하지 않는다.
- 색감은 승인된 r005 픽셀 수정 대신 [Scp999Window](../../GodotProject/Scripts/Overlay/Scp999Window.cs)의 고정 조명을 밝힌다. 주변광 .9/주광 .5, 외곽선과 VFX는 계속 Unshaded. 리비전/승인/알파/실루엣 계약을 보존한다.

## 대안과 선택 이유

- 기존 바닥 스냅과 모의 ApplyMotion을 유지하면 실제 창 도약/낙하와 테스트가 다른 상태를 사용한다. 공통 solver의 실제 상태를 읽는 어댑터로 통일했다.
- 점액을 화면 절대 좌표에 고정하면 이동 창에서 공중에 남으므로 지지면 상대 좌표를 저장한다. 본체에 부모 연결하면 지나간 자국도 따라 움직여 채택하지 않았다.
- 승인 PNG를 재색칠하는 대신 제품 조명의 어두운 주변광을 수정해 원본 계약과 제작 승인 이력을 유지했다.

## 영향과 검증

- [LG 실제 제품105개 검사](../plans/evidence/PLAN-005/run-035/README.md), [공통 규칙66개](../plans/evidence/PLAN-005/run-037/README.md), [실제 색 비교](../plans/evidence/PLAN-005/run-029/README.md).
- 몸체 idle 평균 렌더 밝기 +41.21%, 알파/외곽선 오류0, 제출 PNG/contract10개 동일. 이 수치는 전체 프레임이나 광도계의 밝기 측정이 아니다.
- 사용자 창을 이동하지 않고 소유 시험 창과 별도 입력 수신기만 사용했다. CUA 없이 Godot/Win32 하네스로 검사했다.
- DWM 사각 근사·50ms 사이 변화·혼합 DPI 실장비·전체 비용 한계는 기존 기반과 같다. 임시 점액 시각 승인은 [PLAN-010](../plans/PLAN-010-scp-999-trail-integration.md) 정식 적용 선행이다.
