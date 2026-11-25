# PLAN-017 run-058

- 환경: Windows, Godot 4.7.2 mono, .NET 8.0.318. 실제 화면 검사는 LG GSM5BDE/96dpi, 모의 음수 원점·다중 화면/배율과 구분한다.
- 대상/명령(대기 상한 인자는 생략 가능): `./scripts/godot.ps1 -Action Build; node scripts/validate-agent-docs.mjs; git diff --check`
- 결과: PASS.
- 해석: 최종 C# 컴파일 경고 0/오류 0. EXE/PCK SHA-256 및 문서/공백 검사 기록.
- 한계: 실제 혼합 DPI 하드웨어·장시간 부하 미검증. 모의 검사는 OS 입력 통과 증거가 아니다.
- 선택 자료: 전환 기록은 BehaviorSmoke, 비용은 측정 회차, PNG는 관련 렌더 회차에만 생성. 다른 회차는 불필요하여 생략.

- [artifact-hashes.json](artifact-hashes.json)
- [build.log](build.log)

- [docs-validation.log](docs-validation.log)
- [diff-check.log](diff-check.log)
