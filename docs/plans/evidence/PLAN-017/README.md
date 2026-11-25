# PLAN-017 실행 증거

기준 소스 main ad177d1에 대한 미커밋 구현. 사용자 변경이 없던 PLAN 파일에서 시작했다. 실패 실행을 보존하며 최종 판정에 구 DLL 실행(run-049)을 사용하지 않는다.

## 최종 기능 근거

- [공통 행동 20개](run-050/README.md): 실제 보행/운반, 경로 캐시, 독립 FSM/취소, 모의 환경 공유와 종료, 999 배회·봉쇄·추적 dead zone, 049 불가 목표/대안/환경 복구.
- [999 최종 로직](run-051/README.md), [최종 native](run-056/README.md), [실제 입력 111개](run-019/README.md).
- [049 최종 로직](run-052/README.md), [실제 입력 71개](run-025/README.md), [즉시 치료 취소 아트 68개](run-040/README.md).
- [173 native 29개](run-046/README.md), [좀비 최종 로직](run-057/README.md), [좀비 입력/렌더 728개](run-030/README.md), [겹침 입력 197개](run-031/README.md).
- [활성화 native 80개](run-045/README.md), [시스템 입력 55개](run-026/README.md), [물리 36개](run-013/README.md).
- [트레이 사용자 조작](run-048/README.md), [최종 EXE](run-053/README.md), [재시작 관찰](run-055/README.md), [빌드/최종 검사](run-058/README.md).

## 비용과 제한

[변경 전](run-002/metrics.json) 및 [변경 후](run-041/metrics.json): 각 3회, 준비5초+측정20초, 3 SCP, 아이콘 작업자 OFF, 60fps 상한. 실시간 외부 창/커서와 배회 활동량이 달라 **고정 입력 CPU 개선율을 증명하지 않는다**.

| 지표 | 변경 전 3회 | 변경 후 3회 |
| --- | --- | --- |
| 프로세스 CPU ms | 5375 / 6968.75 / 7265.625 | 7171.875 / 7312.5 / 5875 |
| 할당 bytes | 23565288 / 29429768 / 27919776 | 14481176 / 13570824 / 13349512 |
| 프레임 간격 p95 ms | 17.4375 / 17.4624 / 17.5096 | 17.3397 / 17.313 / 17.3784 |
| OS 창 관찰 /20초 | 변경 전 계수 미계측 | 394 / 300 / 300 |

프레임 간격은 FPS 대기를 포함하며 CPU 갱신 시간과 다르다. 총 CPU가 일관되게 감소하지 않았으므로 성능 향상을 선언하지 않는다. 구조적 비용 기준은 같은 프레임 20개 요청의 추가 관찰 최대1회(run-041), 고정 모의 입력 100회에서 수집100회·각 콜백100회, 같은 발판 쌍의 이동 목표100회에서 경로 재탐색0회(run-050)로 확인했다. 전체 경로 탐색 수/CPU 자체 구간 분포는 미계측이다.

실제 혼합 DPI 장치 전환은 미검증이며 음수 좌표·배율·화면 교체는 로직과 LG 실제 소유 창 증거를 구분한다. 초기 EXE run-047은 시작 통과했지만 최종 재시작 run-054/055는 투명64px 엔진 부트 창이 한 샘플 foreground여서 통합 검사 FAIL이다. 전체 제어창 노출은 없고 173 OFF 복원·두 SCP 표시·정상 종료는 확인했다. 이 초기 포커스 제한을 숨기거나 통과로 바꾸지 않는다.

## 실행 목록

