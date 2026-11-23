local s=sprite
assert(s.filename:lower():find("scp_desktop"),"Wrong workspace")
local rgba=app.pixelColor.rgba
local C={
 outline=rgba(67,34,39,255),base=rgba(247,138,33,211),
 shade=rgba(219,91,25,216),light=rgba(255,178,57,224),
 rim=rgba(255,202,89,225),cream=rgba(255,235,171,245),
 eye=rgba(63,32,39,255),eyeLight=rgba(255,247,207,255),
 cheek=rgba(255,121,69,240),mouth=rgba(113,39,44,255),tongue=rgba(255,157,115,255)
}
local shape={{12,50},{13,47},{10,45},{8,42},{8,38},{10,36},{12,37},{15,40},{15,34},{17,28},{20,24},{24,22},{29,21},{35,21},{40,23},{44,27},{47,33},{48,39},{51,35},{54,34},{56,36},{56,40},{54,43},{51,46},{54,50},{53,53},{49,55},{43,56},{38,54},{34,55},{30,56},{25,55},{20,56},{15,55},{12,53}}
local function inside(x,y,poly)
 local yes=false
 local j=#poly
 for i=1,#poly do
  local a,b=poly[i],poly[j]
  if ((a[2]>y)~=(b[2]>y)) and x<(b[1]-a[1])*(y-a[2])/(b[2]-a[2])+a[1] then yes=not yes end
  j=i
 end
 return yes
end
local function rect(x,y,l,t,r,b) return x>=l and x<=r and y>=t and y<=b end
local function dotline(x,y,points)
 for _,p in ipairs(points) do if math.abs(x-p[1])<0.6 and math.abs(y-p[2])<0.6 then return true end end
 return false
end
local function paint(p)
 local sx,sy,lean,dy=p[1],p[2],p[3],p[4]
 local expression=p[5]
 local imgs={Image(64,64),Image(64,64),Image(64,64),Image(64,64)}
 local poly={}
 for _,v in ipairs(shape) do
  local armOffset=0
  if v[1]<=12 and v[2]<47 then armOffset=(expression=="happy" and -2 or 0)+lean*0.25 end
  if v[1]>=51 and v[2]<47 then armOffset=(expression=="happy" and -3 or 0)-lean*0.25 end
  local drag=(p[8] or 0)*math.max(0,math.min(1,(v[2]-47)/8))*math.max(0,math.min(1,(24-v[1])/12))
  table.insert(poly,{32+(v[1]-drag-32)*sx+lean*(56-v[2])/34,56+(v[2]-56)*sy+dy+armOffset})
 end
 local function inmask(x,y) return inside(x+0.5,y+0.5,poly) end
 for y=0,63 do for x=0,63 do
  if inmask(x,y) then
   local v=56+(y+0.5-56-dy)/sy
   local u=32+(x+0.5-32-lean*(56-v)/34)/sx
   local edge=not inmask(x-1,y) or not inmask(x+1,y) or not inmask(x,y-1) or not inmask(x,y+1)
   if edge then imgs[2]:drawPixel(x,y,C.outline)
   else
    local color=C.base
    if v<31 and u<41 then color=C.light end
    if v>49+(u%7)*0.13 or (u>45 and v>37) then color=C.shade end
    if v>52 and u>19 and u<41 then color=C.light end
    imgs[1]:drawPixel(x,y,color)
    -- Short, intentionally stepped material reflections, not a directional cast shadow.
    if rect(u,v,23,26,29,27) or rect(u,v,20,28,23,31) or rect(u,v,18,33,19,37) then imgs[4]:drawPixel(x,y,C.cream)
    elseif rect(u,v,26,24,30,25) or rect(u,v,22,28,26,30) or rect(u,v,42,30,43,33)
       or rect(u,v,46,43,47,46) or rect(u,v,19,49,23,50) then imgs[4]:drawPixel(x,y,C.rim) end
    -- Tiny suspended jelly bubbles.
    if ((u-43)^2+(v-47)^2<3 and (u-43)^2+(v-47)^2>0.9) or rect(u,v,22,46,23,47) then imgs[4]:drawPixel(x,y,C.rim) end
    local fx=u-(p[7] or 0)
    local fy=v
    local face=nil
    if rect(fx,fy,21,41,24,42) or rect(fx,fy,40,41,43,42) then face=C.cheek end
    if expression=="happy" then
      if dotline(fx,fy,{{25,38},{26,37},{27,37},{28,38},{37,38},{38,37},{39,37},{40,38}}) then face=C.eye end
    elseif expression=="blink" then
      if rect(fx,fy,25,38,28,39) or rect(fx,fy,37,38,40,39) then face=C.eye end
    else
      if rect(fx,fy,25,35,28,39) or rect(fx,fy,37,35,40,39) then
        face=C.eye
        if rect(fx,fy,25,35,26,36) or rect(fx,fy,37,35,38,36) then face=C.eyeLight end
      end
      if expression=="worry" and (dotline(fx,fy,{{24,33},{25,32},{26,32},{39,32},{40,32},{41,33}})) then face=C.eye end
    end
    if expression=="happy" then
      if rect(fx,fy,30,42,35,44) or rect(fx,fy,31,45,34,46) then face=C.mouth end
      if rect(fx,fy,31,45,34,46) then face=C.tongue end
    elseif expression=="worry" then
      if rect(fx,fy,31,43,34,44) then face=C.mouth end
    else
      if dotline(fx,fy,{{30,42},{31,43},{32,43},{33,43},{34,43},{35,42}}) then face=C.eye end
    end
    if face then imgs[3]:drawPixel(x,y,face) end
   end
  end
 end end
 return imgs,poly
