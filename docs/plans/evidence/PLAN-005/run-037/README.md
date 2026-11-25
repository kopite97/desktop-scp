# PLAN-005 최종 규칙·환경 검사

- 2026-11-24, 저장소 C:/Dev/scp_desktop, 기존 Godot 4.7.2 .NET/SDK8.
- Build 경고0/오류0. `Scp999Smoke -Headless` [66개 통과](results.json), [로그](execution.log).
- run-035 이후 제품 코드 변경 없이 자동 커서 도약, 실제 발판 pet 시작, 지지 소실 시 pet 중단의 공통 경로 검사를 추가했다.
- 각 액션은 `scripts/godot.ps1`, EvidenceDirectory는 이 디렉터리, LogFile은 아래 파일로 실행했다.
- `Import`: 종료0, [import.log](import.log). `Smoke -Headless`: 종료0, 환경/시작/종료 마커, [environment.log](environment.log).
- [에셋10개 SHA256](asset-hashes.json) 원본·런타임·run-025 동일. 밝기 수정은 런타임 조명뿐이다.
- 실제 Windows 입력/물리는 [run-035](../run-035/README.md), 색 비교는 [run-029](../run-029/README.md). Headless 결과를 native 결과로 세지 않는다.
