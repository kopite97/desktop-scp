---
id: TECH-013
status: accepted
title: Shell 아이콘 작업 프로세스와 세션 외형 캐시
updated: 2026-11-25
---

# TECH-013 Shell 아이콘 작업 프로세스와 세션 외형 캐시

- 근거: 사용자 실제 아이콘 복제 방향 및 PLAN-012 → 007 → 008 실행 승인. [PLAN-012](../plans/PLAN-012-desktop-icon-clones.md), [제품 규칙](../game-design/desktop-icon-clones.md).
- 기존 [TECH-011](TECH-011-plague-objects-and-reservations.md)의 객체/예약 소유, [TECH-012](TECH-012-overlay-stacking.md)의 창 순서를 확장하며 대체하지 않는다.

## 문제와 제약

임의 Shell 아이콘 확장은 느리거나 응답하지 않을 수 있다. 바탕화면 경로 열거만으로 실제 Desktop 뷰와 가상 항목을 정확히 대응할 수 없고, 투명 구멍을 단일 사각 클릭 윤곽으로 처리할 수 없다.

## 결정

- [DesktopIconWorker](../../tools/DesktopIconWorker/Program.cs)는 독립 STA 프로세스에서 ShellWindows의 Desktop 뷰, IFolderView.Items/IShellItemArray, IShellItemImageFactory ICONONLY를 사용한다. 최대64항목/32고유이미지, 48px 그림만 내보낸다. 파일명·경로·실제 파일 내용은 출력하지 않는다.
- [DesktopIconProvider](../../GodotProject/Scripts/Platform/Windows/DesktopIconProvider.cs)는 10초 한계·취소·JSON 크기/픽셀 해시 검증을 담당한다. [WorkerJob](../../GodotProject/Scripts/Platform/Windows/WorkerJob.cs)의 kill-on-close Job으로 부모 종료 시 작업 프로세스도 정리한다.
- Main 소유 [DesktopIconAppearance](../../GodotProject/Scripts/Overlay/DesktopIconAppearance.cs)는 30초 갱신, 이미지 해시 기반 중복 제거, 카탈로그+생존 객체 참조 캐시를 소유한다. 게임 상태에는 appearance ID만 연결하며 치료 후 유지한다. HBITMAP 해제는 worker, Godot 텍스처 해제는 Main의 서비스가 담당한다.
- 취득 오류는 새 후보를 비우고 기존 객체 스냅샷을 보존한다. 세대 번호로 봉쇄 전 결과를 버리고 새 취득으로 복구한다. 캐시 객체 수는 후보32+생존 객체 상한으로 제한된다.
- 아이콘은 실제 RGBA/Unshaded로 표시한다. 입력은 픽셀 알파16 이상과 실제 임시 팔다리 사각형을 사용한다. 각 publish에서 물리 커서를 읽고 Godot MousePassthrough 및 Windows WS_EX_LAYERED/WS_EX_TRANSPARENT를 함께 적용해 다른 프로세스로 입력을 전달한다. 효과에는 입력 영역을 추가하지 않는다.
- helper는 C# 빌드 의존성이며 개발 실행은 `.godot/IconWorker` 복사본을 사용한다. 배포용 내보내기·helper 동봉은 이번에 검증하지 않았다.

## 대안과 선택 이유

- UI 스레드 취득이나 취소 불가능한 장기 COM 호출을 메인 프로세스에 두는 대신 종료 가능한 작업 프로세스를 사용한다.
- 화면 OCR·스크린샷 복제 대신 Shell 이미지 사용으로 배경/라벨을 혼입하지 않는다. 썸네일·배지·선택 강조의 완전 복제는 이번 범위가 아니다.
- Godot MousePassthrough만으로 다른 프로세스 수신이 성립하지 않는 경우를 실제 시험에서 확인하여 기존 장식 효과의 layered 방식과 결합했다. 창 순서는 별도로 올리지 않는다.

## 영향과 검증

- PLAN-014의 [TECH-015](TECH-015-zombie-presentation-lifetime.md)는 캐시 유지 범위를 살아 있는 객체와 표시 전용 소멸 객체의 합집합으로 확장하고, 임시 팔다리 입력을 승인 발의 실제 알파로 교체한다. 아래 PLAN-012 기록은 당시 검증 범위다.

- [최종 기록](../plans/evidence/PLAN-012/run-014/README.md): RGBA roundtrip·실제 viewport·치료/회수/드래그·구멍 클릭 투과·취소·갱신 실패/늦은 세대·자원 정리.
- 실제 LG/96DPI 검증 및 사용자 미리보기 승인. 혼합 DPI·Explorer 실제 재시작·동기화 배지·장기 비용은 미검증. Explorer 실패/복구는 제품 취득 경계에 실패/지연을 넣어 검사했다.
- 입력 윤곽 갱신은 렌더 프레임 주기다. 초고속 커서 이동과 같은 프레임 클릭의 지연 위험은 남으며, 현재 실제 입력 검증 범위와 구분한다.
