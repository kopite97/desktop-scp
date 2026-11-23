local root="C:/Dev/scp_desktop/Art/assets/characters/Scp999/"
local active=sprite
local s=Sprite{fromFile=root.."revisions/r004/source/Scp999-lighting.aseprite"}
local expected={220,180,100,160,180,200,120,120,100,100,140,160,90,80,100,90,90,110,140,130,140,140,140,130,140,100,110,170,90,110,70,80,140,140,140,140,60,80,100,100,160,120,140,120,140,120,150,100,120,160}
assert(s.width==64 and s.height==64 and #s.frames==50 and #s.layers==6 and #s.tags==10)
local out={status="PASS",frames=#s.frames,layers=#s.layers,tags={}}
for i,f in ipairs(s.frames) do assert(math.floor(f.duration*1000+.5)==expected[i]) end
for _,t in ipairs(s.tags) do table.insert(out.tags,{name=t.name,from=t.fromFrame.frameNumber-1,to=t.toFrame.frameNumber-1,repeats=t.repeats}) end
for _,name in ipairs({"color","normal","specular"}) do local im=Image{fromFile=root.."revisions/r004/export/Scp999_"..name..".png"};assert(im.width==3200 and im.height==64) end
s:close()
app.activeSprite=active;app.activeFrame=active.frames[1];app.activeLayer=active.layers[1];app.refresh()
return out
