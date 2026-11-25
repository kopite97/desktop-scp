# PLAN-012 최종 검증

- 2026-11-25 Windows 11, Godot 4.7.2 .NET/OpenGL, .NET8, LG GSM5BDE/96DPI/음수 물리 원점.
- 실행: `./scripts/godot.ps1 -Action Build`, `-Action DesktopIconSmoke -MonitorId GSM5BDE -EvidenceDirectory docs/plans/evidence/PLAN-012/run-014 -LogFile docs/plans/evidence/PLAN-012/run-014/execution.log`.
- [결과](results.json), [로그](execution.log), [비용](cost.json), [테스트 소유 아이콘 렌더](owned-fixture-render.png).
- [run-010 실제 입력](../run-010/results.json): 투명 구멍의 별도 프로세스 down/up 수신, 불투명 클릭 회수, 오른쪽 드래그/해제 PASS. 해당 실행은 사용자의 20초 입력 검사 승인 범위에서 수행했다.
- [run-011 갱신 수명](../run-011/results.json): 실패 후 후보 제거/생존 스냅샷 유지, 봉쇄 전 늦은 결과 거부, 참조 해제 PASS. 최종 run-014에도 포함했다.
- [run-006 049 회귀](../run-006/results.json): 기존 공통 규칙 검사. 임시 외형 경로를 명시적으로 사용한 회귀이며 새 아이콘 표시 검증을 대신하지 않는다.

## 확인한 결과

- 실제 Desktop 뷰에서 고유 아이콘13개, 테스트 소유 바로가기 추가 시14개 취득. 종류는 폴더/바로가기/기타이며 사용자 파일명/경로/아이콘은 증거로 저장하지 않았다.
- 테스트용 48px 투명 구멍 아이콘의 RGB/알파, Godot 텍스처 전체 byte roundtrip, 실제 native viewport 픽셀 확인 PASS. 증거의 아이콘 그림은 테스트가 만든 것이다.
- 같은 appearance ID가 치료 후 좀비로 유지되고, 이미지가 없으면 스폰을 건너뛴다. 숨김·복귀·종료·캐시 참조 해제를 확인했다.
- worker 취소·반복 취득 후 잔류 worker0. 최종3회 취득은 약205/216/216ms. managed bytes는 약1.55MB→3.31MB로 이 짧은 측정만으로 장기 무누수를 주장하지 않는다. 작업 프로세스/카탈로그/캐시는 개수·시간 상한을 둔다.
- 2026-11-25 LG 미리보기(run-012)에서 사용자가 “현재 아이콘 복제 동작 승인”으로 응답했다. 임시 049/좀비 부착 표현임을 설명한 뒤 받은 기능/화면 확인이며 ART 승인으로 간주하지 않는다.
- 테스트 소유 `SCP-Icon-Probe-7b612f.lnk`는 생성/검사 후 삭제했다. 실제 사용자 원본을 이동·수정·삭제하지 않았다. [fixture](../run-001/fixture/target.txt)와 ICO는 재현용으로 보존한다.

## 실패 기록과 한계

- run-002: Godot 메모리 로드 assembly.Location이 비어 helper 경로 탐색 실패. 빌드 복사 경로/실행 경계를 명시해 수정.
- run-004~008: 수신기 topmost 및 Godot/Win32 입력 투과 결합을 진단. 실패를 보존했고 run-009/010에서 별도 프로세스 실제 수신으로 검증했다.
- run-013: 미리보기 console wrapper 종료를 먼저 기다리다 fixture 준비가 실행되지 않아 fixture 검사 실패. 소유 GUI 자식 종료를 확인하고 fixture를 생성한 run-014에서 통과.
- 실제 Explorer 강제 재시작은 사용자 세션에 영향이 있어 실행하지 않았다. 오류/빈 목록/늦은 결과는 같은 서비스 경계의 실패·지연 주입으로 검사했다. 실제 환경 재연결·혼합 DPI·썸네일/동기화 배지·배포·장기 비용은 완료 주장 범위에서 제외한다.
