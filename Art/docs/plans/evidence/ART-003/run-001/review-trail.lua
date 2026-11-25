local root="C:/Dev/scp_desktop/Art/assets/vfx/Scp999Trail/"
local review=root.."revisions/r001/review/"
local run="C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-003/run-001/"
local engine="C:/Dev/scp_desktop/docs/plans/evidence/PLAN-005/run-014/"
local pc=app.pixelColor;local full=app.params.mode=="full"
local s=Sprite{fromFile=root.."source/Scp999Trail"..(full and "" or "-samples")..".aseprite"}
local function write(path,obj)local f=assert(io.open(path,"w"));f:write(json.encode(obj));f:close()end
local function scaled(im,k)local v=im:clone();v:resize(im.width*k,im.height*k);return v end
local function bg(w,h,kind)
 local im=Image(w,h)
 for y=0,h-1 do for x=0,w-1 do
  local rgb=kind==1 and {242,240,232}or kind==2 and {26,31,41}or ((math.floor(x/4)+math.floor(y/4))%2==0 and {191,173,145}or{125,153,163})
  im:drawPixel(x,y,pc.rgba(rgb[1],rgb[2],rgb[3],255))
 end end;return im
end
local frames={};for n=1,#s.frames do frames[n]=Image(32,16);frames[n]:drawSprite(s,n)end
local holds={frames[full and 3 or 1],frames[full and 12 or 2]}
local bodyBoard=Image{fromFile="C:/Dev/scp_desktop/docs/plans/evidence/PLAN-005/run-012/backgrounds.png"}
-- Four rows: day/unlit, night/unlit, day/engine-tint candidate, night/engine-tint candidate.
local comparison=Image(288,288)
for row=0,3 do for col=0,2 do
 local panel=bg(96,72,col+1);local night=row%2==1
 local body=Image(bodyBoard,Rectangle(col*128,(night and 1 or 0)*128,128,128));body:resize(64,64)
 panel:drawImage(body,Point(32,0))
 for k=1,2 do
  local effect=holds[k]:clone()
  if row==3 then for it in effect:pixels()do local p=it();it(pc.rgba(math.floor(pc.rgbaR(p)*.45+.5),math.floor(pc.rgbaG(p)*.50+.5),math.floor(pc.rgbaB(p)*.62+.5),pc.rgbaA(p)))end end
  panel:drawImage(effect,Point((k-1)*24,48))
 end
 comparison:drawImage(panel,Point(col*96,row*72))