| 회차 | 대상 | 판정 (검사 수) |
| --- | --- | --- |
| [run-001](run-001/README.md) | BehaviorSmoke | FAIL |
| [run-002](run-002/README.md) | BehaviorSmoke | PASS (3) |
| [run-003](run-003/README.md) | BehaviorSmoke | PASS (6) |
| [run-004](run-004/README.md) | Scp999Smoke | FAIL |
| [run-005](run-005/README.md) | Scp999Smoke | PASS (66) |
| [run-006](run-006/README.md) | Scp049Smoke | PASS (43) |
| [run-007](run-007/README.md) | StatueSmoke | PASS (18) |
| [run-008](run-008/README.md) | Scp049Smoke | PASS (43) |
| [run-009](run-009/README.md) | ZombieArtSmoke | PASS (109) |
| [run-010](run-010/README.md) | ScpActivationSmoke | PASS (70) |
| [run-011](run-011/README.md) | Scp999Smoke | PASS (66) |
| [run-012](run-012/README.md) | SystemSmoke | PASS (18) |
| [run-013](run-013/README.md) | PlatformSmoke | PASS (36) |
| [run-014](run-014/README.md) | BehaviorSmoke | PASS (17) |
| [run-015](run-015/README.md) | Scp999Smoke | FAIL |
| [run-016](run-016/README.md) | Scp999Smoke | FAIL |
| [run-017](run-017/README.md) | Scp999Smoke | FAIL |
| [run-018](run-018/README.md) | Scp999Smoke | FAIL |
| [run-019](run-019/README.md) | Scp999Smoke | PASS (111) |
| [run-020](run-020/README.md) | Scp049Smoke | FAIL |
| [run-021](run-021/README.md) | Scp049Smoke | FAIL |
| [run-022](run-022/README.md) | Scp049Smoke | PASS (43) |
| [run-023](run-023/README.md) | ZombieArtSmoke | PASS (109) |
| [run-024](run-024/README.md) | Scp049Smoke | FAIL |
| [run-025](run-025/README.md) | Scp049Smoke | PASS (71) |
| [run-026](run-026/README.md) | SystemSmoke | PASS (55) |
| [run-027](run-027/README.md) | ZombieArtSmoke | FAIL |
| [run-028](run-028/README.md) | ZombieArtSmoke | PASS (109) |
| [run-029](run-029/README.md) | ZombieArtSmoke | FAIL |
| [run-030](run-030/README.md) | ZombieArtSmoke | PASS (728) |
| [run-031](run-031/README.md) | OverlayStackSmoke | PASS (197) |
| [run-032](run-032/README.md) | BehaviorSmoke | PASS (17) |
| [run-033](run-033/README.md) | Scp999Smoke | PASS (66) |
| [run-034](run-034/README.md) | Scp049Smoke | PASS (43) |
| [run-035](run-035/README.md) | StatueSmoke | PASS (18) |
| [run-036](run-036/README.md) | ScpActivationSmoke | PASS (70) |
| [run-037](run-037/README.md) | SystemSmoke | PASS (18) |
| [run-038](run-038/README.md) | ZombieArtSmoke | PASS (109) |
| [run-039](run-039/README.md) | DoctorArtSmoke | FAIL |
| [run-040](run-040/README.md) | DoctorArtSmoke | PASS (68) |
| [run-041](run-041/README.md) | BehaviorSmoke | PASS (21) |
| [run-042](run-042/README.md) | ScpActivationSmoke | FAIL |
| [run-044](run-044/README.md) | EXE/빌드/수동 확인 | PASS |
| [run-045](run-045/README.md) | ScpActivationSmoke | PASS (80) |
| [run-046](run-046/README.md) | StatueSmoke | PASS (29) |
| [run-047](run-047/README.md) | EXE/빌드/수동 확인 | PASS |
| [run-048](run-048/README.md) | EXE/빌드/수동 확인 | PASS |
| [run-049](run-049/README.md) | BehaviorSmoke | 구 DLL 통과 / 대체됨 (17) |
| [run-050](run-050/README.md) | BehaviorSmoke | PASS (20) |
| [run-051](run-051/README.md) | Scp999Smoke | PASS (66) |
| [run-052](run-052/README.md) | Scp049Smoke | PASS (43) |
| [run-053](run-053/README.md) | EXE/빌드/수동 확인 | PASS |
| [run-054](run-054/README.md) | EXE/빌드/수동 확인 | FAIL |
| [run-055](run-055/README.md) | EXE/빌드/수동 확인 | FAIL |
| [run-056](run-056/README.md) | Scp999Smoke | PASS (96) |
| [run-057](run-057/README.md) | ZombieArtSmoke | PASS (109) |
| [run-058](run-058/README.md) | EXE/빌드/수동 확인 | PASS |

run-043은 선행 실행 실패로 시작하지 않아 생성하지 않았다. run-058의 빌드 이후 제품 코드는 변경하지 않는다.
