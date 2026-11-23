# 아트 구조와 명명

적용: 아트 파일 생성·이동·명명. 탐색: [Art 폴더](../../README.md).

## 저장 위치

아래 경로는 `C:/Dev/scp_desktop/Art` 기준이다. 에셋별 경로는 실제 자료가 생길 때 만든다.

| 경로 | 역할 |
| --- | --- |
| assets/<category>/<AssetId>/source/<AssetId>.aseprite | 현재 편집용 원본 |
| assets/<category>/<AssetId>/spec.md | 현재 규격·제작 ART·요청과 적용 기록 연결 |
| assets/<category>/<AssetId>/revisions/rNNN/source/ | 제출 시점 원본 보존 |
| assets/<category>/<AssetId>/revisions/rNNN/export/ | 제출 시점 PNG 시트·필요한 메타데이터 |
| assets/<category>/<AssetId>/revisions/rNNN/README.md | 제출 당시 명세와 파일 목록 |
| assets/<category>/<AssetId>/revisions/rNNN/review/ | 원본 크기·확대 비교·애니메이션 미리보기·피드백 |
| palettes/ | 실제 채택한 팔레트 |
| references/ | 실제 제공·채택한 기준 이미지와 출처 |
| docs/plans/ | ART-NNN·목록·승인/상태 관리·템플릿 |
| docs/plans/evidence/ART-NNN/run-NNN/ | 제작 작업별 기술 검증 로그·결과 |
| docs/evidence/ | PLAN 외 하네스 관리 작업의 기록 |

제품 PLAN 검증 로그는 프로젝트 `docs/plans/evidence/PLAN-NNN/run-NNN/`에 둔다.
아트 리뷰 이미지는 리비전 review에 한 번만 보관하고 ART·개발 PLAN 증거에서 연결한다.
분류는 characters·ui·vfx·objects 등 실제 대상에 맞는 소문자 이름을 사용한다.

## 식별과 보존

- AssetId는 지속적인 영문 식별자다. 예시 `Scp999`는 실제 에셋 등록이 아니다.
- 같은 에셋의 수정은 같은 AssetId와 증가하는 리비전으로 관리한다. final-final 이름을 쓰지 않는다.
- 작업용 source와 보존용 revisions를 구분하고 제출본은 일상 편집 대상으로 삼지 않는다.
- 같은 원본의 동작·방향·변형은 태그·프레임으로 관리할 수 있다. 파일 분할 여부는 실제 편집·출력 요구로 정한다.
- 배치 경로에 파일을 만든 뒤 [에셋 목록](../../assets/README.md)에 spec 링크를 추가한다.
- 그림·수령 파일이 없으면 예정 경로를 코드 표기로 적는다. 존재하지 않는 파일의 링크를 만들지 않는다.
- Godot의 최종 런타임 경로는 [전달 지침](handoff.md)을 따른다. source와 review를 제품 폴더로 일괄 복사하지 않는다.

## 문서

- Art/AGENTS.md는 MAP만 두고 100줄 이내로 유지한다.
- Art/docs/agents의 각 지침은 150줄 이내로 유지한다. 길어지면 주제별 분리 후 MAP을 갱신한다.
- Art/docs/plans/plan.md도 150줄 이내로 유지한다. 제작은 [ART 관리 규칙](../plans/plan.md)을 따른다.
- 개발 PLAN은 프로젝트 docs/plans, 공통 TECH는 docs/decisions에 둔다. 개발·아트의 승인과 active 제한은 각각 관리한다.
