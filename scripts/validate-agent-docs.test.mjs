import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import test from 'node:test';
import { requiredGuides, requiredArtDocuments, validateAgentDocs } from './validate-agent-docs.mjs';

const planSections = [
  '목표와 범위', '기준 문서와 선행 조건', '결과물 저장 위치', '작업 목록',
  '완료 기준과 검증 방법', '현재 상황과 다음 행동', '검증 결과', '관련 결정과 상태 전환',
];

function fixture(t) {
  const temporaryParent = path.resolve(os.tmpdir());
  const root = fs.mkdtempSync(path.join(temporaryParent, 'scp-doc-validator-'));
  t.after(() => {
    // Only remove the exact temporary fixture created above, never the repository.
    assert.equal(path.dirname(path.resolve(root)), temporaryParent);
    assert.ok(path.basename(root).startsWith('scp-doc-validator-'));
    fs.rmSync(root, { recursive: true, force: true });
  });
  const write = (name, text) => {
    const file = path.join(root, name);
    fs.mkdirSync(path.dirname(file), { recursive: true });
    fs.writeFileSync(file, text, 'utf8');
  };
  const read = name => fs.readFileSync(path.join(root, name), 'utf8');
  const edit = (name, change) => write(name, change(read(name)));
  for (const name of [
    'AGENTS.md', 'SCP_site-desktop.md', 'docs/game-design/README.md',
    'docs/game-design/roadmap.md', 'docs/plans/plan.md', 'docs/plans/_template.md', 'docs/decisions/_template.md',
    ...requiredGuides.map(name => `docs/agents/${name}`),
    ...requiredArtDocuments,
  ]) write(name, '# Fixture\n');
  const plan = (id = 'PLAN-001', status = 'proposed') =>
    `---\nid: ${id}\ntype: development\nstatus: ${status}\ntitle: Fixture\nupdated: 2026-11-23\n---\n\n`
    + planSections.map(section => `## ${section}\n\nFixture\n`).join('\n');
  write('docs/plans/PLAN-001-fixture.md', plan());
  write('docs/plans/README.md', '| [PLAN-001](PLAN-001-fixture.md) | Fixture | development | proposed |\n');
  write('docs/decisions/TECH-001-fixture.md',
    '---\nid: TECH-001\nstatus: accepted\ntitle: Fixture\nupdated: 2026-11-23\n---\n'
    + ['문제와 제약', '결정', '대안과 선택 이유', '영향과 검증'].map(s => `\n## ${s}\n\nFixture\n`).join(''));
  write('docs/decisions/README.md', '| [TECH-001](TECH-001-fixture.md) | Fixture | accepted |\n');
  const art = (id = 'ART-001', status = 'proposed', revision = 1, approved = 0) =>
    plan(id, status).replace('type: development', 'type: art')
      .replace('title: Fixture', `title: Fixture\nasset_id: Scp999\nrevision: ${revision}\napproved_revision: ${approved}`);
  const artRow = (id = 'ART-001', status = 'proposed', revision = 1, approved = 0) =>
    `| [${id}](${id}-fixture.md) | Fixture | art | ${status} | Scp999 | ${revision} | ${approved} |\n`;
  return { root, write, read, edit, plan, art, artRow, check: () => validateAgentDocs(root) };
}

test('valid proposed-only project allows zero active plans', t => {
  const result = fixture(t).check();
  assert.deepEqual(result.errors, []);
  assert.deepEqual(result.active, []);
  assert.equal(result.planCount, 1);
  assert.equal(result.artPlanCount, 0);
  assert.deepEqual(result.activeArt, []);
  assert.equal(result.decisionCount, 1);
});

test('plan.md is a required length-limited guide, never a work PLAN', t => {
  const f = fixture(t);
  assert.deepEqual(f.check().errors, []);
  assert.equal(f.check().planCount, 1);
  f.write('docs/plans/plan.md', 'line\n'.repeat(151));
  assert.ok(f.check().errors.some(s => s.includes('Line limit exceeded: docs/plans/plan.md')));
  fs.unlinkSync(path.join(f.root, 'docs/plans/plan.md'));
  assert.ok(f.check().errors.some(s => s.includes('Missing required document: docs/plans/plan.md')));
});

test('plan.md links are still validated despite exclusion from PLAN records', t => {
  const f = fixture(t);
  f.write('docs/plans/plan.md', '# PLAN guide\n[missing](missing-guide.md)\n');
  assert.ok(f.check().errors.some(s => s.includes('Broken local file link in docs/plans/plan.md')));
  assert.equal(f.check().planCount, 1);
});

