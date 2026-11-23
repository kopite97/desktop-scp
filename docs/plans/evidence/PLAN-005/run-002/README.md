# PLAN-005 run-002 — 반사·네이티브 창 수정 확인

- [로그](execution.log), [관찰값](results.json), [창 상태](windows.json).
- 네이티브 창/숨김/복원 true, 반사 ON/OFF 차이 >0, 좌우 대응 오차 0.
- 이후 투명 Viewport 결과의 premultiplied alpha를 재차 곱하는 합성 문제가 확인됐다. 이 회차의 어두운 비교 이미지를 최종 색상 기준으로 사용하지 않는다. 최종 r002 색상 확인은 run-004다.
