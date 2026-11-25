---
id: PLAN-010
type: art
status: completed
title: SCP-999 점액 승인본 정식 적용
updated: 2026-11-24
---

# PLAN-010 SCP-999 점액 승인본 정식 적용

## 목표와 범위

- 완료된 [PLAN-005](PLAN-005-scp-999.md)의 미승인 임시 Scp999Trail r001을 사용자 승인된 제출 리비전으로 정식 적용한다.
- [ART-003](../../Art/docs/plans/ART-003-scp-999-trail.md)이 원본 제작/수정/시각 검수를 담당한다. 이 PLAN은 Godot PNG/contract 참조·재생·합성·입력/수명 통합을 담당한다.
- development 완료 후 남은 정식 아트 적용을 분리하라는 PLAN-005 수령 계약에 따라 후보로 작성했다. 2026-11-24 사용자가 “점액 아트도 승인할게. Plan-010을 진행해서 정식 적용까지 마무리해줘.”로 r001 승인과 정식 적용 실행을 지시했다. r001이 그대로 승인되어도 적용 상태와 증거를 갱신한다.

## 기준 문서와 선행 조건

- ART-003의 명시적 사용자 시각 승인과 [handoff](../../Art/assets/vfx/Scp999Trail/handoff.md)에 승인 리비전 고정.
- 사용자 실행 승인 뒤 proposed → active. 승인본 파일/태그/피벗/알파/시간을 [기존 제품 계약](evidence/PLAN-005/run-035/art-integration.md)과 비교한다.
- [PLAN 관리](plan.md), [검증](../agents/verification.md), [Art 전달](../../Art/docs/agents/handoff.md), [조명 계약](../../Art/docs/agents/lighting.md).

## 작업 목록

- [x] 승인 리비전과 원본/PNG/contract·사용자 확인 근거를 대조한다.
- [x] 필요한 PNG/contract만 제품에 적용하고 SHA256·리비전·태그 매핑을 기록한다.
- [x] 밝아진 본체와 점액의 색 조화, Unshaded·알파·nearest2배·뒤쪽 합성·발판 경계를 실제 Godot에서 확인한다.
- [x] 생성/수명/상한, 이동 발판 부착·가림/최소화/종료 정리, 다른 프로세스 클릭 투과를 변경 영향에 맞춰 검증한다.
- [x] Godot 정식 적용 화면에 대한 사용자 시각 확인, handoff/PLAN/목록 갱신 후 completed로 판정한다.

## 현재 상황과 다음 행동

- 2026-11-24 정식 적용 완료, active → completed. 사용자 승인 대상 r001은 이전 LG 제품 표시본과 동일하며 픽셀/재질/배율 변경 없이 승인본으로 확정했다.
- [최종 증거](evidence/PLAN-010/run-001/README.md): 실제 LG/native 입력111개 및 점액 전체18프레임 렌더 검사 통과. ART-003도 approved_revision=1/completed로 마감한다.
- 추가 시각 확인을 다시 요청하지 않는다. 현재 표현에 대한 명시적 승인을 [F-004](../../Art/assets/vfx/Scp999Trail/revisions/r001/review/feedback.md)에 연결했다.
- 다음 PLAN을 자동 활성화하지 않는다.

## 결과물 저장 위치

- 승인 PNG/contract는 기존 GodotProject/Assets/Sprites/vfx/Scp999Trail/ 파일을 그대로 사용한다. [SHA256·태그·피벗 대조](evidence/PLAN-010/run-001/asset-receipt.json).
- [run-001](evidence/PLAN-010/run-001/README.md)에 결과/로그/실제 제품 화면/18프레임 렌더를 저장했다.
- Scp999Controller 시작 로그를 r001-approved로 갱신, Scp999Smoke.Trail.cs에서 제품 렌더/조명/알파 검사를 추가했다.
- [handoff](../../Art/assets/vfx/Scp999Trail/handoff.md)·리비전 피드백·ART/PLAN 목록 갱신.

## 완료 기준과 검증 방법

- 승인 제출과 런타임 파일의 SHA256·태그/시간/피벗 대조.
- 실제 Godot 렌더와 소유 창/입력 수신기에서 작업 목록의 표현·수명·부착·클릭 투과 검증.
- 정식 적용 화면의 사용자 시각 확인 근거를 기록해야 완료한다. 제작 승인만으로 제품 검수까지 완료하지 않는다.

## 검증 결과

- [run-001](evidence/PLAN-010/run-001/README.md): native+실제 입력111개 PASS, 로그 오류0/종료0. Build 경고/오류0, Import 종료0.
- 18프레임의 태그/유지 프레임 선택·nearest2배 원본 알파·주변광/점광원 불변성 오류0. 실제 창 이동/최소화/가림/종료 부착 처리와 별도 프로세스 클릭 투과 통과.
- 승인 source/제출 source 동일, PNG/contract SHA256 동일. 원본을 바꾸지 않아 리비전을 올리지 않았다.
- 문서 검사 PASS(개발/ART active 모두0), `git diff --check` PASS. [LG 정식 실행](evidence/PLAN-010/run-002/README.md)의 승인본 시작 로그 확인 후 열어 두었다. 전체 비용·혼합 DPI 하드웨어·배포는 이번 미측정 범위다.

## 관련 결정과 상태 전환

- 2026-11-24 proposed 생성 당시 실행 승인 없음. 이후 “점액 아트도 승인할게. Plan-010을 진행해서 정식 적용까지 마무리해줘.”로 proposed → active, 승인본 대조/필수 실제 검증 후 completed.
- [TECH-010](../decisions/TECH-010-companion-platform-integration.md)의 지지면 부착·기존 임시 표현 계약을 보존한다.
