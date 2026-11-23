local root="C:/Dev/scp_desktop/Art/assets/vfx/Scp999Reaction/"
local review=root.."revisions/r001/review/"
local engine="C:/Dev/scp_desktop/docs/plans/evidence/PLAN-005/run-010/"
local pc=app.pixelColor;local s=sprite
local effects={}
for n=1,16 do effects[n]=Image(32,32);effects[n]:drawSprite(s,n)end
local function scale(im,k)local out=im:clone();out:resize(im.width*k,im.height*k);return out end
local backgrounds=Image{fromFile=engine.."backgrounds.png"}
for _,k in ipairs({1,2})do
 local board=Image(192*k,160*k)
 for row=0,1 do for col=0,2 do
  local bx,by=col*64*k,row*80*k
  for y=0,80*k-1 do for x=0,64*k-1 do
   local c=col==0 and {242,240,232} or col==1 and {26,31,41} or (math.floor(x/(4*k))+math.floor(y/(4*k)))%2==0 and {191,173,145} or {125,153,163}
   board:drawPixel(bx+x,by+y,pc.rgba(c[1],c[2],c[3],255))
  end end
  local tile=Image(backgrounds,Rectangle(col*128,row*128,128,128))
  tile:resize(64*k,64*k);board:drawImage(tile,Point(bx,by+16*k))
  board:drawImage(scale(effects[4],k),Point(bx+2*k,by+4*k))
  board:drawImage(scale(effects[12],k),Point(bx+30*k,by+4*k))
 end end
 board:saveAs(review.."composite-backgrounds-"..k.."x.png")
end
local body={};for n=13,19 do body[n]=Image{fromFile=engine.."playback/frame-"..string.format("%02d",n-1)..".png"}end
local events={{ms=0,kind="heart"},{ms=200,kind="note"},{ms=650,kind="heart"},{ms=950,kind="note"},{ms=1300,kind="heart"},{ms=1600,kind="note"}}
local durations={70,80,100,160,160,120,100,60}
local boundaries={[0]=true,[2600]=true}
for t=0,2600,50 do boundaries[t]=true end
for _,ev in ipairs(events)do local t=ev.ms;boundaries[t]=true;for _,d in ipairs(durations)do t=t+d;if t<=2600 then boundaries[t]=true end end end
local bodyCycle=0;for n=13,19 do bodyCycle=bodyCycle+args.bodyFrames[n].ms end
for start=0,2600,bodyCycle do local t=start;for n=13,19 do t=t+args.bodyFrames[n].ms;if t<=2600 then boundaries[t]=true end end end
local times={};for t,_ in pairs(boundaries)do table.insert(times,t)end;table.sort(times)
local function bodyAt(t)local rem=t%bodyCycle;for n=13,19 do if rem<args.bodyFrames[n].ms then return n end;rem=rem-args.bodyFrames[n].ms end;return 13 end
local function effectAt(age)local elapsed=0;for phase,ms in ipairs(durations)do if age<elapsed+ms then return phase end;elapsed=elapsed+ms end;return nil end
local base=Image(256,192)
for y=0,191 do for x=0,255 do local c=(math.floor(x/16)+math.floor(y/16))%2==0 and {39,47,58} or {45,54,65};base:drawPixel(x,y,pc.rgba(c[1],c[2],c[3],255))end end
local timeline={}
for i=1,#times-1 do
 local t=times[i];local im=base:clone();local bn=bodyAt(t)
 im:drawImage(body[bn],Point(64,64))
 local visible=0
 for _,ev in ipairs(events)do
  local age=t-ev.ms
  if age>=0 and age<850 then
   local phase=effectAt(age);local n=phase+(ev.kind=="note" and 8 or 0)
   local anchor=args.bodyFrames[bodyAt(ev.ms)].anchor
   local offset=ev.kind=="heart" and -14 or 14
   local rise=math.floor(18*age/850)
   local dx=64+(anchor.x+offset-16)*2
   local dy=64+(anchor.y-10-16-rise)*2
   im:drawImage(scale(effects[n],2),Point(dx,dy));visible=visible+1
  end
 end
 im:saveAs(review.."composite-frames/frame-"..string.format("%03d",i-1)..".png")
 table.insert(timeline,{file="composite-frames/frame-"..string.format("%03d",i-1)..".png",timeMs=t,durationMs=times[i+1]-t,bodyFrame=bn,instances=visible})
end
-- Check effect pixels against Face for all body poses at spawn; no parent flip applied to glyphs.
local bodySource=Sprite{fromFile="C:/Dev/scp_desktop/Art/assets/characters/Scp999/revisions/r004/source/Scp999-lighting.aseprite"}
local faceOverlap=0
for n=1,50 do
 local face=bodySource.layers[3]:cel(n);local anchor=args.bodyFrames[n].anchor
 for _,kind in ipairs({"heart","note"})do
  local start=kind=="heart" and 1 or 9;local ox=kind=="heart" and -14 or 14
  for phase=start,start+7 do
   for y=0,31 do for x=0,31 do
    if pc.rgbaA(effects[phase]:getPixel(x,y))>0 then
     local fx=anchor.x+ox-16+x-face.position.x
     local fy=anchor.y-10-16+y-face.position.y
     if fx>=0 and fy>=0 and fx<face.image.width and fy<face.image.height and pc.rgbaA(face.image:getPixel(fx,fy))>0 then faceOverlap=faceOverlap+1 end
    end
   end end
  end
 end
end
bodySource:close();app.activeSprite=s;app.activeFrame=s.frames[4];app.activeLayer=s.layers[1];app.refresh()
return {timeline=timeline,durationMs=2600,events=events,faceOverlapPixelsAtSpawn=faceOverlap,bodyReference="Scp999 r004; PLAN-005 run-010 real Godot captures",scope="Aseprite compositing review, not Godot VFX integration. Rise and spawn timings are illustrative.",scales={1,2},backgrounds={"light","dark","pattern"},dayNight=true}
