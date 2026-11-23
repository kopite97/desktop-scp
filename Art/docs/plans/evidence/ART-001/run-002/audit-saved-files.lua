local root="C:/Dev/scp_desktop/Art/assets/characters/Scp999/"
local original=sprite
local s=Sprite{fromFile=root.."revisions/r002/source/Scp999-lighting-sample.aseprite"}
local r={width=s.width,height=s.height,frameCount=#s.frames,layerCount=#s.layers,colorMode=s.colorMode==ColorMode.RGB and "RGB" or "other",frames={},layers={},tags={},sheets={}}
assert(s.width==64 and s.height==64 and #s.frames==4 and #s.layers==6)
local durations={220,120,100,160}
for i,f in ipairs(s.frames) do local ms=math.floor(f.duration*1000+.5);assert(ms==durations[i]);r.frames[i]=ms end
for i,l in ipairs(s.layers) do r.layers[i]={name=l.name,visible=l.isVisible};assert(l.isVisible==(i<=3)) end
for i,t in ipairs(s.tags) do r.tags[i]={name=t.name,from=t.fromFrame.frameNumber,to=t.toFrame.frameNumber,repeats=t.repeats} end
assert(#s.tags==2 and s.tags[1].name=="idle_sample" and s.tags[2].name=="move_key_samples")
for _,name in ipairs({"color","normal","specular"}) do local im=Image{fromFile=root.."revisions/r002/export/Scp999_"..name..".png"};assert(im.width==256 and im.height==64);r.sheets[name]={width=im.width,height=im.height} end
s:close()
app.activeSprite=original;app.activeFrame=original.frames[1];app.activeLayer=original.layers[1];app.refresh()
r.status="PASS"
return r
