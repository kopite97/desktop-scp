# PLAN-006 완료 후 LG 제품 실행

- 2026-11-24 `./scripts/godot.ps1 -Action Run -MonitorId GSM5BDE -LogFile docs/plans/evidence/PLAN-006/run-027/live-preview.log` 실행.
- [로그](live-preview.log)의 SCP999_STARTED/SCP049_STARTED/SITE_DESKTOP_READY 확인, 마감 시 오류 없음. 사용자 확인을 위해 실행 상태로 남겼다.
- 실제 제품 Main에 SCP-999와 임시 표현의 SCP-049/가짜 데이터/좀비가 함께 표시된다. Spawn dummy data는 내부 오브젝트만 생성하며 왼쪽 클릭으로 데이터/좀비를 회수하고 오른쪽 드래그로 배치할 수 있다.
- Hide overlays는 두 개체/데이터/좀비를 함께 숨기고 시간을 멈춘다. Exit는 모든 게임 창·입력 구독·상태를 정리한다.
- 같은 프로젝트의 미리보기가 실행 중이면 native 진단을 거부하는 [사전 검사](../run-026/preview-guard.log)도 확인했다. 이 검사는 새 진단을 실행하지 않았다.
