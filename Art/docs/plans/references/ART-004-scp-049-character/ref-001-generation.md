# ref-001 생성 기록

- 생성일: 2026-11-24, 사용자 “역병의사 이미지를 검색해서 참고할 만한 레퍼런스 이미지를 생성해줄래?” 요청.
- 도구: 내장 image_gen, imagegen 스킬의 기본 모드. 불투명 배경.
- 이미지: [detail-reference.png](ART-004-scp-049-character-ref-001-detail-reference.png).
- 원래 출력 파일명: `exec-4d45edc1-ef89-4221-b5a5-bd1aa93ea3e3.png`. 생성 원본을 보존하고 프로젝트 폴더에 복사했다.
- 검색: `SCP 049 plague doctor mask black robe reference`, `site:scp-wiki.wikidot.com scp-049 image credits`.
- [SCP Wiki 원전](https://scp-wiki.wikidot.com/scp-049): 로브·도자기처럼 보이는 가면·검은 의사 가방의 서술을 확인했다. 원전의 성인 비율 대신 프로젝트의 SD 비율을 지시했다.
- 검색 후보: [zhmdlmag 3D 모델](https://sketchfab.com/3d-models/scp-049-plague-doctor-ee445ccb6968461c841cc9da6c424d4f), [DeluCat 일러스트](https://delucat.deviantart.com/art/SCP-049-Plague-Doctor-433787272). 이미지 검색의 설명을 확인했으나 원문 페이지는 403, Wiki 원본 이미지 요청은 시간 초과였다. 후보 이미지를 로컬로 내려받거나 생성 입력으로 첨부하지 않았다.
- 생성 방식: 확인한 서술과 검색의 일반적 모티프를 텍스트 프롬프트로 전달했다. 특정 검색 이미지를 직접 편집하거나 합성한 결과가 아니다.
- 출력 검토: 3/4 정면·측면·후면, 가면·천·소품 패널과 레이블 확인. 픽셀 아트 원본·엄밀한 2등신 측정·프레임/입력 계약의 검증을 대신하지 않는 디자인 참고용 이미지다.

## 사용한 전체 프롬프트

```text
Use case: stylized-concept.
Asset type: detailed character reference sheet for ART-004, an SCP-049 desktop pixel-art game character. This is a high-resolution design reference, not a final sprite or animation sheet.
Primary request: Create a polished, cohesive SCP-049 plague doctor design with significantly richer readable detail than a simple slime character. Derive broad motifs from researched SCP-049 descriptions: pale ceramic-like long beak, deep dark eye sockets, black hood and heavy robes, black doctor's bag, restrained surgical instruments. Create a new composition rather than reproducing an existing artist's image.
Composition: landscape 3:2 reference board, generous margins and orderly panels on flat medium warm gray background. Top/main row: one large full-body three-quarter front view on left, smaller matching pure side and rear views at right. Bottom strip: enlarged mask close-up, layered shoulder/robe-fold construction, gloved hand and compact black leather doctor's bag plus small scalpel and syringe. All panels depict the SAME design with consistent construction, colors and proportions. Separate panels cleanly; no overlapping silhouettes.
Character proportions: clearly TWO HEADS TALL total, large hooded head approximately half total height, compact torso and short visible boots, a dignified eerie-but-cute SD character. NOT a tall adult, NOT a 3D collectible, NOT photorealistic. Hood close to skull, no wide-brimmed hat. Large sculpted ivory beak with deliberate ridge, taper and restrained wear, two deep shadowed eye openings without glowing eyes; no round steampunk goggles. No exposed face.
Detail hierarchy: immediately readable mask silhouette first; charcoal hood framing mask second; layered shoulder cape with broad weighted folds and subtle hem stitching third; small leather belt, worn satchel seams, metal clasps and glove finger articulation last. Maintain large readable color planes: dark blue charcoal highlights distinguish heavy cloth from near-black smoother leather; warm ivory mask, muted gunmetal tools. Avoid excessive ornaments, belts or tiny all-over texture. Compact soft cloth volume, readable elbow and sleeve overlap, both gloved hands anatomically clear. Main pose: dignified quiet stance, one hand carries a small black doctor bag and the other rests near torso; no weapons in threatening pose.
Style/medium: excellent professional 2D game concept art, confident crisp dark outlines, controlled 3-to-5-step cel shading with restrained material texture, accurate consistent turnaround, legible shapes that can later be translated into 64x64 or 128x128 pixel art. Do not fake pixel resolution or render blurred tiny sprite samples.
Lighting: consistent gentle upper-left studio light, no rim glow, no dramatic black background, no depth-of-field.
Text: only short precise English labels: "SCP-049", "DETAIL REFERENCE", "FRONT 3/4", "SIDE", "BACK", "MASK", "CLOTH", "TOOLS". Small discreet "CONCEPT" label. No other text, no logos, no watermark, no stats, no mock game interface.
Constraints: clean non-gory reference sheet, no blood, no patients, no other characters. All views complete and unclipped. High visual detail but disciplined design suitable for small desktop character readability. Opaque background.
```