end
local groups={
 {name="idle",loop=true,poses={
 {1,1,0,0,"normal",220},{1.02,.98,0,0,"normal",180},{1.03,.97,0,0,"blink",100},
 {1.02,.98,0,0,"normal",160},{.98,1.02,0,0,"normal",180},{.99,1.01,0,0,"normal",200}}},
 {name="move",loop=true,poses={
 {1.02,.98,2,0,"normal",120,1,3},{1.04,.96,3,0,"normal",120,1,6},
 {1.06,.94,4,0,"normal",100,2,9},{1.04,.96,4,0,"normal",100,2,9},
 {1.02,.98,3,0,"normal",140,1,6},{1,1,1,0,"normal",160,1,3}}},
}
if args.full then
 local more={
 {name="typing_react",loop=false,poses={
 {1.10,.88,0,0,"happy",90},{.91,1.12,0,-2,"happy",80},{.94,1.05,1,-6,"happy",100},
 {1,1,0,-4,"happy",90},{1.12,.88,0,0,"happy",90},{1.03,.97,0,0,"normal",110},{1,1,0,0,"normal",140}}},
 {name="pet",loop=true,poses={
 {1.07,.92,-2,0,"happy",130},{1.15,.82,-3,0,"happy",140},{1.17,.8,0,0,"happy",140},
 {1.15,.82,3,0,"happy",140},{1.07,.92,2,0,"happy",130},{1.02,.97,0,0,"happy",140}}},
 {name="pet_recover",loop=false,poses={
 {1.07,.92,0,0,"happy",100},{.96,1.04,0,0,"happy",110},{1,1,0,0,"normal",170}}},
 {name="takeoff",loop=false,poses={
 {1,1,0,0,"normal",90},{1.15,.82,1,0,"normal",110},{1.05,.92,2,0,"normal",70},{.85,1.20,2,0,"happy",80}}},
 {name="fall",loop=true,poses={
 {.88,1.16,0,0,"worry",140},{.90,1.14,-1,0,"worry",140},{.88,1.16,0,0,"worry",140},{.90,1.14,1,0,"worry",140}}},
 {name="land",loop=false,poses={
 {.88,1.16,0,0,"worry",60},{1.2,.78,0,0,"blink",80},{1.14,.84,0,0,"happy",100},{.96,1.04,0,0,"normal",100},{1,1,0,0,"normal",160}}},
 {name="balance",loop=true,poses={
 {1.02,.98,-4,0,"worry",120},{.98,1.02,-6,0,"worry",140},{1.04,.96,0,0,"worry",120},
 {.98,1.02,6,0,"worry",140},{1.02,.98,4,0,"worry",120},{1,1,0,0,"normal",150}}},
 {name="balance_recover",loop=false,poses={
 {1.04,.96,0,0,"worry",100},{.98,1.02,0,0,"normal",120},{1,1,0,0,"normal",160}}}
 }
 for _,g in ipairs(more) do table.insert(groups,g) end
end
local meta={frames={},tags={},palette={},pivot={x=32,y=56},canvas={width=64,height=64}}
app.transaction("ART-001 sculpted jelly poses",function()
 while #s.frames>1 do s:deleteFrame(#s.frames) end
 for i=#s.tags,1,-1 do s:deleteTag(s.tags[i]) end
 while #s.layers>1 do s:deleteLayer(s.layers[#s.layers]) end
 local layers={s.layers[1],s:newLayer(),s:newLayer(),s:newLayer()}
 local names={"Body","Outline","Face","Highlights"}
 for i,l in ipairs(layers) do l.name=names[i]; l.blendMode=BlendMode.NORMAL end
 local n=0
 for _,g in ipairs(groups) do
  local first=n+1
  for _,p in ipairs(g.poses) do
   n=n+1
   if n>1 then s:newEmptyFrame(n) end
   s.frames[n].duration=p[6]/1000
   local imgs=paint(p)
   for k,l in ipairs(layers) do s:newCel(l,n,imgs[k],Point(0,0)) end
   table.insert(meta.frames,{index=n-1,asepriteFrame=n,tag=g.name,durationMs=p[6],
     reactionAnchor={x=math.floor(32+p[3]+0.5),y=math.floor(56-38*p[2]+p[4]-4)},
     trailAnchor={x=32,y=56},pivot={x=32,y=56}})
  end
  table.insert(meta.tags,{name=g.name,from=first-1,to=n-1,loop=g.loop})
 end
 for _,g in ipairs(meta.tags) do
  local tag=s:newTag(g.from+1,g.to+1)
  tag.name=g.name; tag.repeats=g.loop and 0 or 1; tag.color=Color{r=244,g=155,b=63}
 end
 s.data="ART-001 / Scp999 / r001 draft / unapproved; pivot 32,56; no runtime lighting baked"
end)
s:saveAs("C:/Dev/scp_desktop/Art/assets/characters/Scp999/source/Scp999.aseprite")
app.activeSprite=s
app.activeFrame=s.frames[1]
if app.editor then app.editor.zoom=8 end
app.refresh()
return meta
