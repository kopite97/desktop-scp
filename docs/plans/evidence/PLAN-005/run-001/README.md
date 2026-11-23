# PLAN-005 run-001 — 초기 진단 실패 기록

- r002 4장, Godot 4.7.2 Compatibility/Windows/NVIDIA RTX 5070. [로그](execution.log), [관찰값](results.json).
- ForceNative를 Visible=true 상태에서 지정한 오류 2건과 specular_shininess=24에서 반사 차이 0을 확인했다. 이 실행은 합격이 아니다.
- Window 생성 시 Visible=false를 먼저 지정하고 반사 파라미터를 .45로 변경했다. 후속 run-002/003에서 연결을 확인하고 run-004에서 알파 합성까지 수정했다.
