local a=assert(_G.art007);local pc=app.pixelColor;local checks={};local failures={}
local function check(name,pass,value)checks[#checks+1]={name=name,pass=pass,value=value};if not pass then failures[#failures+1]=name end end
local f=assert(io.open(a.root..'revisions/r001/export/Scp049Mutation.contract.json','r'));local c=json.decode(f:read('*a'));f:close()
local sheet=Image{fromFile=a.root..'revisions/r001/export/Scp049Mutation.png'};check('PNG192x192',sheet.width==192 and sheet.height==192)
local count=0;local timing=0;local images={}
for _,path in ipairs({a.root..'source/Scp049Mutation.aseprite',a.root..'revisions/r001/source/Scp049Mutation.aseprite'})do
 local s=Sprite{fromFile=path};check('source structure '..path,s.width==48 and s.height==48 and #s.frames==16 and #s.layers==2 and #s.tags==2 and s.colorMode==ColorMode.RGB)
 for n,fr in ipairs(s.frames)do local im=Image(48,48);im:drawSprite(s,n);local r=c.frames[n].rect;local out=Image(sheet,Rectangle(r.x,r.y,48,48));if im:isEqual(out)then count=count+1 end;if math.abs(fr.duration*1000-c.frames[n].durationMs)<.001 then timing=timing+1 end;images[n]=out end
 for i,t in ipairs(s.tags)do local ct=c.tags[i];check('tag '..t.name..' '..path,t.name==ct.name and t.fromFrame.frameNumber==ct.from and t.toFrame.frameNumber==ct.to and t.repeats==(ct.loop and 0 or 1))end
 s:close()
end
check('32 source/preserved frames equal PNG',count==32,count);check('32 durations match contract',timing==32,timing)
local centerErrors=0;local boundaryErrors=0;local zeroRGBErrors=0;local visibleFrames=0;local perFrame={};local unique={}
for n,im in ipairs(images)do
 local pixels=0;local maxAlpha=0;unique[im.bytes]=true
 for it in im:pixels()do local v=it();local alpha=pc.rgbaA(v)
  if alpha>0 then pixels=pixels+1;maxAlpha=math.max(maxAlpha,alpha)
   if it.x>=12 and it.x<=35 and it.y>=12 and it.y<=35 then centerErrors=centerErrors+1 end
   if it.x==0 or it.x==47 or it.y==0 or it.y==47 then boundaryErrors=boundaryErrors+1 end
  elseif v~=0 then zeroRGBErrors=zeroRGBErrors+1 end
 end
 if pixels>0 then visibleFrames=visibleFrames+1 end
 perFrame[#perFrame+1]={frame=n,visiblePixels=pixels,maxAlpha=maxAlpha}
end
check('24x24 icon center completely transparent all frames',centerErrors==0,centerErrors)
check('no artwork touches cell edge',boundaryErrors==0,boundaryErrors)
check('transparent RGB zero',zeroRGBErrors==0,zeroRGBErrors)
check('only final transform frame is empty',visibleFrames==15 and images[16]:isEmpty(),visibleFrames)
local uniqueCount=0;for _ in pairs(unique)do uniqueCount=uniqueCount+1 end;check('16 distinct intended frames',uniqueCount==16,uniqueCount)
local transformMs=0;for n=9,16 do transformMs=transformMs+c.frames[n].durationMs end;check('transform800 including transparent100',transformMs==800 and c.frames[16].durationMs==100,transformMs)
local logoErrors=0;for k,icon in ipairs(_G.art006.icons)do for n=1,16 do local comp=Image(96,96);comp:drawImage(icon,Point(16,16));comp:drawImage(a.effects[n]);for it in icon:pixels()do if pc.rgbaA(it())>0 and comp:getPixel(it.x+16,it.y+16)~=it()then logoErrors=logoErrors+1 end end end end
check('all six icon logos unchanged by front effect',logoErrors==0,logoErrors)
local normalMs=0;local overloadMs=0;local canceledClean=true;local restarted=false;local removedClean=true;local successCount=0
for _,meta in ipairs(a.timeline)do
 if meta.stage=='NORMAL 3 SEC'then normalMs=normalMs+meta.ms end
 if meta.stage=='OVERLOAD 2 SEC'then overloadMs=overloadMs+meta.ms end
 if meta.stage=='CANCELLED / NO FEET'then canceledClean=canceledClean and not meta.effect and not meta.zombie end
 if meta.stage=='RESTART AT FRAME 1'and meta.elapsedMs==0 then restarted=meta.effect==1 end
 if meta.stage=='REMOVED / CLEAR VFX'then removedClean=removedClean and not meta.effect and not meta.zombie and meta.removed end
 if meta.stage:find('/ TRANSFORM',1,true)and meta.zombieMs==0 then successCount=successCount+1;check('success synchronizes spawn '..successCount,meta.effect==9 and meta.zombie)end
end
check('mock normal treatment exactly3000',normalMs==3000,normalMs);check('mock overload exactly2000 same120ms clip clock',overloadMs==2000,overloadMs)
check('mock cancellation has no success/feet',canceledClean);check('mock restart begins frame1',restarted);check('mock removal has no effect',removedClean)
local result={date='2026-11-25',art='ART-007',revision=1,pass=#failures==0,checks=checks,failures=failures,perFrame=perFrame,userVisualApproval=false,godotIntegration='not run; all event scenarios are mock art review, not gameplay tests'}
local out=assert(io.open(a.run..'results.json','w'));out:write(json.encode(result));out:close()
app.activeSprite=a.review;app.activeFrame=a.review.frames[3];app.refresh()
return {pass=result.pass,checks=#checks,failures=failures,uniqueFrames=uniqueCount}
