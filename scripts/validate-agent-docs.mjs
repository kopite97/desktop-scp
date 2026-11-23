import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

export const requiredGuides = [
  'workflow.md', 'plan-management.md', 'project-structure.md', 'architecture.md',
  'godot-tools.md', 'windows-integration.md', 'harness.md', 'verification.md',
];

export const requiredArtDocuments = [
  'Art/AGENTS.md', 'Art/README.md', 'Art/assets/README.md',
  'Art/docs/plans/README.md', 'Art/docs/plans/plan.md', 'Art/docs/plans/_template.md',
  ...['README', 'workflow', 'project-structure', 'design', 'lighting', 'animation', 'aseprite',
    'revisions', 'handoff', 'verification'].map(name => `Art/docs/agents/${name}.md`),
  ...['README', 'asset-spec', 'revision', 'handoff'].map(name => `Art/templates/${name}.md`),
];

const planStates = new Set(['proposed', 'active', 'completed', 'rejected', 'on hold']);
const planTypes = new Set(['development', 'art']);
const decisionStates = new Set(['proposed', 'accepted', 'superseded']);
const planSections = [
  '목표와 범위', '기준 문서와 선행 조건', '결과물 저장 위치', '작업 목록',
  '완료 기준과 검증 방법', '현재 상황과 다음 행동', '검증 결과', '관련 결정과 상태 전환',
];
const decisionSections = ['문제와 제약', '결정', '대안과 선택 이유', '영향과 검증'];

