-- ART-007 native pixel art. Run inline through the Aseprite Live tool.
local pc=app.pixelColor;local root='C:/Dev/scp_desktop/Art/assets/vfx/Scp049Mutation/'
local run='C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-007/run-001/'
local function rgba(r,g,b,a)return pc.rgba(r,g,b,a or 255)end
local colors={outline={39,32,51},purple={151,119,184},lilac={196,161,214},green={160,200,117},light={216,236,161}}
local function protected(x,y)return x>=12 and x<=35 and y>=12 and y<=35 end
local function frame(kind,n)
 local fill={};local sparks={};local alpha=kind=='treat'and 196 or ({210,235,220,190,145,95,40,0})[n]
 local function p(target,x,y,key,a)
  x=math.floor(x+.5);y=math.floor(y+.5)
  if x>=2 and x<=45 and y>=2 and y<=45 and not protected(x,y)then local c=colors[key];target[y*48+x]=rgba(c[1],c[2],c[3],a or alpha)end
 end
 local function arc(r,from,to,key)
  for angle=from,to,1 do local rad=angle*math.pi/180;p(fill,23.5+r*math.cos(rad),23.5+r*math.sin(rad),key)end
 end
 if alpha>0 then
  if kind=='treat'then
   local shift=8*math.sin((n-1)*math.pi/4)
   local radius=18.5+.5*math.cos((n-1)*math.pi/4)
   arc(radius,-28+shift,30+shift,'purple');arc(radius,152+shift,210+shift,'purple')
   arc(radius,25+shift,30+shift,'lilac');arc(radius,205+shift,210+shift,'lilac')
   for j=0,3 do
    local q=(n-1+j*2)%8;local x=(j%2==0 and 5 or 42)+(q%3==0 and 1 or 0);local y=39-q*4
    p(sparks,x,y,'green',170+q*8);p(sparks,x,y-1,'light',150+q*8)
   end
  else
   local r=({18.5,19,19.5,20,20.5,21,21,21})[n];local rotation=(n-1)*7
   local length=({40,45,40,34,26,18,8,0})[n]
   for j=0,3 do arc(r,rotation+j*90-length/2,rotation+j*90+length/2,n<=2 and 'lilac'or'green')end
   for j=0,3 do local angle=(45+j*90+rotation)*math.pi/180;local x=23.5+21*math.cos(angle);local y=23.5+21*math.sin(angle)
    p(sparks,x,y,'light',alpha);if n<=4 then p(sparks,x+1,y,'green',alpha);p(sparks,x,y-1,'light',alpha)end
   end
  end
 end
 local function outlined(pixels)
  local im=Image(48,48)
  for k,v in pairs(pixels)do local x=k%48;local y=math.floor(k/48)
   for _,d in ipairs({{-1,0},{1,0},{0,-1},{0,1}})do local nx=x+d[1];local ny=y+d[2];if nx>=1 and nx<=46 and ny>=1 and ny<=46 and not protected(nx,ny)and not pixels[ny*48+nx]then local old=im:getPixel(nx,ny);local a=math.max(pc.rgbaA(old),math.floor(pc.rgbaA(v)*.72+.5));im:drawPixel(nx,ny,rgba(39,32,51,a))end end
  end
  for k,v in pairs(pixels)do im:drawPixel(k%48,math.floor(k/48),v)end
  return im
 end
 return outlined(fill),outlined(sparks)
end
if args and args.sample then
 local s=Sprite(48,48,ColorMode.RGB);s.layers[1].name='Representative aura sample'
 for i,q in ipairs({{'treat',1},{'treat',5},{'transform',3},{'transform',6}})do if i>1 then s:newEmptyFrame(i)end;local halo,motes=frame(q[1],q[2]);halo:drawImage(motes);s:newCel(s.layers[1],i,halo);s.frames[i].duration=.2 end
 _G.art007Sample=s;app.activeSprite=s;app.editor.zoom=4;app.refresh();return {sample=true,id=s.id}
