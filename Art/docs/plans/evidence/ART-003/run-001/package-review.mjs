import fs from 'node:fs';
import path from 'node:path';
import assert from 'node:assert/strict';
import crypto from 'node:crypto';
const root=path.resolve('Art/assets/vfx/Scp999Trail/revisions/r001');
const run=path.resolve('Art/docs/plans/evidence/ART-003/run-001');
const contract=JSON.parse(fs.readFileSync(path.join(root,'export/Scp999Trail.contract.json')));
const native=JSON.parse(fs.readFileSync(path.join(root,'export/Scp999Trail.json')));
const review=JSON.parse(fs.readFileSync(path.join(run,'composite-review.json')));
const crcTable=Array.from({length:256},(_,n)=>{for(let k=0;k<8;k++)n=(n&1)?0xedb88320^(n>>>1):n>>>1;return n>>>0;});
function crc(bytes){let c=0xffffffff;for(const v of bytes)c=crcTable[(c^v)&255]^(c>>>8);return(c^0xffffffff)>>>0;}
function chunk(name,data){const type=Buffer.from(name),b=Buffer.alloc(data.length+12);b.writeUInt32BE(data.length);type.copy(b,4);data.copy(b,8);b.writeUInt32BE(crc(Buffer.concat([type,data])),8+data.length);return b;}
function read(file){const b=fs.readFileSync(file),chunks=[];for(let p=8;p<b.length;){const len=b.readUInt32BE(p),name=b.toString('ascii',p+4,p+8),data=b.subarray(p+8,p+8+len);assert.equal(b.readUInt32BE(p+8+len),crc(b.subarray(p+4,p+8+len)));chunks.push({name,data});p+=len+12;}const header=chunks.find(c=>c.name==='IHDR').data;assert.equal(header[9],6,'RGBA source required');return{signature:b.subarray(0,8),header,idat:Buffer.concat(chunks.filter(c=>c.name==='IDAT').map(c=>c.data))};}
function pack(out,frames,plays){const ac=Buffer.alloc(8);ac.writeUInt32BE(frames.length);ac.writeUInt32BE(plays,4);let seq=0;const first=frames[0].png,parts=[first.signature,chunk('IHDR',first.header),chunk('acTL',ac)];for(let i=0;i<frames.length;i++){const {png,ms}=frames[i];assert.ok(png.header.equals(first.header));assert.ok(ms>0 && ms<65536);const fc=Buffer.alloc(26);fc.writeUInt32BE(seq++);fc.writeUInt32BE(first.header.readUInt32BE(0),4);fc.writeUInt32BE(first.header.readUInt32BE(4),8);fc.writeUInt16BE(ms,20);fc.writeUInt16BE(1000,22);fc[24]=0;fc[25]=0;parts.push(chunk('fcTL',fc));if(i===0)parts.push(chunk('IDAT',png.idat));else{const n=Buffer.alloc(4);n.writeUInt32BE(seq++);parts.push(chunk('fdAT',Buffer.concat([n,png.idat])));}}parts.push(chunk('IEND',Buffer.alloc(0)));fs.writeFileSync(out,Buffer.concat(parts));return{frames:frames.length,durationMs:frames.reduce((s,f)=>s+f.ms,0),plays};}
assert.equal(native.frames.length,18);
for(let i=0;i<18;i++){assert.deepEqual(native.frames[i].frame,contract.frames[i].frame);assert.equal(native.frames[i].duration,contract.frames[i].durationMs);assert.equal(native.frames[i].rotated,false);assert.equal(native.frames[i].trimmed,false);}
for(let i=0;i<2;i++){const a=native.meta.frameTags[i],b=contract.tags[i];assert.equal(a.name,b.name);assert.equal(a.from,b.from);assert.equal(a.to,b.to);assert.equal(+a.repeat,1);}
const output={metadata:'PASS',animations:{}};
for(const t of contract.tags){output.animations[t.name]=pack(path.join(root,'review',t.name+'.apng'),contract.frames.slice(t.from,t.to+1).map(f=>({png:read(path.join(root,'review/frames','frame-'+String(f.index).padStart(2,'0')+'.png')),ms:f.durationMs})),1);}
output.animations.composite=pack(path.join(root,'review/trail-preview.apng'),review.timeline.map(f=>({png:read(path.join(root,'review',f.file)),ms:f.durationMs})),0);
const work=path.resolve('Art/assets/vfx/Scp999Trail/source/Scp999Trail.aseprite'),copy=path.join(root,'source/Scp999Trail.aseprite');
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
assert.equal(hash(work),hash(copy));output.sourceSha256=hash(work);
output.referenceBodySha256=hash(path.resolve('Art/assets/characters/Scp999/revisions/r005/source/Scp999-outline-unlit.aseprite'));
assert.equal(output.referenceBodySha256.toUpperCase(),'CF6FA4CBE281134A27860A0869827BD25B2C23738D2A4438B1A7075CB59BB290');
fs.writeFileSync(path.join(run,'package-result.json'),JSON.stringify(output,null,2)+'\n');console.log(JSON.stringify(output,null,2));