test('one active plan passes; two active plans fail', t => {
  const f = fixture(t);
  f.write('docs/plans/PLAN-001-fixture.md', f.plan('PLAN-001', 'active'));
  f.edit('docs/plans/README.md', s => s.replace('proposed', 'active'));
  assert.deepEqual(f.check().errors, []);
  f.write('docs/plans/PLAN-002-fixture.md', f.plan('PLAN-002', 'active'));
  f.edit('docs/plans/README.md', s => s + '| [PLAN-002](PLAN-002-fixture.md) | Fixture | development | active |\n');
  assert.ok(f.check().errors.some(s => s.includes('at most 1 active PLAN; found 2')));
});

test('missing documents and line limit violations fail', t => {
  const f = fixture(t);
  fs.unlinkSync(path.join(f.root, 'docs/agents/workflow.md'));
  f.write('AGENTS.md', 'line\n'.repeat(101));
  f.write('docs/agents/harness.md', 'line\n'.repeat(151));
  const errors = f.check().errors;
  assert.ok(errors.some(s => s.includes('Missing required document: docs/agents/workflow.md')));
  assert.equal(errors.filter(s => s.includes('Line limit exceeded')).length, 2);
});

test('invalid state, missing type, duplicate fields and impossible dates fail', t => {
  const f = fixture(t);
  f.edit('docs/plans/PLAN-001-fixture.md', s => s
    .replace('type: development\n', '')
    .replace('status: proposed', 'status: proposed\nstatus: done')
    .replace('2026-11-23', '2026-02-30'));
  const errors = f.check().errors;
  for (const fragment of ['Missing PLAN field type', 'Duplicate field status', 'Invalid PLAN status', 'Invalid updated date']) {
    assert.ok(errors.some(s => s.includes(fragment)), fragment);
  }
});

test('PLAN IDs, filenames, sections and index summaries are checked', t => {
  const f = fixture(t);
  f.write('docs/plans/PLAN-001-duplicate.md', f.plan());
  f.write('docs/plans/PLAN-003-mismatch.md', f.plan('PLAN-009'));
  f.edit('docs/plans/PLAN-001-fixture.md', s => s.replace('## 검증 결과', '## Other'));
  f.edit('docs/plans/README.md', s => s.replace('development | proposed', 'art | completed'));
  const errors = f.check().errors;
  for (const fragment of ['Duplicate PLAN ID', 'PLAN ID and filename mismatch', 'Missing section', 'PLAN index status mismatch', 'PLAN index type mismatch', 'PLAN missing from README']) {
    assert.ok(errors.some(s => s.includes(fragment)), fragment);
  }
});

test('unknown, malformed and duplicate index entries fail', t => {
  const f = fixture(t);
  f.edit('docs/plans/README.md', s => s + s
    + '| [PLAN-999](PLAN-999-missing.md) | Missing | development | proposed |\n'
    + '| PLAN-002 | Malformed | development | proposed |\n');
  const errors = f.check().errors;
  for (const fragment of ['Duplicate PLAN index entry', 'Unknown PLAN index entry', 'Malformed PLAN index entry']) {
    assert.ok(errors.some(s => s.includes(fragment)), fragment);
  }
});

test('TECH duplicate IDs, states, dates and index consistency are checked', t => {
  const f = fixture(t);
  f.write('docs/decisions/TECH-001-duplicate.md', f.read('docs/decisions/TECH-001-fixture.md'));
  f.edit('docs/decisions/TECH-001-fixture.md', s => s.replace('status: accepted', 'status: active').replace('2026-11-23', 'bad-date'));
  const errors = f.check().errors;
  for (const fragment of ['Duplicate TECH ID', 'Invalid TECH status', 'Invalid updated date', 'TECH index status mismatch', 'TECH missing from README']) {
    assert.ok(errors.some(s => s.includes(fragment)), fragment);
  }
});

test('real local links are checked while fenced examples, anchors and web links are skipped', t => {
  const f = fixture(t);
  f.write('docs/game-design/a space.md', '# Target\n');
  f.write('AGENTS.md', [
    '[valid](docs/game-design/a%20space.md#not-validated)',
    '[valid](<docs/game-design/a space.md>)',
    '[anchor](#not-validated)', '[web](https://example.com/missing)',
    '```markdown', '[example](missing-example.md)', '```',
    '~~~~', '[example](another-missing.md)', '~~~~',
  ].join('\n'));
  assert.deepEqual(f.check().errors, []);
  f.edit('AGENTS.md', s => s + '\n[broken](actual-missing.md)\n[bad](%ZZ.md)\n');
  const errors = f.check().errors;
  assert.ok(errors.some(s => s.includes('Broken local file link') && s.includes('actual-missing.md')));
  assert.ok(errors.some(s => s.includes('Invalid local file link') && s.includes('%ZZ.md')));
});

