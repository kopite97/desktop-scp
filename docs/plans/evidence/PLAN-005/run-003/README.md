# PLAN-005 run-003 — 알파 원인 확인

- [로그](execution.log)의 ALPHA_DIAGNOSTIC: 원본 RGBA (.9686,.5412,.1294,.8275), 렌더 결과 (.8,.4471,.1059,.8275).
- RGB에 alpha가 이미 곱해진 Viewport 출력임을 확인했다. 파일 원본의 알파 손상이 아니다.
- 후속 수정: 화면 TextureRect에는 PremultAlpha, PNG 증거에는 straight RGBA 복원을 한 번 적용했다. run-004에서 재검증했다.