end end
comparison:saveAs(run.."lighting-candidates-1x.png");scaled(comparison,2):saveAs(run.."lighting-candidates-2x.png")
local board=Image(288,144)
for row=0,1 do for col=0,2 do board:drawImage(Image(comparison,Rectangle(col*96,row*72,96,72)),Point(col*96,row*72))end end
if full then
 board:saveAs(review.."backgrounds-1x.png");scaled(board,2):saveAs(review.."backgrounds-2x.png")
 local contact=bg(288,32,3)
 for n,im in ipairs(frames)do contact:drawImage(im,Point((n-1)%9*32,math.floor((n-1)/9)*16))end
 scaled(contact,3):saveAs(review.."contact-sheet.png")
 local overlap=bg(288,48,2)
 for col=0,2 do
  local tile=bg(96,48,col+1)
  tile:drawImage(holds[1],Point(8,8));tile:drawImage(holds[2],Point(27,8));tile:drawImage(holds[1],Point(20,8));tile:drawImage(holds[2],Point(18,8))
  local flipped=holds[1]:clone();flipped:flip(FlipType.HORIZONTAL);tile:drawImage(flipped,Point(45,24))
  overlap:drawImage(tile,Point(col*96,0))
 end
 scaled(overlap,2):saveAs(review.."overlap-and-flip.png")
 -- Exact event boundaries plus 50ms movement samples: no timing quantization of VFX transitions.
 local durations={80,100,1800,140,140,140,120,100,80}
 local total=0;for _,ms in ipairs(durations)do total=total+ms end
 local events={{ms=0,x=44,kind=1},{ms=450,x=71,kind=2},{ms=900,x=98,kind=1},{ms=1350,x=125,kind=2},{ms=1800,x=152,kind=1}}
 local finish=5000;local bounds={[0]=true,[finish]=true}
 for t=0,finish,50 do bounds[t]=true end
 for _,ev in ipairs(events)do local t=ev.ms;bounds[t]=true;for _,d in ipairs(durations)do t=t+d;if t<finish then bounds[t]=true end end end
 local bf=assert(io.open("C:/Dev/scp_desktop/Art/assets/characters/Scp999/revisions/r005/export/Scp999.contract.json","r"));local bodyMeta=json.decode(bf:read("*a"));bf:close()
 local body={};local cycle=0
 for i=7,12 do body[i]=Image{fromFile=engine.."playback/frame-"..string.format("%02d",i-1)..".png"};cycle=cycle+bodyMeta.frames[i].durationMs end
 for start=0,2600,cycle do local t=start;for i=7,12 do t=t+bodyMeta.frames[i].durationMs;if t<2600 then bounds[t]=true end end end
 local idle=Image{fromFile=engine.."playback/frame-00.png"}
 local times={};for t,_ in pairs(bounds)do table.insert(times,t)end;table.sort(times)
 local function phase(age)local t=0;for n,d in ipairs(durations)do if age<t+d then return n end;t=t+d end;return nil end
 local function bodyAt(t)local age=t%cycle;for i=7,12 do if age<bodyMeta.frames[i].durationMs then return i end;age=age-bodyMeta.frames[i].durationMs end;return 7 end
 app.fs.makeAllDirectories(review.."composite-frames")
 local anim=Sprite(512,160,ColorMode.RGB);anim:assignColorSpace(ColorSpace{sRGB=true});anim.layers[1].name="CompositeReview"
 local timeline={}
 for i=1,#times-1 do
  local t=times[i];local im=scaled(bg(256,80,2),2);local visible=0
  for _,ev in ipairs(events)do local age=t-ev.ms;if age>=0 and age<total then
   local p=phase(age);im:drawImage(scaled(frames[(ev.kind-1)*9+p],2),Point((ev.x-16)*2,(60-8)*2));if p<9 then visible=visible+1 end
  end end
  local x=44+math.floor(math.min(t,2600)*.06);local bi=t<2600 and body[bodyAt(t)]or idle
  im:drawImage(bi,Point((x-32)*2,8))
  if i>1 then anim:newEmptyFrame(i)end
  anim.frames[i].duration=(times[i+1]-t)/1000;anim:newCel(anim.layers[1],i,im,Point(0,0))
  local file="composite-frames/frame-"..string.format("%03d",i-1)..".png";im:saveAs(review..file)
  table.insert(timeline,{file=file,timeMs=t,durationMs=times[i+1]-t,visibleTrails=visible})
 end
 anim.data="ART-003 composite review only; existing r005 Godot captures + new trail pixels. Demonstration hold 1800ms, spawn 450ms, speed .06 source px/ms; not product parameters."
 anim:saveAs(review.."trail-preview.aseprite")
 write(run.."composite-review.json",{timeline=timeline,durationMs=finish,demoHoldMs=1800,demoLifetimeMs=total,events=events,scope="Aseprite compositing; no product platform or engine integration",bodyReference="Scp999 r005 PLAN-005 run-014 Godot renders"})
 s:close();app.activeSprite=anim;app.activeFrame=anim.frames[1]
 if app.isUIAvailable then app.editor.zoom=2;app.refresh()else anim:close()end
else
 local display=Sprite(576,576,ColorMode.RGB);display.layers[1].name="LightingCandidates";display:newCel(display.layers[1],1,scaled(comparison,2),Point(0,0));display:saveAs(run.."lighting-candidates.aseprite")
 s:close();app.activeSprite=display;if app.isUIAvailable then app.editor.zoom=1;app.refresh()else display:close()end
end
write(run..(full and "review-build.json" or "sample-build.json"),{mode=full and "full" or "samples",passed=true,frames=#frames,version=tostring(app.version),api=app.apiVersion})
