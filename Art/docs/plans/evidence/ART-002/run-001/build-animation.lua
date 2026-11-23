local root="C:/Dev/scp_desktop/Art/assets/vfx/Scp999Reaction/"
local pc=app.pixelColor
local patterns={
heart={"...###.###...","..#########..",".###########.","#############","#############",".###########.","..#########..","...#######...","....#####....",".....###.....","......#......"},
note={".......####","...########","...########","...##....##","...##....##","...##....##","...##....##","...##....##","...##....##","...##..####","...##.#####",".####..###.","#####......","#####......",".###......."}
}
local palette={pc.rgba(0,0,0,0),pc.rgba(81,43,63,255),pc.rgba(239,112,147,255),pc.rgba(255,191,200,255),pc.rgba(36,65,76,255),pc.rgba(95,206,195,255),pc.rgba(178,244,223,255)}
local function draw(kind,scaleX,scaleY,alpha)
 local fill,outline=Image(32,32),Image(32,32)
 local pattern=patterns[kind];local w,h=#pattern[1],#pattern
 local mask={}
 local function has(x,y)return mask[y*32+x]==true end
 for y=0,31 do for x=0,31 do
  local sx=math.floor((x-16)/scaleX+w/2)
  local sy=math.floor((y-16)/scaleY+h/2)
  if sx>=0 and sx<w and sy>=0 and sy<h and pattern[sy+1]:sub(sx+1,sx+1)=="#" then mask[y*32+x]=true end
 end end
 local c=kind=="heart" and {239,112,147} or {95,206,195}
 local dark=kind=="heart" and {81,43,63} or {36,65,76}
 local accent=kind=="heart" and {255,191,200} or {178,244,223}
 if alpha>0 then
 for y=1,30 do for x=1,30 do
  if has(x,y) then
   local rgb=c
   -- Small graphic accent, not a light-dependent shine.
   if kind=="heart" and y==13 and x>=12 and x<=14 then rgb=accent end
   if kind=="note" and y==11 and x>=15 and x<=19 then rgb=accent end
   fill:drawPixel(x,y,pc.rgba(rgb[1],rgb[2],rgb[3],alpha))
  elseif has(x-1,y) or has(x+1,y) or has(x,y-1) or has(x,y+1) then outline:drawPixel(x,y,pc.rgba(dark[1],dark[2],dark[3],alpha)) end
 end end end
 return fill,outline
end
local s=sprite
assert(s.filename:gsub("\\","/")==root.."source/Scp999Reaction.aseprite","Wrong sprite")
while #s.tags>0 do s:deleteTag(s.tags[#s.tags]) end
while #s.frames>1 do s:deleteFrame(s.frames[#s.frames]) end
for _,l in ipairs(s.layers)do local c=l:cel(1);if c then s:deleteCel(c)end end
local durations={70,80,100,160,160,120,100,60}
local scales={.55,.8,1.12,1,1,.85,.65,.5}
local alphas={160,230,255,255,230,150,80,0}
local sheet=Image(256,64)
local meta={assetId="Scp999Reaction",revision=1,bodyReference={assetId="Scp999",revision=4},
canvas={width=32,height=32},sheet={width=256,height=64,columns=8,rows=2,padding=0,spacing=0},
pivot={x=16,y=16},colorMode="RGBA",colorSpace="sRGB",alpha="straight",
lighting="unlit; no normal/specular/emission maps",frames={},tags={},
spawn={anchor="reactionAnchor snapshot at spawn",offsets={heart={x=-14,y=-10},note={x=14,y=-10}},units="source pixels; multiply by body integer scale",followBody=false,flipGlyph=false},
lifetimeMs=850,engineFade=false,engineOwns={"spawn frequency","event merging","concurrency limit","world-space rise","cleanup","input passthrough"}}
for kindIndex,kind in ipairs({"heart","note"})do
 for phase=1,8 do
  local n=(kindIndex-1)*8+phase
  if n>1 then s:newEmptyFrame(n) end
  s.frames[n].duration=durations[phase]/1000
  local fill,edge=draw(kind,scales[phase],scales[phase],alphas[phase])
  s:newCel(s.layers[kindIndex==1 and 1 or 3],n,fill,Point(0,0))
  s:newCel(s.layers[kindIndex==1 and 2 or 4],n,edge,Point(0,0))
  local im=Image(32,32);im:drawSprite(s,n)
  sheet:drawImage(im,Point((phase-1)*32,(kindIndex-1)*32))
  im:saveAs(root.."revisions/r001/review/frames/frame-"..string.format("%02d",n-1)..".png")
  table.insert(meta.frames,{index=n-1,asepriteFrame=n,tag=kind,durationMs=durations[phase],alpha=alphas[phase],frame={x=(phase-1)*32,y=(kindIndex-1)*32,w=32,h=32},pivot={x=16,y=16},phase=phase<=2 and "appear" or (phase<=5 and "accent" or "disappear")})
 end
end
for k,kind in ipairs({"heart","note"})do
 local tag=s:newTag((k-1)*8+1,k*8);tag.name=kind;tag.repeats=1
 table.insert(meta.tags,{name=kind,from=(k-1)*8,to=k*8-1,loop=false,repeats=1,direction="forward",durationMs=850,endState="transparent; remove instance"})
end
s.data="ART-002 Scp999Reaction r001. Unlit standalone VFX. No baked rise. 2 one-shot tags. Body reference Scp999 r004."
s:saveAs(s.filename)
s:saveCopyAs(root.."revisions/r001/source/Scp999Reaction.aseprite")
sheet:saveAs(root.."revisions/r001/export/Scp999Reaction.png")
app.activeSprite=s;app.activeFrame=s.frames[4];app.activeLayer=s.layers[1];app.editor.zoom=8;app.refresh()
return meta
