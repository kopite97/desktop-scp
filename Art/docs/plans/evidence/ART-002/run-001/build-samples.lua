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
local s=Sprite(32,32,ColorMode.RGB);s:assignColorSpace(ColorSpace{sRGB=true})
s.filename=root.."source/Scp999Reaction.aseprite"
local heart=s.layers[1];heart.name="HeartFill"
local he=s:newLayer();he.name="HeartOutline"
local note=s:newLayer();note.name="NoteFill"
local ne=s:newLayer();ne.name="NoteOutline"
local guide=s:newLayer();guide.name="Guide";guide.isVisible=false
local pal=Palette(#palette);for i,c in ipairs(palette)do pal:setColor(i-1,Color(c)) end;s:setPalette(pal)
for i,kind in ipairs({"heart","note"})do
 if i>1 then s:newEmptyFrame(i) end
 local fill,edge=draw(kind,1,1,255)
 s:newCel(kind=="heart" and heart or note,i,fill,Point(0,0))
 s:newCel(kind=="heart" and he or ne,i,edge,Point(0,0))
 s.frames[i].duration=.16
 local t=s:newTag(i,i);t.name=kind;t.repeats=1
end
s:saveAs(s.filename);app.activeSprite=s;app.activeFrame=s.frames[1]
app.editor.zoom=8;app.editor.scroll={x=16,y=16};app.refresh()
local sample=Image(64,32)
for i=1,2 do local im=Image(32,32);im:drawSprite(s,i);sample:drawImage(im,Point((i-1)*32,0))end
sample:saveAs("C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-002/run-001/samples.png")
return {file=s.filename,width=s.width,height=s.height,frames=#s.frames,paletteColors=#palette}
