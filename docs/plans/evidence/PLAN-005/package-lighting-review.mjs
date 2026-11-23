import fs from 'node:fs';
import path from 'node:path';
import assert from 'node:assert/strict';
// Repackage lossless engine-rendered PNG frames, keeping IDAT pixels unchanged.
const run=path.resolve(process.argv[2]);
const asset=path.resolve('Art/assets/characters/Scp999/revisions',process.argv[3]??'r003');
const contract=JSON.parse(fs.readFileSync(path.join(asset,'export/Scp999.contract.json')));
const crcTable=Array.from({length:256},(_,n)=>{for(let k=0;k<8;k++)n=(n&1)?0xedb88320^(n>>>1):n>>>1;return n>>>0;});
function crc(bytes){let c=0xffffffff;for(const v of bytes)c=crcTable[(c^v)&255]^(c>>>8);return(c^0xffffffff)>>>0;}
function chunk(name,data){const type=Buffer.from(name),b=Buffer.alloc(data.length+12);b.writeUInt32BE(data.length);type.copy(b,4);data.copy(b,8);b.writeUInt32BE(crc(Buffer.concat([type,data])),8+data.length);return b;}
function read(file){const b=fs.readFileSync(file),chunks=[];for(let p=8;p<b.length;){const len=b.readUInt32BE(p),name=b.toString('ascii',p+4,p+8),data=b.subarray(p+8,p+8+len);assert.equal(b.readUInt32BE(p+8+len),crc(b.subarray(p+4,p+8+len)));chunks.push({name,data});p+=len+12;}return{signature:b.subarray(0,8),header:chunks.find(c=>c.name==='IHDR').data,idat:Buffer.concat(chunks.filter(c=>c.name==='IDAT').map(c=>c.data))};}
for(const tag of contract.tags){
 const frames=contract.frames.slice(tag.from,tag.to+1).map(f=>({png:read(path.join(run,'playback',`frame-${String(f.sampleIndex).padStart(2,'0')}.png`)),ms:f.durationMs}));
 const ac=Buffer.alloc(8);ac.writeUInt32BE(frames.length);ac.writeUInt32BE(tag.loop?0:1,4);let sequence=0;
 const parts=[frames[0].png.signature,chunk('IHDR',frames[0].png.header),chunk('acTL',ac)];
 for(let i=0;i<frames.length;i++){const{png,ms}=frames[i];assert.ok(png.header.equals(frames[0].png.header));const fc=Buffer.alloc(26);fc.writeUInt32BE(sequence++);fc.writeUInt32BE(128,4);fc.writeUInt32BE(128,8);fc.writeUInt16BE(ms,20);fc.writeUInt16BE(1000,22);parts.push(chunk('fcTL',fc));if(i===0)parts.push(chunk('IDAT',png.idat));else{const seq=Buffer.alloc(4);seq.writeUInt32BE(sequence++);parts.push(chunk('fdAT',Buffer.concat([seq,png.idat])));}}
 parts.push(chunk('IEND',Buffer.alloc(0)));fs.writeFileSync(path.join(run,`${tag.name}-lit.apng`),Buffer.concat(parts));
}
const timing=JSON.parse(fs.readFileSync(path.join(run,'playback-timing.json')));
const timings=[];
for(let i=1;i<timing.length;i++){const a=timing[i-1],b=timing[i];if(a.direction!==b.direction||a.flip!==b.flip)continue;timings.push({frame:a.frame,direction:a.direction,flip:a.flip,expectedMs:contract.frames[a.frame].durationMs,actualMs:b.ms-a.ms});}
const errors=timings.map(t=>Math.abs(t.actualMs-t.expectedMs)).sort((a,b)=>a-b);
const summary={frames:contract.frames.length,tags:contract.tags.map(t=>t.name),samples:timings.length,contexts:[...new Set(timings.map(t=>`${t.direction}/${t.flip}`))],maxAbsFrameDurationErrorMs:errors.at(-1),p95AbsFrameDurationErrorMs:errors[Math.floor((errors.length-1)*.95)],idleCycleMs:contract.frames.slice(0,6).reduce((s,f)=>s+f.durationMs,0),moveCycleMs:contract.frames.slice(6,12).reduce((s,f)=>s+f.durationMs,0),apngSource:'Actual Godot rendered frames; left/day/normal facing; original frame durations, not a screen recording.'};
fs.writeFileSync(path.join(run,'playback-review.json'),JSON.stringify({summary,timings},null,2));console.log(JSON.stringify(summary,null,2));