// Only our documented scalar front matter and inline file links are supported.
// This is a structural check, not a Markdown/YAML parser or approval verifier.
function withoutFencedCode(source) {
  let fence = null;
  return source.split('\n').map(line => {
    const marker = line.match(/^\s{0,3}(`{3,}|~{3,})(.*)$/);
    if (fence) {
      if (marker && marker[1][0] === fence.char && marker[1].length >= fence.length
          && !marker[2].trim()) fence = null;
      return '';
    }
    if (marker) {
      fence = { char: marker[1][0], length: marker[1].length };
      return '';
    }
    return line;
  }).join('\n');
}

function validDate(value) {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value ?? '')) return false;
  const date = new Date(`${value}T00:00:00Z`);
  return Number.isFinite(date.getTime()) && date.toISOString().slice(0, 10) === value;
}

export function validateAgentDocs(rootDirectory) {
  const root = path.resolve(rootDirectory);
  const errors = [];
  const lines = [];
  const relative = file => path.relative(root, file).replaceAll(path.sep, '/');
  const read = file => fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, '').replaceAll('\r\n', '\n');
  const isFile = file => fs.existsSync(file) && fs.statSync(file).isFile();
  const fail = message => errors.push(message);

  function markdownFiles(directory) {
    if (!fs.existsSync(directory) || !fs.statSync(directory).isDirectory()) return [];
    return fs.readdirSync(directory, { withFileTypes: true }).flatMap(entry => {
      const file = path.join(directory, entry.name);
      if (entry.isDirectory()) return markdownFiles(file);
      return entry.isFile() && entry.name.endsWith('.md') ? [file] : [];
    }).sort();
  }

  const required = [
    'AGENTS.md', 'SCP_site-desktop.md', 'docs/game-design/README.md',
    'docs/game-design/roadmap.md', 'docs/plans/README.md', 'docs/plans/plan.md', 'docs/plans/_template.md',
    'docs/decisions/README.md', 'docs/decisions/_template.md',
    ...requiredGuides.map(name => `docs/agents/${name}`),
    ...requiredArtDocuments,
  ];
  for (const name of required) {
    if (!isFile(path.join(root, name))) fail(`Missing required document: ${name}`);
  }

  const rootGuide = path.join(root, 'AGENTS.md');
  const artGuide = path.join(root, 'Art/AGENTS.md');
  const artDocuments = markdownFiles(path.join(root, 'Art'));
  for (const file of [rootGuide, artGuide, path.join(root, 'docs/plans/plan.md'),
    path.join(root, 'Art/docs/plans/plan.md'),
    ...markdownFiles(path.join(root, 'docs/agents')), ...markdownFiles(path.join(root, 'Art/docs/agents'))]) {
    if (!isFile(file)) continue;
    const source = read(file);
    const count = source ? source.replace(/\n$/, '').split('\n').length : 0;
    const limit = file === rootGuide || file === artGuide ? 100 : 150;
    lines.push(`LINES ${relative(file)}: ${count}/${limit}`);
    if (count > limit) fail(`Line limit exceeded: ${relative(file)} (${count}/${limit})`);
  }

  // Art uses its own ART registry; misplaced records must not bypass its active limit.
  for (const file of artDocuments) {
    if (/^PLAN-\d+.*\.md$/.test(path.basename(file))) {
      fail(`Art work uses ART IDs in Art/docs/plans, not PLAN IDs: ${relative(file)}`);
    }
    if (/^ART-\d+.*\.md$/.test(path.basename(file)) && path.dirname(file) !== path.join(root, 'Art/docs/plans')) {
      fail(`ART work record must be directly in Art/docs/plans: ${relative(file)}`);
    }
  }

  function records(folder, prefix, requiredFields, sections) {
    const directory = path.join(root, folder);
    const result = new Map();
    const ids = new Set();
    const managementFiles = new Set(['README.md', '_template.md']);
    if (prefix !== 'TECH') managementFiles.add('plan.md');
    const files = markdownFiles(directory).filter(file => path.dirname(file) === directory)
      .filter(file => !managementFiles.has(path.basename(file)));
    for (const file of files) {
      const name = path.basename(file);
      if (!new RegExp(`^${prefix}-\\d{3,}-[a-z0-9]+(?:-[a-z0-9]+)*\\.md$`).test(name)) {
        fail(`Invalid ${prefix} filename: ${relative(file)}`);
      }
      const source = read(file);
      const frontMatter = source.match(/^---\n([\s\S]*?)\n---(?:\n|$)/);
      const fields = new Map();
      if (!frontMatter) fail(`Missing ${prefix} front matter: ${relative(file)}`);
      else for (const line of frontMatter[1].split('\n')) {
        if (!line.trim()) continue;
        const match = line.match(/^([a-z_]+):\s*(.*?)\s*$/);
        if (!match) {
          fail(`Invalid metadata line in ${relative(file)}: ${line}`);
          continue;
        }
        if (fields.has(match[1])) fail(`Duplicate field ${match[1]}: ${relative(file)}`);
        fields.set(match[1], match[2]);
      }
      for (const key of requiredFields) {
        if (!fields.get(key)) fail(`Missing ${prefix} field ${key}: ${relative(file)}`);
      }
      const id = fields.get('id');
      if (!new RegExp(`^${prefix}-\\d{3,}$`).test(id ?? '') || !name.startsWith(`${id}-`)) {
        fail(`${prefix} ID and filename mismatch: ${relative(file)}`);
      }
      if (id && ids.has(id)) fail(`Duplicate ${prefix} ID: ${id}`);
      if (id) ids.add(id);
      const status = fields.get('status');
      if (!(prefix !== 'TECH' ? planStates : decisionStates).has(status)) {
        fail(`Invalid ${prefix} status ${JSON.stringify(status)}: ${relative(file)}`);
      }
      if (prefix === 'PLAN' && !planTypes.has(fields.get('type'))) {
        fail(`Invalid PLAN type ${JSON.stringify(fields.get('type'))}: ${relative(file)}`);
      }
      if (prefix === 'ART') {
        if (fields.get('type') !== 'art') fail(`Invalid ART type: ${relative(file)}`);
        if (!/^[A-Za-z][A-Za-z0-9_-]*$/.test(fields.get('asset_id') ?? '')) fail(`Invalid ART asset_id: ${relative(file)}`);
        const revision = Number(fields.get('revision'));
        const approved = Number(fields.get('approved_revision'));
        const validRevision = /^[1-9]\d*$/.test(fields.get('revision') ?? '') && Number.isSafeInteger(revision);
        const validApproved = /^(0|[1-9]\d*)$/.test(fields.get('approved_revision') ?? '') && Number.isSafeInteger(approved);
        if (!validRevision) fail(`Invalid ART revision: ${relative(file)}`);
        if (!validApproved) fail(`Invalid ART approved_revision: ${relative(file)}`);
        if (validRevision && validApproved) {
          if (approved > revision) fail(`ART approved_revision exceeds revision: ${relative(file)}`);
          if (status === 'completed' && approved !== revision) fail(`Completed ART requires current approved_revision: ${relative(file)}`);
        }
      }
      if (!validDate(fields.get('updated'))) fail(`Invalid updated date: ${relative(file)}`);
      const headings = new Set(withoutFencedCode(source).split('\n')
        .filter(line => line.startsWith('## ')).map(line => line.slice(3).trim()));
      for (const section of sections) {
        if (!headings.has(section)) fail(`Missing section ${JSON.stringify(section)}: ${relative(file)}`);
      }
      result.set(file, Object.fromEntries(fields));
    }
    return result;
  }

  const plans = records('docs/plans', 'PLAN', ['id', 'type', 'status', 'title', 'updated'], planSections);
  const artPlans = records('Art/docs/plans', 'ART',
    ['id', 'type', 'status', 'title', 'asset_id', 'revision', 'approved_revision', 'updated'], planSections);
  const decisions = records('docs/decisions', 'TECH', ['id', 'status', 'title', 'updated'], decisionSections);
  const active = [...plans].filter(([, metadata]) => metadata.status === 'active').map(([file]) => relative(file));
  if (active.length > 1) fail(`Expected at most 1 active PLAN; found ${active.length}`);
  lines.push(`PLANS total=${plans.size}, active=${active.length}`);
  active.forEach(file => lines.push(`ACTIVE ${file}`));
  const activeArt = [...artPlans].filter(([, metadata]) => metadata.status === 'active').map(([file]) => relative(file));
  if (activeArt.length > 1) fail(`Expected at most 1 active ART; found ${activeArt.length}`);
  lines.push(`ART_PLANS total=${artPlans.size}, active=${activeArt.length}`);
  activeArt.forEach(file => lines.push(`ACTIVE_ART ${file}`));
  lines.push(`DECISIONS total=${decisions.size}`);

  function checkIndex(folder, prefix, entries) {
    const file = path.join(root, folder, 'README.md');
    if (!isFile(file)) return;
    const indexed = new Set();
    for (const line of withoutFencedCode(read(file)).split('\n')) {
      if (!line.trim().startsWith('|')) continue;
      const columns = line.split('|').slice(1, -1).map(value => value.trim());
      const cell = columns[0] ?? '';
      const match = cell.match(new RegExp(`^\\[(${prefix}-\\d{3,})\\]\\(([^)]+)\\)$`));
      if (!match) {
        if (cell.includes(`${prefix}-`)) fail(`Malformed ${prefix} index entry: ${cell}`);
        continue;
      }
      let target;
      try { target = path.resolve(path.dirname(file), decodeURIComponent(match[2])); }
      catch { fail(`Invalid ${prefix} index path: ${match[2]}`); continue; }
      const metadata = entries.get(target);
      if (!metadata || metadata.id !== match[1]) fail(`Unknown ${prefix} index entry: ${match[1]}`);
      if (indexed.has(target)) fail(`Duplicate ${prefix} index entry: ${match[1]}`);
      indexed.add(target);
      const plain = value => (value ?? '').replace(/^`|`$/g, '');
      const status = plain(columns[prefix !== 'TECH' ? 3 : 2]);
      if (metadata && status !== metadata.status) fail(`${prefix} index status mismatch: ${match[1]}`);
      if (prefix !== 'TECH' && metadata && plain(columns[2]) !== metadata.type) {
        fail(`${prefix} index type mismatch: ${match[1]}`);
      }
      if (prefix === 'ART' && metadata) {
        for (const [column, field] of [[4, 'asset_id'], [5, 'revision'], [6, 'approved_revision']]) {
          if (plain(columns[column]) !== metadata[field]) fail(`ART index ${field} mismatch: ${match[1]}`);
        }
      }
    }
    for (const record of entries.keys()) {
      if (!indexed.has(record)) fail(`${prefix} missing from README: ${relative(record)}`);
    }
    lines.push(`${prefix}_INDEX entries=${indexed.size}`);
  }
  checkIndex('docs/plans', 'PLAN', plans);
  checkIndex('Art/docs/plans', 'ART', artPlans);
  checkIndex('docs/decisions', 'TECH', decisions);

  let checkedLinks = 0;
  const documents = new Set([rootGuide, path.join(root, 'SCP_site-desktop.md'),
    ...markdownFiles(path.join(root, 'docs')), ...artDocuments]);
  for (const file of documents) {
    if (!isFile(file)) continue;
    const source = withoutFencedCode(read(file));
    for (const match of source.matchAll(/!?\[[^\]\n]*\]\((<[^>]+>|[^)\n]+)\)/g)) {
      let target = match[1].trim().replace(/^<|>$/g, '');
      if (target.startsWith('#') || target.startsWith('//')) continue;
      if (/^[a-z][a-z\d+.-]*:/i.test(target) && !/^[a-z]:[\\/]/i.test(target)) continue;
      target = target.split('#')[0];
      if (!target) continue;
      checkedLinks++;
      try {
        const resolved = path.resolve(path.dirname(file), decodeURIComponent(target));
        if (!isFile(resolved)) fail(`Broken local file link in ${relative(file)}: ${target}`);
      } catch { fail(`Invalid local file link in ${relative(file)}: ${target}`); }
    }
  }
  lines.push(`LOCAL_LINKS checked=${checkedLinks} (inline file targets only; no anchors or web requests)`);
  return { errors, lines, planCount: plans.size, artPlanCount: artPlans.size,
    decisionCount: decisions.size, active, activeArt, checkedLinks };
}

const ownFile = fileURLToPath(import.meta.url);
if (process.argv[1] && path.resolve(process.argv[1]) === ownFile) {
  try {
    const result = validateAgentDocs(path.resolve(path.dirname(ownFile), '..'));
    result.lines.forEach(line => console.log(line));
    if (result.errors.length) {
      result.errors.forEach(error => console.error(`ERROR ${error}`));
      process.exitCode = 1;
    } else {
      console.log('PASS document structure, PLAN/ART/TECH metadata and indexes, at most 1 active per PLAN/ART scope, local file links');
    }
  } catch (error) {
    console.error(`ERROR validator could not finish: ${error.message}`);
    process.exitCode = 1;
  }
}
