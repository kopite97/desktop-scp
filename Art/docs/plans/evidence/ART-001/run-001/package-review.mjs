import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';
import assert from 'node:assert/strict';
import crypto from 'node:crypto';
const asset=path.resolve(import.meta.dirname,'../../../../../assets/characters/Scp999');
const revision=path.join(asset,'revisions/r001');
const review=path.join(revision,'review');
const manifest=JSON.parse(fs.readFileSync(path.join(revision,'export/Scp999.json')));
const contract=JSON.parse(fs.readFileSync(path.join(revision,'export/Scp999.contract.json')));
const audit=JSON.parse(fs.readFileSync(path.join(import.meta.dirname,'pixel-audit.json')));
const png=fs.readFileSync(path.join(revision,'export/Scp999.png'));
const crcTable=Array.from({length:256},(_,n)=>{for(let k=0;k<8;k++)n=(n&1)?0xedb88320^(n>>>1):n>>>1;return n>>>0;});
function crc(bytes){let c=0xffffffff;for(const v of bytes)c=crcTable[(c^v)&255]^(c>>>8);return (c^0xffffffff)>>>0;}
function chunk(name,data){const type=Buffer.from(name);const b=Buffer.alloc(data.length+12);b.writeUInt32BE(data.length);type.copy(b,4);data.copy(b,8);b.writeUInt32BE(crc(Buffer.concat([type,data])),8+data.length);return b;}
const sig=Buffer.from([137,80,78,71,13,10,26,10]);
assert.ok(png.subarray(0,8).equals(sig));
const idat=[];let w=0,h=0;
for(let i=8;i<png.length;){const len=png.readUInt32BE(i);const name=png.toString('ascii',i+4,i+8);const data=png.subarray(i+8,i+8+len);
 assert.equal(png.readUInt32BE(i+8+len),crc(png.subarray(i+4,i+8+len)));
 if(name==='IHDR'){w=data.readUInt32BE(0);h=data.readUInt32BE(4);assert.deepEqual([...data.subarray(8)],[8,6,0,0,0]);}
 if(name==='IDAT')idat.push(data);i+=len+12;}
const raw=zlib.inflateSync(Buffer.concat(idat));const pixels=Buffer.alloc(w*h*4);const stride=w*4;
function paeth(a,b,c){const p=a+b-c,pa=Math.abs(p-a),pb=Math.abs(p-b),pc=Math.abs(p-c);return pa<=pb&&pa<=pc?a:pb<=pc?b:c;}
for(let y=0;y<h;y++){const filter=raw[y*(stride+1)];for(let x=0;x<stride;x++){
 const i=y*stride+x,a=x>=4?pixels[i-4]:0,b=y?pixels[i-stride]:0,c=y&&x>=4?pixels[i-stride-4]:0;
 const predictors=[0,a,b,Math.floor((a+b)/2),paeth(a,b,c)];assert.ok(filter<=4);
 pixels[i]=(raw[y*(stride+1)+1+x]+predictors[filter])&255;}}
function canvas(width,height,bg){const data=Buffer.alloc(width*height*4);if(bg)for(let y=0;y<height;y++)for(let x=0;x<width;x++){const c=bg(x,y);data.set([...c,255],(y*width+x)*4);}return{width,height,data};}
function blit(dst,src,dx,dy,scale=1,flip=false){for(let y=0;y<src.height*scale;y++)for(let x=0;x<src.width*scale;x++){
 const tx=dx+x,ty=dy+y;if(tx<0||ty<0||tx>=dst.width||ty>=dst.height)continue;
 const sx=flip?src.width-1-Math.floor(x/scale):Math.floor(x/scale);
 const si=(Math.floor(y/scale)*src.width+sx)*4,di=(ty*dst.width+tx)*4;
 const sa=src.data[si+3]/255,da=dst.data[di+3]/255,oa=sa+da*(1-sa);
 if(oa===0)continue;for(let c=0;c<3;c++)dst.data[di+c]=Math.round((src.data[si+c]*sa+dst.data[di+c]*da*(1-sa))/oa);
 dst.data[di+3]=Math.round(oa*255);
}}
const frames=manifest.frames.map((f,i)=>{
 assert.deepEqual(f.sourceSize,{w:64,h:64});assert.equal(f.trimmed,false);assert.equal(f.rotated,false);
 assert.equal(f.frame.x,(i%8)*64);assert.equal(f.frame.y,Math.floor(i/8)*64);
 assert.equal(f.duration,contract.frames[i].durationMs);
 const im=canvas(64,64);for(let y=0;y<64;y++)pixels.copy(im.data,y*256,((f.frame.y+y)*w+f.frame.x)*4,((f.frame.y+y)*w+f.frame.x+64)*4);return im;});
