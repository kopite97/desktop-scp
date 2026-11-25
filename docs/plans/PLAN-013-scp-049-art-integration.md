---
id: PLAN-013
type: art
status: completed
title: 승인된 SCP-049 r001 정식 적용
updated: 2026-11-25
---

# PLAN-013 승인된 SCP-049 r001 정식 적용

## 목표와 범위

- 사용자 2026-11-25 “승인한 ART-004 역병의사 아트를 정식 적용해줘. 이번에도 LG 모니터에서 작업해줘.” 요청.
- 포함: 승인 r001 PNG/계약 수령, 임시 본체 교체, 10태그/42프레임 상태 연결, 반전·피벗·알파 입력, 실제 LG 검증·사용자 제품 화면 확인.
- 제외: 승인 원본 수정, 좀비 팔다리/변이 효과 신규 제작, SCP-173 아트, 배포용 내보내기.

## 기준 문서와 선행 조건

- [ART-004](../../Art/docs/plans/ART-004-scp-049-character.md), [명세](../../Art/assets/characters/Scp049/spec.md), [handoff](../../Art/assets/characters/Scp049/handoff.md), [r001](../../Art/assets/characters/Scp049/revisions/r001/README.md).
- [로드맵](../game-design/roadmap.md), 완료 PLAN-006/011/012/008의 상태·창 순서·아이콘·봉쇄를 유지한다.
- [PLAN 관리](plan.md), [아키텍처](../agents/architecture.md), [검증](../agents/verification.md), [Windows 연동](../agents/windows-integration.md), [조명 계약](../../Art/docs/agents/lighting.md).
- nearest2배, 64×64셀·7열6행, 피벗(32,56), Unshaded/Mix/흰 부모. 치료 시간·예약·물리는 PlagueState가 소유한다. 표현 태그/시간은 같은 상태에서 갱신하며 창 Publish는 시간을 진행하지 않는다.

## 결과물 저장 위치

기준 C:/Dev/scp_desktop. 아래 제품/검증 파일은 생성되어 실제 적용했다.

- 복사: `GodotProject/Assets/Sprites/characters/Scp049/Scp049.png`, `Scp049.contract.json`.
- 생성: `GodotProject/Scripts/Simulation/DoctorAnimation.cs`, `Overlay/DoctorSprite.cs`, `Diagnostics/DoctorArtSmoke.cs`, 대응 진단 scene.
- 변경: PlagueState/PlagueController/PlagueActorWindow, scripts/godot.ps1, 전달/기획 기록.
- 증거: `docs/plans/evidence/PLAN-013/run-001/README.md`, 회차별 results.json/execution.log/필요한 PNG. 기존 회차 보존.

## 작업 목록

- [x] 승인 리비전·상태 매핑·파일 수령
- [x] 제품 적용·상태/입력 연결
- [x] 실제 태그 재생·반전·낮/밤 불변·알파·LG 입력·봉쇄/정리 검증
- [x] 비용 기록·사용자 제품 화면 확인·handoff/PLAN 마감

## 완료 기준과 검증 방법

| 기준 | 검증 | 증거 |
| --- | --- | --- |
| 승인 파일과 동일한 정식 본체 | 해시, 실제 viewport RGBA/nearest/피벗/반전 | 회차 결과·PNG |
| 상태와 전체 태그 연결 | 실제 치료/취소·물리·한 번/반복 재생, 고부하 치료 속도 | 제품 상태 시나리오 |
| 투명 여백 투과·드래그·겹침 유지 | 별도 소유 수신기 실제 입력, 기존 순서 소유자 | 입력 로그 |
| 조명 제외 계약 | CanvasModulate 낮/밤 및 Light2D 하에서 동일 픽셀, 중립 부모 | 렌더 비교 |
| 적용 완료 | LG 실제 화면 사용자 확인, 숨김/종료·회귀·문서 검사 | 확인 기록 |

## 현재 상황과 다음 행동

- completed: 승인된 r001 본체를 정식 적용하고 LG 기술·입력 검증과 사용자 “정식 적용 화면 승인”을 확인했다.
- 다음: LG 최신 제품을 실행 상태로 유지한다. 좀비 부착 요소/변이 효과·173 정식 아트는 별도 승인 범위다.
- 비용: 1개128×128 본체, 사전 계산 알파 마스크, 갱신당 픽셀1회 판정. 같은 창 수/조건의 짧은 렌더 비용을 기록하며 장기 성능은 별도다.

## 검증 결과

- [실행 증거·사용자 확인](evidence/PLAN-013/run-001/README.md). Build 경고/오류0, headless68개·LG 실제 입력 포함166개 PASS. 42프레임×양방향 색/알파/입력 마스크 불일치0.
- 기존049 43개·173 18개·시스템/봉쇄 LG28개 회귀 PASS. 실제 광원/주변색 하에서 Unshaded 원본색 유지.
- GPU·장기 성능·실제 혼합DPI/탈착·배포 미검증. 짧은 조건별 렌더 비용은 증거 기록에 한정한다.
- 2026-11-25 사용자가 LG 제품의 크기·색감·이동·치료를 확인하고 “정식 적용 화면 승인”으로 응답했다.

## 관련 결정과 상태 전환

- 기존 TECH-011/012/013/014 상태·입력·창 순서를 재사용한다.
- 2026-11-25 proposed 생성. 위 사용자 지시가 본 PLAN 범위의 실행 승인이다.

- 2026-11-25 proposed → active: 사용자 정식 적용·LG 작업 요청에 따라 승인 r001 통합을 시작한다.

- 2026-11-25 active → completed: 정식 본체/전체 동작/알파 입력 적용과 필수 기술 검증, 실제 제품 화면 승인을 충족했다.
