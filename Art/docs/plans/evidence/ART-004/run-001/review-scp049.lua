local root='C:/Dev/scp_desktop/Art/assets/characters/Scp049/'
local run='C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-004/run-001/'
local out=root..'revisions/r001/review/'
local s=Sprite{fromFile=root..'source/Scp049.aseprite'}
local preserved=Sprite{fromFile=root..'revisions/r001/source/Scp049.aseprite'}
local pc=app.pixelColor
local function read(path)local f=assert(io.open(path,'r'));local t=f:read('*a');f:close();return json.decode(t)end
local function write(path,v)local f=assert(io.open(path,'w'));f:write(json.encode(v));f:close()end
local contract=read(root..'revisions/r001/export/Scp049.contract.json')
local ase=read(root..'revisions/r001/export/Scp049.json')
local sheet=Image{fromFile=root..'revisions/r001/export/Scp049.png'}
local errors={};local function check(ok,msg)if not ok then errors[#errors+1]=msg end end
check(s.width==64 and s.height==64 and #s.frames==42,'canvas/frame count')
check(#s.layers==8 and #s.tags==10,'layer/tag count')
check(sheet.width==448 and sheet.height==384,'fixed 7x6 sheet')
check(#ase.frames==42 and #contract.frames==42,'metadata frame count')
local ims={};local unique={};local colorSet={};local bounds={};local alphaErrors=0;local outlineErrors=0;local counts={}
local edge=pc.rgba(20,21,29,255)
for n,f in ipairs(s.frames)do
 local im=Image(64,64);im:drawSprite(s,n);ims[n]=im
 local ref=Image(64,64);ref:drawSprite(preserved,n);check(im:isEqual(ref),'preserved source frame '..n)
 local rect=contract.frames[n].frame
 check(im:isEqual(Image(sheet,Rectangle(rect.x,rect.y,64,64))),'sheet pixels frame '..n)
 check(math.floor(f.duration*1000+.5)==contract.frames[n].durationMs and ase.frames[n].duration==contract.frames[n].durationMs,'duration frame '..n)
 check(ase.frames[n].frame.x==rect.x and ase.frames[n].frame.y==rect.y,'sheet coordinate frame '..n)
 local anchor=contract.frames[n].treatmentAnchor
 if contract.frames[n].tool~='none'then check(pc.rgbaA(im:getPixel(anchor.x,anchor.y))==255,'treatment tool-tip anchor '..n)end
 local b=im:shrinkBounds();bounds[n]={x=b.x,y=b.y,w=b.width,h=b.height};check(b.x>0 and b.y>0 and b.x+b.width<64 and b.y+b.height<64,'unclipped margin '..n)
 local pixels=0;for y=0,63 do for x=0,63 do local v=im:getPixel(x,y);local a=pc.rgbaA(v)
  if a~=0 and a~=255 then alphaErrors=alphaErrors+1 end
  if a>0 then
   pixels=pixels+1;colorSet[tostring(v)]=true
   for _,d in ipairs({{-1,0},{1,0},{0,-1},{0,1}})do local xx,yy=x+d[1],y+d[2];if xx>=0 and xx<64 and yy>=0 and yy<64 and pc.rgbaA(im:getPixel(xx,yy))==0 and v~=edge then outlineErrors=outlineErrors+1 end end
  end
 end end
 counts[n]=pixels;unique[im.bytes]=true
end
check(alphaErrors==0,'alpha 0/255 only');check(outlineErrors==0,'continuous one-pixel dark boundary')
for i,t in ipairs(s.tags)do local c=contract.tags[i];check(c.name==t.name and c.from==t.fromFrame.frameNumber-1 and c.to==t.toFrame.frameNumber-1,'tag '..i)end
local function bg(w,h,kind)
 local im=Image(w,h);for y=0,h-1 do for x=0,w-1 do
  local v=kind==1 and {240,237,226}or kind==2 and {25,29,39}or ((math.floor(x/8)+math.floor(y/8))%2==0 and {126,145,153}or{190,174,157})
  im:drawPixel(x,y,pc.rgba(v[1],v[2],v[3],255))
 end end;return im
end
local function scaled(im,k)local copy=im:clone();copy:resize(im.width*k,im.height*k);return copy end
local board=bg(448,384,3);for n,im in ipairs(ims)do board:drawImage(im,Point((n-1)%7*64,math.floor((n-1)/7)*64))end;scaled(board,2):saveAs(out..'contact-sheet.png')
local background=Image(192,192)
for row,n in ipairs({1,6,18})do for kind=1,3 do local tile=bg(64,64,kind);tile:drawImage(ims[n]);background:drawImage(tile,Point((kind-1)*64,(row-1)*64))end end
background:saveAs(out..'backgrounds-1x.png');scaled(background,2):saveAs(out..'backgrounds-2x.png');scaled(background,4):saveAs(out..'backgrounds-4x.png')
local flip=Image(288,128)
for kind=1,3 do local tile=bg(96,128,kind)
 local mirrored=ims[1]:clone();mirrored:flip(FlipType.HORIZONTAL);tile:drawImage(ims[1],Point(0,0));tile:drawImage(mirrored,Point(32,0))
 local treatment=ims[18]:clone();treatment:flip(FlipType.HORIZONTAL);tile:drawImage(treatment,Point(16,64));flip:drawImage(tile,Point((kind-1)*96,0))
end;scaled(flip,3):saveAs(out..'flip-overlap.png')
-- Concept crop beside native pixels, with separate scales; this is a review board only.
local reference=Image{fromFile='C:/Dev/scp_desktop/Art/docs/plans/references/ART-004-scp-049-character/ART-004-scp-049-character-ref-001-detail-reference.png'}
local crop=Image(reference,Rectangle(210,0,390,590));crop:resize(195,295)
local comparison=bg(680,320,1);comparison:drawImage(crop,Point(8,10));comparison:drawImage(scaled(ims[1],3),Point(236,30));comparison:drawImage(scaled(ims[18],3),Point(450,30));comparison:saveAs(out..'reference-comparison.png')
-- Time-preserving preview of every authored frame on three backgrounds, plus mirrored view.
local preview=Sprite(384,192,ColorMode.RGB);preview:assignColorSpace(ColorSpace{sRGB=true})
for n,im in ipairs(ims)do
 if n>1 then preview:newEmptyFrame(n)end;preview.frames[n].duration=s.frames[n].duration
 local frame=Image(384,192)
 for kind=1,3 do local tile=bg(128,192,kind);tile:drawImage(im,Point(0,6));local mirrored=im:clone();mirrored:flip(FlipType.HORIZONTAL);tile:drawImage(mirrored,Point(64,6));tile:drawImage(scaled(im,2),Point(0,64));frame:drawImage(tile,Point((kind-1)*128,0))end
 preview:newCel(preview.layers[1],n,frame,Point(0,0))
end
preview:saveAs(out..'Scp049-review.aseprite');preview:saveCopyAs(out..'animation.gif')
local colorCount=0;for _ in pairs(colorSet)do colorCount=colorCount+1 end
local distinct=0;for _ in pairs(unique)do distinct=distinct+1 end
local result={passed=#errors==0,errors=errors,frames=42,tags=10,layers=8,colors=colorCount,distinctFrames=distinct,alphaErrors=alphaErrors,outlineBoundaryErrors=outlineErrors,bounds=bounds,opaquePixels=counts,sourceExportPixelEquality=true,preservedSourcePixelEquality=true,review='Three backgrounds, native 1x/2x, mirror, overlap; exact authored frame durations in GIF. Engine validation and user visual approval not performed.'}
write(run..'results.json',result);assert(#errors==0,table.concat(errors,'; '));return result
