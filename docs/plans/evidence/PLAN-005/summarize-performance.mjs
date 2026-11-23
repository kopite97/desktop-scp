import fs from 'node:fs';
import path from 'node:path';
const run=path.resolve(process.argv[2]);
const raw=JSON.parse(fs.readFileSync(path.join(run,'performance.json')));
const pairs=[];
const baselineMode=raw.samples.some(s=>s.mode==='legacy')?'legacy':'base';
for(const windowCount of [1,2])for(let repeat=0;repeat<3;repeat++){
 const base=raw.samples.find(s=>s.windowCount===windowCount&&s.repeat===repeat&&s.mode===baselineMode);
 const lit=raw.samples.find(s=>s.windowCount===windowCount&&s.repeat===repeat&&s.mode==='lit');
 if(!base||!lit)throw new Error('Missing paired sample');
 const pair={windowCount,repeat,cpuRenderP95DeltaMs:lit.cpuRenderP95Ms-base.cpuRenderP95Ms,gpuRenderP95DeltaMs:lit.gpuRenderP95Ms-base.gpuRenderP95Ms,cpuOneCoreDeltaPercentagePoints:lit.processCpuOneCorePercent-base.processCpuOneCorePercent,workingSetDeltaMiB:(lit.workingSetBytes-base.workingSetBytes)/1048576,frameP95DeltaMs:lit.frameP95Ms-base.frameP95Ms};
 pair.pass=base.gpuRenderP95Ms>0&&lit.gpuRenderP95Ms>0&&pair.cpuRenderP95DeltaMs<=1&&pair.gpuRenderP95DeltaMs<=1&&pair.cpuOneCoreDeltaPercentagePoints<=5&&pair.workingSetDeltaMiB<=32;
 pairs.push(pair);
}
const summary={status:pairs.every(p=>p.pass)?'PASS':'FAIL',method:'All six paired runs; no outlier removal',budgets:{cpuRenderAdditionalP95Ms:1,gpuRenderAdditionalP95Ms:1,cpuOneCoreAdditionalPercentagePoints:5,workingSetAdditionalMiB:32},totalWorkingSetDriftMiB:(raw.samples.at(-1).workingSetBytes-raw.samples[0].workingSetBytes)/1048576,pairs,limitations:'Shared 128x128 render target, 1-2 display windows, one animated character state. Not a general product benchmark.'};
fs.writeFileSync(path.join(run,'performance-summary.json'),JSON.stringify(summary,null,2));
const columns=Object.keys(raw.samples[0]);fs.writeFileSync(path.join(run,'performance.csv'),columns.join(',')+'\n'+raw.samples.map(s=>columns.map(k=>s[k]).join(',')).join('\n')+'\n');
console.log(JSON.stringify(summary,null,2));
