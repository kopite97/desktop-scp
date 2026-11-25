-- ART-006: hand-authored pixel clusters, independent foot layers, generic review fixtures.
local root='C:/Dev/scp_desktop/Art/assets/characters/Scp0492/'
local run='C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-006/run-001/'
local pc=app.pixelColor
local function rgba(r,g,b,a)return pc.rgba(r,g,b,a or 255)end
local C={outline=rgba(39,32,51),shadow=rgba(88,62,107),green=rgba(118,164,103),light=rgba(180,213,136),mid=rgba(149,190,116)}
local function write(path,value)local f=assert(io.open(path,'w'));f:write(json.encode(value));f:close()end
local function foot(dx,lift,alpha,rear,kind)
 local im=Image(32,32);local fill={}
 local function p(x,y,c)if x>=0 and x<32 and y>=0 and y<32 then fill[y*32+x]=c end end
 if alpha==0 then return im end
 p(16,27,C.shadow);p(16,28,C.shadow)
 if kind=='fall' then
  for y=28,30 do p(16+dx,y,C.green);p(17+dx,y,C.mid)end
  p(18+dx,30,C.green)
 elseif kind=='land' then
  p(16,29,C.shadow)
  for x=13,21 do p(x,30,x<16 and C.shadow or C.green)end
  p(16,29,C.light);p(17,29,C.mid)
 else
  for x=15,18 do p(x+dx,29-lift,x==16 and C.light or C.green)end
  for x=14,20 do p(x+dx,30-lift,x<=15 and C.shadow or (x<=18 and C.mid or C.green))end
 end
 for k,v in pairs(fill)do
  local x=k%32;local y=math.floor(k/32)
  for _,d in ipairs({{-1,0},{1,0},{0,-1},{0,1}})do local nx=x+d[1];local ny=y+d[2];if nx>=0 and nx<32 and ny>=0 and ny<32 and not fill[ny*32+nx]then im:drawPixel(nx,ny,C.outline)end end
 end
 for k,v in pairs(fill)do
  if rear then if v==C.light then v=C.mid elseif v==C.mid then v=C.green elseif v==C.green then v=rgba(104,143,98)end end
  im:drawPixel(k%32,math.floor(k/32),v)
 end
 if alpha~=255 then for it in im:pixels()do local v=it();if pc.rgbaA(v)>0 then it(rgba(pc.rgbaR(v),pc.rgbaG(v),pc.rgbaB(v),alpha))end end end
 return im
end
-- Samples are reviewed before expanding the approved design into the full timeline.
if args and args.sample then
 local s=Sprite(160,48,ColorMode.RGB);s.layers[1].name='Representative feet (roots separated for review only)'
 local im=Image(160,48)
 local samples={{0,0,'idle'},{1,1,'move'},{0,0,'fall'},{0,0,'land'}}
 for i,q in ipairs(samples)do local l=foot(q[1],q[2],255,true,q[3]);local r=foot(-q[1],0,255,false,q[3]);im:drawImage(l,Point((i-1)*40-2,0));im:drawImage(r,Point((i-1)*40+8,0))end
 s:newCel(s.layers[1],1,im);app.activeSprite=s;app.editor.zoom=6;app.refresh()
 _G.art006Sample=s
 return {sample=true,sprite=s.id}
end
for _,d in ipairs({'source','revisions/r001/source','revisions/r001/export','revisions/r001/review'})do app.fs.makeAllDirectories(root..d)end
local tags={
 {name='spawn',ms={100,120,120,160},loop=false,alpha={0,110,200,255}},
 {name='idle',ms={300,300,300,300},loop=true},
 {name='move',ms={100,100,120,100,100,100,120,100},loop=true},
 {name='fall',ms={160,160},loop=true},
 {name='land',ms={90,110,140},loop=false},
 {name='collect',ms={80,80,100,100},loop=false,alpha={255,170,75,0},body={1,.65,.25,0}},
 {name='expire',ms={120,120,140,140,140},loop=false,alpha={255,200,120,45,0},body={1,.8,.45,.15,0}}
}
local frames={};local left={};local right={};local n=0
for _,t in ipairs(tags)do
 t.from=n+1;t.totalMs=0
 for j,ms in ipairs(t.ms)do
  n=n+1;t.totalMs=t.totalMs+ms
  local dx,lift,rdx,rlift=0,0,0,0;local kind=t.name
  if kind=='move'then
   local q={{-1,0,1,1},{0,0,1,1},{1,0,0,1},{1,0,-1,0},{1,1,-1,0},{1,1,0,0},{0,1,1,0},{-1,0,1,0}}
   dx,lift,rdx,rlift=table.unpack(q[j])
  elseif kind=='spawn'then lift=(j==2 and 1 or 0);rlift=lift
  elseif kind=='fall'then dx=j-1;rdx=1-j
  elseif kind=='land'and j>1 then kind='idle'
  elseif kind=='collect'then lift=math.min(2,j-1);rlift=lift
  end
  left[n]=foot(dx,lift,t.alpha and t.alpha[j]or 255,true,kind)
  right[n]=foot(rdx,rlift,t.alpha and t.alpha[j]or 255,false,kind)
  frames[n]={index=n,index0=n-1,tag=t.name,durationMs=ms,rect={x=((n-1)%6)*32,y=math.floor((n-1)/6)*32,w=32,h=32},bodyAlpha=t.body and t.body[j]or 1}
 end
 t.to=n