test('evidence markdown is link-checked but never treated as a PLAN', t => {
  const f = fixture(t);
  f.write('docs/plans/evidence/PLAN-001/run-001/README.md', '# Evidence\n');
  assert.deepEqual(f.check().errors, []);
  f.edit('docs/plans/evidence/PLAN-001/run-001/README.md', s => s + '[missing](missing.log)\n');
  assert.ok(f.check().errors.some(s => s.includes('Broken local file link')));
  assert.equal(f.check().planCount, 1);
});

test('BOM and CRLF are accepted, malformed front matter is rejected', t => {
  const f = fixture(t);
  f.edit('docs/plans/PLAN-001-fixture.md', s => '\uFEFF' + s.replaceAll('\n', '\r\n'));
  assert.deepEqual(f.check().errors, []);
  f.write('docs/plans/PLAN-001-fixture.md', '# No metadata\n');
  assert.ok(f.check().errors.some(s => s.includes('Missing PLAN front matter')));
});

test('Art documents are required and MAP/guide limits are enforced', t => {
  const f = fixture(t);
  fs.unlinkSync(path.join(f.root, 'Art/templates/asset-spec.md'));
  f.write('Art/AGENTS.md', 'line\n'.repeat(101));
  f.write('Art/docs/agents/aseprite.md', 'line\n'.repeat(151));
  const errors = f.check().errors;
  for (const fragment of ['Missing required document: Art/templates/asset-spec.md',
    'Line limit exceeded: Art/AGENTS.md', 'Line limit exceeded: Art/docs/agents/aseprite.md']) {
    assert.ok(errors.some(s => s.includes(fragment)), fragment);
  }
});

test('Art links resolve across roots and broken revision links are rejected', t => {
  const f = fixture(t);
  f.write('Art/AGENTS.md', '[plans](../docs/plans/README.md)\n');
  f.write('Art/docs/agents/workflow.md', '[plans](../../../docs/plans/plan.md)\n');
  f.write('Art/assets/Scp999/spec.md', '[revision](revisions/r001/README.md)\n');
  f.write('Art/assets/Scp999/revisions/r001/README.md', '# Revision\n');
  assert.deepEqual(f.check().errors, []);
  f.write('Art/assets/Scp999/revisions/r001/README.md', '[missing](export/missing.png)\n');
  assert.ok(f.check().errors.some(s => s.includes('Broken local file link in Art/assets/Scp999/revisions/r001/README.md')));
});

test('Art rejects PLAN IDs and misplaced ART records', t => {
  const f = fixture(t);
  f.write('Art/docs/plans/PLAN-001-fixture.md', f.plan('PLAN-001', 'active'));
  f.write('Art/assets/ART-002-fixture.md', f.art('ART-002', 'active'));
  const errors = f.check().errors;
  assert.ok(errors.some(s => s.includes('Art work uses ART IDs')));
  assert.ok(errors.some(s => s.includes('ART work record must be directly in Art/docs/plans')));
  assert.equal(f.check().planCount, 1);
});

test('development and ART may each be active, but neither scope allows two', t => {
  const f = fixture(t);
  f.write('docs/plans/PLAN-001-fixture.md', f.plan('PLAN-001', 'active'));
  f.edit('docs/plans/README.md', s => s.replace('proposed', 'active'));
  f.write('Art/docs/plans/ART-001-fixture.md', f.art('ART-001', 'active'));
  f.write('Art/docs/plans/README.md', f.artRow('ART-001', 'active'));
  let result = f.check();
  assert.deepEqual(result.errors, []);
  assert.equal(result.active.length, 1);
  assert.equal(result.activeArt.length, 1);
  assert.equal(result.artPlanCount, 1);
  f.write('Art/docs/plans/ART-002-fixture.md', f.art('ART-002', 'active'));
  f.edit('Art/docs/plans/README.md', s => s + f.artRow('ART-002', 'active'));
  result = f.check();
  assert.equal(result.errors.length, 1);
  assert.match(result.errors[0], /at most 1 active ART; found 2/);
  f.write('docs/plans/PLAN-002-fixture.md', f.plan('PLAN-002', 'active'));
  f.edit('docs/plans/README.md', s => s + '| [PLAN-002](PLAN-002-fixture.md) | Fixture | development | active |\n');
  assert.ok(f.check().errors.some(s => s.includes('at most 1 active PLAN; found 2')));
});

