---
id: TECH-002
status: accepted
title: Godot 4.7.2 .NET과 최소 C# 개발 경로
updated: 2026-11-23
---

# TECH-002 Godot 4.7.2 .NET과 최소 C# 개발 경로

- 결정일: 2026-11-23
- 근거: 사용자의 PLAN-002 실행 승인 안에서 개발 환경·최소 프로젝트 구성을 결정함.
- 관련 작업: [PLAN-002](../plans/PLAN-002-development-environment.md).
- 기준: [기획 원본](../../SCP_site-desktop.md)의 Godot 4.x .NET/C#·Windows 대상.
- 적용: [프로젝트](../../GodotProject/project.godot), [C# 프로젝트](../../GodotProject/SiteDesktop.csproj), [실행 스크립트](../../scripts/godot.ps1).
- 대체 관계: 최초 엔진·개발 도구 결정.

## 문제와 제약

기존 PATH에서 Godot과 dotnet을 찾을 수 없었다. Unity 설치에는 .NET SDK 8.0.318과 런타임 8.0.21이 있었다.
기획은 Windows 데스크톱 2D지만 투명 오버레이의 구현 전략은 PLAN-003에서 검증해야 한다.

## 결정

- 공식 Windows x64 Godot .NET 4.7.2 배포본을 프로젝트 로컬 .tools에 배치한다.
- 기존 Unity 6000.6.7f1의 SDK 8.0.318을 읽기 전용으로 재사용한다. 시스템 전역 설치·PATH 변경은 하지 않는다.
- Godot.NET.Sdk 4.7.2와 net8.0을 사용한다. global.json에서 8.0.318과 latestPatch 정책을 지정한다.
- GodotProject를 엔진 루트로 하고 Main을 제품 진입 씬, EnvironmentSmoke를 별도 검증 씬으로 둔다.
- 최소 2D 창에는 Compatibility/OpenGL을 사용한다. 투명 표시와 다중 모니터 적합성은 다음 PLAN에서 판정한다.
- 실행 스크립트는 로컬 JSON의 도구 경로를 사용하며 호출 환경을 복원한다. Smoke는 로그·수명 표시·실행 한계로 판정한다.
- 원본·UID·솔루션을 보존하고 로컬 도구·머신별 경로·Godot/IDE 생성물을 .gitignore로 구분한다.

## 대안과 선택 이유

- 시스템 .NET SDK 추가 설치: 현재 지원 조합의 기존 SDK로 컴파일·실행이 가능하여 추가 설치를 피했다.
- 일반 Godot 배포: C# 지원이 필요하므로 .NET 에디션을 선택했다.
- Forward+: 현재 최소 2D 환경에는 고급 3D 렌더링 기능이 필요하지 않아 Compatibility를 선택했다.
- 제품 코드에 자동 종료용 분기 추가: 제품 씬을 별도 검증 씬에서 실행하여 제품 기본 동작과 검증을 구분했다.

## 영향과 검증

- Unity SDK 경로는 머신 의존적이다. 해당 Unity 버전 삭제 시 로컬 설정을 호환 SDK 경로로 바꿔야 한다.
- Godot 배포본·SDK·NuGet 패키지는 버전을 맞춰 사용한다. 사용자 전역 NuGet 설정은 수정하지 않았다.
- 컴파일 0경고·0오류, headless 초기화/종료, 실제 Windows 창·Exit 버튼, 재실행·로그 보호·환경 복원을 확인했다.
- 에디터 리소스 임포트는 headless로 검증했다. 편집기 UI 조작·내보내기·오버레이·성능 검증은 포함하지 않는다.
- 실제 결과·다운로드 해시·서명·명령은 PLAN-002의 증거에 연결한다.

## 공식 근거

- [Godot Windows 공식 배포](https://godotengine.org/download/windows/): 조사 시 4.7.2 .NET 배포 확인.
- [C# 준비와 프로젝트 흐름](https://docs.godotengine.org/en/4.7/tutorials/scripting/c_sharp/c_sharp_basics.html): 별도 .NET SDK와 .NET 에디션 필요.
- [CLI](https://docs.godotengine.org/en/4.7/tutorials/editor/command_line_tutorial.html): path·headless·scene·import·log-file 옵션.
- [렌더러](https://docs.godotengine.org/en/4.7/tutorials/rendering/renderers.html): 2D 프로젝트의 Compatibility 선택 근거.