function compress(im){const rows=Buffer.alloc((im.width*4+1)*im.height);for(let y=0;y<im.height;y++)im.data.copy(rows,y*(im.width*4+1)+1,y*im.width*4,(y+1)*im.width*4);return zlib.deflateSync(rows);}
function header(im){const b=Buffer.alloc(13);b.writeUInt32BE(im.width);b.writeUInt32BE(im.height,4);b[8]=8;b[9]=6;return chunk('IHDR',b);}
function savePNG(file,im){fs.writeFileSync(file,Buffer.concat([sig,header(im),chunk('sRGB',Buffer.from([0])),chunk('IDAT',compress(im)),chunk('IEND',Buffer.alloc(0))]));}
function apng(file,images,durations,loops=0){
 const ac=Buffer.alloc(8);ac.writeUInt32BE(images.length);ac.writeUInt32BE(loops,4);
 const parts=[sig,header(images[0]),chunk('sRGB',Buffer.from([0])),chunk('acTL',ac)];let seq=0;
 images.forEach((im,i)=>{const fc=Buffer.alloc(26);fc.writeUInt32BE(seq++);fc.writeUInt32BE(im.width,4);fc.writeUInt32BE(im.height,8);fc.writeUInt16BE(durations[i],20);fc.writeUInt16BE(1000,22);fc[24]=0;fc[25]=0;parts.push(chunk('fcTL',fc));
  const compressed=compress(im);
  if(i===0)parts.push(chunk('IDAT',compressed));else{const prefix=Buffer.alloc(4);prefix.writeUInt32BE(seq++);parts.push(chunk('fdAT',Buffer.concat([prefix,compressed])));}});
 parts.push(chunk('IEND',Buffer.alloc(0)));fs.writeFileSync(file,Buffer.concat(parts));
}
const checker=(x,y)=>((Math.floor(x/8)+Math.floor(y/8))%2)?[56,61,72]:[65,70,82];
const contact=canvas(1280,640,checker);frames.forEach((im,i)=>blit(contact,im,(i%10)*128,Math.floor(i/10)*128,2));
savePNG(path.join(review,'contact-sheet.png'),contact);
const backgrounds=[()=>[244,241,233],()=>[26,30,40],(x,y)=>((Math.floor(x/8)+Math.floor(y/8))%2)?[126,153,164]:[194,173,146]];
const comparison=canvas(384,448,()=>[36,40,50]);
for(let c=0;c<3;c++){for(let y=0;y<448;y++)for(let x=0;x<128;x++)comparison.data.set([...backgrounds[c](x,y),255],(y*384+c*128+x)*4);
 [0,8,21].forEach((n,row)=>blit(comparison,frames[n],c*128,row*128,2));blit(comparison,frames[0],c*128+32,384);}
savePNG(path.join(review,'background-comparison.png'),comparison);
const upscaled=frames.map(im=>{const out=canvas(128,128);blit(out,im,0,0,2);return out;});
apng(path.join(review,'playback.apng'),upscaled,manifest.frames.map(f=>f.duration));
for(const tag of contract.tags){const data=upscaled.slice(tag.from,tag.to+1),duration=manifest.frames.slice(tag.from,tag.to+1).map(f=>f.duration);
 apng(path.join(review,tag.name+'.apng'),data,duration,tag.loop?0:1);}
savePNG(path.join(review,'idle-2x.png'),upscaled[0]);
const mirrored=canvas(256,128,checker);blit(mirrored,frames[8],0,0,2);blit(mirrored,frames[8],128,0,2,true);savePNG(path.join(review,'direction-comparison.png'),mirrored);
assert.equal(audit.sourceExportMismatches,0);assert.equal(audit.outlineBoundaryErrors,0);
assert.equal(frames.length,50);assert.equal(contract.tags.length,10);
for(let i=0;i<contract.tags.length;i++){const a=contract.tags[i],b=manifest.meta.frameTags[i];assert.equal(a.name,b.name);assert.equal(a.from,b.from);assert.equal(a.to,b.to);}
for(const f of audit.frames){assert.ok(f.partialAlphaPixels>0);assert.ok(f.bounds.x>0&&f.bounds.y>0&&f.bounds.x+f.bounds.width<64&&f.bounds.y+f.bounds.height<64);}
const move=audit.frames.slice(6,12);assert.ok(move.every(f=>f.bounds.y+f.bounds.height===56));assert.ok(Math.max(...move.map(f=>f.bounds.width))-Math.min(...move.map(f=>f.bounds.width))>=6);
const source=fs.readFileSync(path.join(asset,'source/Scp999.aseprite'));const preserved=fs.readFileSync(path.join(revision,'source/Scp999.aseprite'));assert.ok(source.equals(preserved));
const results={status:'PASS',frames:frames.length,tags:contract.tags.length,sheet:{width:w,height:h,columns:8,rows:7},sourceExportPixelMismatches:0,outlineBoundaryErrors:0,partialAlphaFrames:50,clippedFrames:0,groundedMoveFrames:6,moveCycleMs:manifest.frames.slice(6,12).reduce((n,f)=>n+f.duration,0),sourceSHA256:crypto.createHash('sha256').update(source).digest('hex'),userVisualApproval:'pending',godotIntegration:'not executed',reviewFiles:fs.readdirSync(review).filter(n=>/\.(png|apng)$/.test(n))};
fs.writeFileSync(path.join(import.meta.dirname,'results.json'),JSON.stringify(results,null,2)+'\n');console.log(JSON.stringify(results,null,2));
