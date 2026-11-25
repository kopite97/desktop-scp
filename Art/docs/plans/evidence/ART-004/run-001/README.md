# ART-004 run-001 — r001 제작과 검수

- 날짜: 2026-11-24. Windows / Aseprite 1.3.18.6 / Lua API 41.
- 대상: [ART-004](../../../ART-004-scp-049-character.md), [r001 제출](../../../../../assets/characters/Scp049/revisions/r001/README.md).
- 기대: 편집 가능한 64×64 본체와 치료·이동 동작, 고정 셀 출력, 원본/출력 일치, 레퍼런스 반영, 실제 재생 검토.

## 실행 방법과 증거

- Aseprite MCP batch를 명시하고 모든 입력·출력에 C:/Dev/scp_desktop/Art 절대 경로를 사용했다. 기본 workDir가 다른 프로젝트였으므로 사용하지 않았다.
- Live 연결은 disconnected, bridge port 41234 사용 중이었다. 다른 Aseprite 인스턴스·브리지 설정을 변경하지 않고 batch 제작과 전용 GUI 미리보기를 병행했다.
- [build-scp049.lua](build-scp049.lua): Aseprite Lua API로 8개 레이어의 픽셀 클러스터·1px 외곽선·동작별 포즈를 작성했다. 참고 이미지를 축소하여 최종 아트로 사용하지 않았다.
- 대표 5프레임 [샘플](samples-4x.png)을 확인한 뒤 전체 42프레임으로 확장했다. build-samples.json/build-full.json은 생성 결과다.
- 전체 제작 호출: run_lua, backend=batch, args.full=true, `_G.art004Full=args.full; return dofile('C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-004/run-001/build-scp049.lua')`.
- [review-scp049.lua](review-scp049.lua): 작업/보존 원본과 시트를 읽어 합성 픽셀·태그·시간·셀 좌표·알파·외곽선·여백·치료 접점을 검사하고 배경·반전·겹침·GIF 리뷰를 생성했다.
- [results.json](results.json): PASS, 42프레임/10태그/8레이어/23색/33고유 합성 프레임. 나머지 자세 반복은 정지 유지·복귀로 의도했다.
- [해시](hashes.json), [실행 요약](execution.log).

## 화면 검토

- LG GSM5BDE의 좌표(-1920,1061)에 작업 전용 Aseprite를 배치했다. [창 위치](gui-window.json), [프로세스](preview-process.json).
- [show-preview.lua](show-preview.lua)의 50ms 타이머로 GUI 프레임 진행을 관찰했다. 출력은 stdout이 아닌 Aseprite Console에 표시되었으며 Computer Use 화면에서 observedFrames 1..42, pollSeconds 12.0, ui=true를 확인했다. 이 타이머를 정확한 프레임 시간 검증으로 취급하지 않는다.
- Console을 닫고 화면 배율을 줄인 뒤 Aseprite 재생 버튼으로 저장된 시간의 전체 타임라인 재생을 확인했다. 서로 다른 시점에서 프레임 4와 14 및 자세 변화를 확인했고 LG에 리뷰를 열어 두었다.
- 세 배경에서 가면과 실루엣이 구분된다. 어두운 배경에서 로브는 억제된 대비이며, 가면·망토 능선·가방 잠금쇠가 주요 읽기 지점이다. PNG 검토와 GUI 재생 관찰은 사용자 시각 승인이 아니다.
- 전체 타임라인 재생은 동작 나열용이다. 게임의 각 상태 유지·취소·치료 3초 연결과 개별 태그 반복 체감은 제품 통합 시 별도 확인한다.

## 수정한 실행 문제와 한계

- 초기 샘플 저장 뒤 닫힌 Sprite의 filename을 읽는 오류가 있었고, 저장 경로를 먼저 보존하도록 수정했다.
- 첫 전체 제작 시 MCP args의 지역 범위 때문에 샘플 모드가 실행되었다. 명시적으로 full 값을 전달한 뒤 42프레임을 다시 생성하고 검사했다.
- 도구 끝 접점 y를 실제 끝 픽셀에 맞추고 계약을 수정했다. 접점 불투명 검사 포함 최종 PASS다.
- Unshaded/Mix/중립 부모는 전달 계약이다. Godot 실제 조명·색 불변성·입력 윤곽·성능은 이번 ART 범위 밖이며 미검증이다.
- r001 사용자 시각 확인 대기. approved_revision=0, ART 상태 active. 제품 코드/임시 도형 교체는 수행하지 않았다.