end
assert(n==30)
local s=Sprite(32,32,ColorMode.RGB);s:assignColorSpace(ColorSpace{sRGB=true})
local ll=s.layers[1];ll.name='LeftFoot';local rr=s:newLayer();rr.name='RightFoot'
for f=1,n do if f>1 then s:newEmptyFrame(f)end;s.frames[f].duration=frames[f].durationMs/1000;s:newCel(ll,f,left[f]);s:newCel(rr,f,right[f])end
for _,t in ipairs(tags)do local tag=s:newTag(t.from,t.to);tag.name=t.name;tag.repeats=t.loop and 0 or 1 end
s.data='ART-006 r001. Separate SLOT layers share root (16,28); combined source canvas is not the icon composite. Icon body belongs to runtime.'
local palette=Palette(7);palette:setColor(0,Color{r=0,g=0,b=0,a=0});local ci=1
for _,key in ipairs({'outline','shadow','green','light','mid'})do local v=C[key];palette:setColor(ci,Color{r=pc.rgbaR(v),g=pc.rgbaG(v),b=pc.rgbaB(v)});ci=ci+1 end
palette:setColor(6,Color{r=104,g=143,b=98});s:setPalette(palette)
s:saveAs(root..'source/Scp0492.aseprite');s:saveCopyAs(root..'revisions/r001/source/Scp0492.aseprite')
local bounds={};local function sheet(list,name)
 local im=Image(192,160);local minx,miny,maxx,maxy=32,32,-1,-1
 for f,frame in ipairs(list)do im:drawImage(frame,Point(frames[f].rect.x,frames[f].rect.y));for it in frame:pixels()do if pc.rgbaA(it())>0 then minx=math.min(minx,it.x);miny=math.min(miny,it.y);maxx=math.max(maxx,it.x);maxy=math.max(maxy,it.y)end end end
 im:saveAs(root..'revisions/r001/export/Scp0492_'..name..'.png');bounds[name]={x=minx,y=miny,w=maxx-minx+1,h=maxy-miny+1}
end
sheet(left,'left-foot');sheet(right,'right-foot')
local contract={schemaVersion=1,assetId='Scp0492',revision=1,source='Scp0492.aseprite',colorMode='RGBA',colorSpace='sRGB',alpha='straight',material='Unshaded/Mix',cell={w=32,h=32},sheet={columns=6,rows=5,w=192,h=160},root={x=16,y=28},displayScale=2,frameIndex='frames.index 1-based; index0 0-based',files={left='Scp0492_left-foot.png',right='Scp0492_right-foot.png'},alphaBounds=bounds,tags=tags,frames=frames,direction={default='right',left='mirror each foot around root x=16; destination x=2*16-x (not 31-x); never mirror icon; preserve timeline phase'},attachment={iconAlphaThreshold=1,hitAlphaThreshold=16,iconVisibleBottomExclusive=56,iconVisibleCenterX=32,preferredHalfGap='clamp(round(visibleWidth * 0.18), 3, 9)',supportRows={52,53,54,55},supportRule='For each slot, choose occupied x in rows52..55 nearest preferred root, ties toward center. Separate roots by >=2px when possible; one-column support shares root. No support means skip feet for fully transparent icon.',layerOrder={'LeftFoot','RightFoot','icon'},rootY=56},lifecycle={spawnMs=500,collectMs=360,expireMs=660,lastDisappearFrameTransparent=true,bodyFade='piecewise constant frames.bodyAlpha, engine applies to icon only; feet alpha is baked',hide='pause presentation time',shutdown='immediate cleanup',logic='game owns movement/success/deletion; disappearance snapshot has no input or game identity'}}
write(root..'revisions/r001/export/Scp0492.contract.json',contract)
_G.art006={root=root,run=run,source=s,left=left,right=right,frames=frames,tags=tags,contract=contract}
app.activeSprite=s;app.activeLayer=rr;app.activeFrame=s.frames[10];app.editor.zoom=12;app.refresh()
return {frames=n,tags=#tags,bounds=bounds,source=s.filename}
