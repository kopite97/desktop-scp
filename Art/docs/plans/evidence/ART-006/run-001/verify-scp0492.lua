local a=assert(_G.art006);local pc=app.pixelColor;local checks={};local failures={}
local function check(name,pass,details)checks[#checks+1]={name=name,pass=pass,details=details};if not pass then failures[#failures+1]=name end end
local paths={a.root..'source/Scp0492.aseprite',a.root..'revisions/r001/source/Scp0492.aseprite'}
local file=assert(io.open(a.root..'revisions/r001/export/Scp0492.contract.json','r'));local contract=json.decode(file:read('*a'));file:close()
local sheets={Image{fromFile=a.root..'revisions/r001/export/Scp0492_left-foot.png'},Image{fromFile=a.root..'revisions/r001/export/Scp0492_right-foot.png'}}
local matched=0;local timings=0;local outlineErrors=0;local empty=0;local bounds={32,32,-1,-1};local transparentRGBErrors=0
local cels={{},{}};local perFrame={}
for _,path in ipairs(paths)do
 local s=Sprite{fromFile=path};check('source dimensions '..path,s.width==32 and s.height==32 and #s.frames==30 and #s.layers==2 and #s.tags==7)
 check('RGB source '..path,s.colorMode==ColorMode.RGB)
 for n,f in ipairs(s.frames)do
  if math.abs(f.duration*1000-contract.frames[n].durationMs)<.001 then timings=timings+1 end
  for li,layer in ipairs(s.layers)do
   local im=Image(32,32);local cel=layer:cel(n);if cel then im:drawImage(cel.image,cel.position)end
   local r=contract.frames[n].rect;local extracted=Image(sheets[li],Rectangle(r.x,r.y,32,32))
   if im:isEqual(extracted)then matched=matched+1 end
   if path==paths[1]then cels[li][n]=im end
  end
 end
 for i,t in ipairs(s.tags)do local ct=contract.tags[i];check('tag '..i..' '..path,t.name==ct.name and t.fromFrame.frameNumber==ct.from and t.toFrame.frameNumber==ct.to and t.repeats==(ct.loop and 0 or 1))end
 s:close()
end
for li,images in ipairs(cels)do for n,im in ipairs(images)do
 local count=0;local maxAlpha=0
 for it in im:pixels()do local v=it();local alpha=pc.rgbaA(v)
  if alpha==0 then if v~=0 then transparentRGBErrors=transparentRGBErrors+1 end
  else
   count=count+1;maxAlpha=math.max(maxAlpha,alpha);bounds[1]=math.min(bounds[1],it.x);bounds[2]=math.min(bounds[2],it.y);bounds[3]=math.max(bounds[3],it.x);bounds[4]=math.max(bounds[4],it.y)
   local edge=false;for _,d in ipairs({{-1,0},{1,0},{0,-1},{0,1}})do local x=it.x+d[1];local y=it.y+d[2];if x<0 or y<0 or x>=32 or y>=32 or pc.rgbaA(im:getPixel(x,y))==0 then edge=true end end
   if edge and (pc.rgbaR(v)~=39 or pc.rgbaG(v)~=32 or pc.rgbaB(v)~=51)then outlineErrors=outlineErrors+1 end
  end
 end
 if count==0 then empty=empty+1 end
 perFrame[#perFrame+1]={slot=li,frame=n,opaquePixels=count,maxAlpha=maxAlpha}
end end
check('PNG dimensions',sheets[1].width==192 and sheets[1].height==160 and sheets[2].width==192 and sheets[2].height==160)
check('120 source/revision/layer frames equal PNG',matched==120,matched)
check('60 original and preserved frame times',timings==60,timings)
check('one pixel dark silhouette outline',outlineErrors==0,outlineErrors)
check('transparent pixels contain zero RGB',transparentRGBErrors==0,transparentRGBErrors)
check('only spawn/collect/expire first or last empty frames',empty==6,empty)
check('bottom contact at y31 with side headroom',bounds[1]>0 and bounds[3]<31 and bounds[4]==31,bounds)
local terminal={};for _,tag in ipairs(contract.tags)do if tag.name=='collect'or tag.name=='expire'then for li=1,2 do local im=cels[li][tag.to];check(tag.name..' terminal alpha slot'..li,im:isEmpty())end;check(tag.name..' terminal icon alpha',contract.frames[tag.to].bodyAlpha==0);terminal[tag.name]=tag.totalMs end end
local clipErrors=0;local holeErrors=0;local sourceMirrorErrors=0
for k,attach in ipairs(a.attachments)do for f=1,30 do for _,flip in ipairs({false,true})do
 for li,images in ipairs(cels)do local root=li==1 and attach.left or attach.right
  for it in images[f]:pixels()do if pc.rgbaA(it())>0 then local x=root+(flip and 16-it.x or it.x-16)*2;local y=56+(it.y-28)*2;if x<0 or x+1>63 or y<0 or y+1>63 then clipErrors=clipErrors+1 end end end
 end
 local composite=a.composite(k,f,flip)
 if k==4 and contract.frames[f].bodyAlpha>0 and pc.rgbaA(composite:getPixel(32,33))~=0 then holeErrors=holeErrors+1 end
 -- Pixels above the foot envelope remain identical to the original, independent of facing.
 if contract.frames[f].bodyAlpha==1 then for y=0,49 do for x=0,63 do if composite:getPixel(x,y)~=a.icons[k]:getPixel(x,y)then sourceMirrorErrors=sourceMirrorErrors+1 end end end end
end end end
check('six fixtures / two directions / 30 frames remain in 64px',clipErrors==0,clipErrors)
check('transparent fixture hole preserved',holeErrors==0,holeErrors)
check('icon logo pixels unchanged by facing',sourceMirrorErrors==0,sourceMirrorErrors)
local result={date='2026-11-25',art='ART-006',revision=1,pass=#failures==0,checks=checks,failures=failures,perFrame=perFrame,disappearanceMs=terminal,userVisualApproval=false,godotIntegration='not run; separate development scope',playback='see playback.json; this audit is static and file-based'}
local out=assert(io.open(a.run..'results.json','w'));out:write(json.encode(result));out:close()
app.activeSprite=a.review;app.activeFrame=a.review.frames[10];app.refresh()
return {pass=result.pass,checks=#checks,failures=failures,bounds=bounds}
