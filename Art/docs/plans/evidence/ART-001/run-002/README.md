# ART-001 run-002 — r002 조명 샘플

- 실행일: 2026-11-23. 대상: [ART-001](../../../ART-001-scp-999-character.md), [r002 제출](../../../../../assets/characters/Scp999/revisions/r002/README.md).
- 환경: Windows, Aseprite 1.3.18.6 Live/Lua(API 41). 작업 루트 C:/Dev/scp_desktop/Art를 절대 경로로 지정했다.
- 기대 결과: 기존 형태·표정·윤곽을 보존한 4장 기본색과 동일 셀의 노멀/반사 후보, r001 원본 보존.

## 실행과 증거

1. [build-lighting-sample.lua](build-lighting-sample.lua)를 Aseprite Live에서 실행했다. r001 제출 원본의 UI 1·7·9·12를 읽어 별도 샘플 문서·제출 사본·PNG 3종을 작성했다.
2. [audit-sample.lua](audit-sample.lua)로 r001 대비 표정/윤곽/커버리지, 균일 Body, 노멀 길이/방향/알파, 스페큘러 값, 3종 출력 픽셀을 비교했다.
3. 비교 시트와 세 배경 이미지를 작성하고 직접 확인했다. Aseprite에서는 샘플 원본을 활성 문서로 열고 프레임 1을 4배 확대해 표시했다.
4. [audit-saved-files.lua](audit-saved-files.lua)로 디스크 제출 원본·PNG 규격/프레임/레이어·시간을 확인했다. SHA256으로 작업 원본/제출 사본을 대조했고 r001 제출 원본 해시는 작업 전 값과 같았다.

- 실제 결과: [results.json](results.json), 호출·검사 요약: [execution.log](execution.log).
- 모든 픽셀 불일치/데이터 오류 카운터 0. 노멀 길이 허용 오차 0.015 이내(8비트 양자화). 이것은 방향 데이터의 형식 검사이며 실제 표면 품질 검증은 아니다.
- [맵 비교](../../../../../assets/characters/Scp999/revisions/r002/review/map-comparison.png): 위부터 기존 시안/기본색/노멀/반사, 4열.
- [배경 비교](../../../../../assets/characters/Scp999/revisions/r002/review/base-backgrounds.png): idle·최대 끌림, 밝음/어두움/무늬.

## 미실행과 다음 단계

- [PLAN-009](../../../../../../docs/plans/PLAN-009-scp-999-lighting-sample.md)의 Godot 조명·반전·창/입력·비용 검증은 미승인·미실행이다.
- 전체 50프레임 보조 맵과 실제 시간 애니메이션 조명 검증은 샘플 통과 이후다. 샘플 4장 순환을 전체 move 검증으로 처리하지 않았다.
- 최종 사용자 시각 확인·제품 적용은 아직 없다. ART active, revision=2, approved_revision=0 유지.
- 검사기 코드는 바꾸지 않았으므로 검사기 자체 테스트는 반복 실행하지 않는다. 문서/상태/링크 검사는 별도 실행한다.
- `node scripts/validate-agent-docs.mjs` 통과: 개발 PLAN 9개/active 0, ART 3개/active 1, TECH 6개. 루트 MAP 역할과 승인 상태를 유지했다.