test('ART revision validation and completion require consistent approval metadata', t => {
  const f = fixture(t);
  const file = 'Art/docs/plans/ART-001-fixture.md';
  f.write('Art/docs/plans/README.md', f.artRow());
  const cases = [
    ['type: art', 'type: development', 'Invalid ART type'],
    ['asset_id: Scp999', 'asset_id: 999 Scp', 'Invalid ART asset_id'],
    ['revision: 1', 'revision: 0', 'Invalid ART revision'],
    ['revision: 1', 'revision: 1.5', 'Invalid ART revision'],
    ['revision: 1', 'revision: 9007199254740992', 'Invalid ART revision'],
    ['approved_revision: 0', 'approved_revision: -1', 'Invalid ART approved_revision'],
    ['approved_revision: 0', 'approved_revision: 2', 'ART approved_revision exceeds revision'],
    ['status: proposed', 'status: completed', 'Completed ART requires current approved_revision'],
    ['asset_id: Scp999\n', '', 'Missing ART field asset_id'],
  ];
  for (const [before, after, expected] of cases) {
    f.write(file, f.art().replace(before, after));
    assert.ok(f.check().errors.some(s => s.includes(expected)), `${before} -> ${after}`);
  }
  for (const [status, revision, approved] of [['active', 2, 1], ['completed', 2, 2], ['on hold', 3, 2]]) {
    f.write(file, f.art('ART-001', status, revision, approved));
    f.write('Art/docs/plans/README.md', f.artRow('ART-001', status, revision, approved));
    assert.deepEqual(f.check().errors, []);
  }
});

test('ART index validates every metadata column and record coverage', t => {
  const f = fixture(t);
  f.write('Art/docs/plans/ART-001-fixture.md', f.art());
  f.write('Art/docs/plans/README.md', f.artRow());
  assert.deepEqual(f.check().errors, []);
  f.write('Art/docs/plans/README.md', '| [ART-001](ART-001-fixture.md) | Fixture | development | active | Wrong | 2 | 1 |\n');
  for (const field of ['type', 'status', 'asset_id', 'revision', 'approved_revision']) {
    assert.ok(f.check().errors.some(s => s.includes(`ART index ${field} mismatch`)), field);
  }
  f.write('Art/docs/plans/README.md', f.artRow() + f.artRow() + f.artRow('ART-999')
    + '| ART-002 | Malformed | art | proposed | Scp999 | 1 | 0 |\n');
  for (const fragment of ['Duplicate ART index entry', 'Unknown ART index entry', 'Malformed ART index entry']) {
    assert.ok(f.check().errors.some(s => s.includes(fragment)), fragment);
  }
  f.write('Art/docs/plans/README.md', '# Empty\n');
  assert.ok(f.check().errors.some(s => s.includes('ART missing from README')));
});

test('ART records enforce IDs, states, dates and required sections', t => {
  const f = fixture(t);
  f.write('Art/docs/plans/ART-001-fixture.md', f.art()
    .replace('status: proposed', 'status: done')
    .replace('2026-11-23', '2026-02-30').replace('## 검증 결과', '## Other'));
  f.write('Art/docs/plans/ART-002-fixture.md', f.art());
  const errors = f.check().errors;
  for (const fragment of ['Invalid ART status', 'Invalid updated date', 'Missing section "검증 결과"',
    'ART ID and filename mismatch', 'Duplicate ART ID']) {
    assert.ok(errors.some(s => s.includes(fragment)), fragment);
  }
});

test('Art plan guide is required and length limited; evidence links are checked without counting records', t => {
  const f = fixture(t);
  const guide = 'Art/docs/plans/plan.md';
  f.write('Art/docs/plans/evidence/ART-001/run-001/README.md', '# Evidence\n');
  assert.deepEqual(f.check().errors, []);
  assert.equal(f.check().artPlanCount, 0);
  f.write(guide, 'line\n'.repeat(151));
  assert.ok(f.check().errors.some(s => s.includes(`Line limit exceeded: ${guide}`)));
  fs.unlinkSync(path.join(f.root, guide));
  assert.ok(f.check().errors.some(s => s.includes(`Missing required document: ${guide}`)));
  f.write('Art/docs/plans/evidence/ART-001/run-001/README.md', '[missing](missing.log)\n');
  assert.ok(f.check().errors.some(s => s.includes('Broken local file link in Art/docs/plans/evidence')));
});

test('Godot art integration and development share the root PLAN active limit', t => {
  const f = fixture(t);
  f.write('docs/plans/PLAN-001-fixture.md', f.plan('PLAN-001', 'active'));
  f.write('docs/plans/PLAN-002-art.md', f.plan('PLAN-002', 'active').replace('type: development', 'type: art'));
  f.write('docs/plans/README.md', '| [PLAN-001](PLAN-001-fixture.md) | Fixture | development | active |\n'
    + '| [PLAN-002](PLAN-002-art.md) | Art | art | active |\n');
  assert.ok(f.check().errors.some(s => s.includes('at most 1 active PLAN; found 2')));
  f.write('docs/plans/PLAN-001-fixture.md', f.plan('PLAN-001', 'completed'));
  f.edit('docs/plans/README.md', s => s.replace('development | active', 'development | completed'));
  assert.deepEqual(f.check().errors, []);
});