end
for _,d in ipairs({'source','revisions/r001/source','revisions/r001/export','revisions/r001/review'})do app.fs.makeAllDirectories(root..d)end
local s=Sprite(48,48,ColorMode.RGB);s:assignColorSpace(ColorSpace{sRGB=true});local aura=s.layers[1];aura.name='Aura';local motes=s:newLayer();motes.name='Particles'
local frames={};local combined={}
for n=1,16 do
 if n>1 then s:newEmptyFrame(n)end;local kind=n<=8 and 'treat'or'transform';local index=n<=8 and n or n-8;local ms=n<=8 and 120 or 100
 local im,particles=frame(kind,index);s:newCel(aura,n,im);s:newCel(motes,n,particles);s.frames[n].duration=ms/1000
 local full=Image(im);full:drawImage(particles);combined[n]=full;frames[n]={index=n,index0=n-1,tag=kind,durationMs=ms,rect={x=((n-1)%4)*48,y=math.floor((n-1)/4)*48,w=48,h=48}}
end
local t=s:newTag(1,8);t.name='treat';t.repeats=0;local x=s:newTag(9,16);x.name='transform';x.repeats=1
local pal=Palette(6);pal:setColor(0,Color{r=0,g=0,b=0,a=0});for i,key in ipairs({'outline','purple','lilac','green','light'})do local c=colors[key];pal:setColor(i,Color{r=c[1],g=c[2],b=c[3]})end;s:setPalette(pal)
s.data='ART-007 r001. 48x48 decorative VFX only. Pivot24,40; 96x96 at2x. Clear center12..35. No icon, feet or doctor tools baked in.'
s:saveAs(root..'source/Scp049Mutation.aseprite');s:saveCopyAs(root..'revisions/r001/source/Scp049Mutation.aseprite')
local sheet=Image(192,192);local minx,miny,maxx,maxy=48,48,-1,-1
for n,im in ipairs(combined)do sheet:drawImage(im,Point(frames[n].rect.x,frames[n].rect.y));for it in im:pixels()do if pc.rgbaA(it())>0 then minx=math.min(minx,it.x);miny=math.min(miny,it.y);maxx=math.max(maxx,it.x);maxy=math.max(maxy,it.y)end end end
sheet:saveAs(root..'revisions/r001/export/Scp049Mutation.png')
local contract={schemaVersion=1,assetId='Scp049Mutation',revision=1,colorMode='RGBA',colorSpace='sRGB',alpha='straight',material='Unshaded/Mix',file='Scp049Mutation.png',cell={w=48,h=48},sheet={w=192,h=192,columns=4,rows=4},displayScale=2,pivot={x=24,y=40},alphaBounds={x=minx,y=miny,w=maxx-minx+1,h=maxy-miny+1},clearCenter={x=12,y=12,w=24,h=24},frameIndex='index is1-based; index0 is0-based',frames=frames,tags={{name='treat',from=1,to=8,loop=true,totalMs=960},{name='transform',from=9,to=16,loop=false,totalMs=800}},attachment={window={w=96,h=96},position='target.foot - (48,80) in physical pixels',layer='above icon; center remains transparent',flip='never flip for target facing; effect rotation independent of foot direction',hitTest='entire window cross-process click-through',bounds='clip at monitor work area; never offset target or hit geometry to keep VFX visible'},lifecycle={treat='loop at120ms/frame while Treating; game owns completion and cancellation; speed unchanged at overload1.5',success='transform and ART-006 spawn start at same logical success event',transformMs=800,transparentTailMs=100,alpha='baked; do not multiply extra fade',cancel='target removal, collection or expiry immediately removes all related VFX',lockdown='hide and freeze clock; restore only valid effects without catch-up',shutdown='immediately release all effects',stack='shared OverlayStackCoordinator; no independent repeated raise'}}
local f=assert(io.open(root..'revisions/r001/export/Scp049Mutation.contract.json','w'));f:write(json.encode(contract));f:close()
_G.art007={root=root,run=run,source=s,frames=frames,combined=combined,contract=contract}
app.activeSprite=s;app.activeFrame=s.frames[3];app.editor.zoom=4;app.refresh()
return {frames=16,tags=2,bounds=contract.alphaBounds,source=s.filename}
