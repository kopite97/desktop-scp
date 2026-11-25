# SCP-999 밝기 비교 — 2026-11-24

- 실제 Godot 4.7.2 Compatibility, LG GSM5BDE, Scp999Window 두 개의 같은 idle 프레임/nearest2배 비교.
- 실행: `./scripts/godot.ps1 -Action Scp999Color -MonitorId GSM5BDE -EvidenceDirectory docs/plans/evidence/PLAN-005/run-029 -LogFile docs/plans/evidence/PLAN-005/run-029/execution.log`.
- [왼쪽 기존 / 오른쪽 수정](comparison.png), [기존 투명 렌더](before.png), [수정 투명 렌더](after.png), [수치](results.json).
- 주변광 .42→.9, 주광 에너지 .72→.5, 주광색 (1,.95,.88)→(1,.98,.94). 원본 r005 PNG·Aseprite 및 외곽선 Unshaded 경로 그대로.
- 비교 대상 몸체4296픽셀의 가중 RGB 밝기 .383951→.542184, +41.21%. 알파 오류0, 외곽선 RGB 오류0. 실제 광도계 수치나 전체 프레임 평균이 아니다.
- PASS, 로그 오류 없이 종료0. run-027의 최초 동일 비교 후 client 영역만 캡처하도록 수정해 run-029를 최종 화면으로 사용한다. run-027의 불필요한 창 바깥 비교 캡처만 제거했고 원본 before/after·수치·로그는 보존했다.
- 사용자 요청의 밝기 수정이며 새로운 아트 리비전 제작이나 사용자 시각 승인 기록은 아니다.
