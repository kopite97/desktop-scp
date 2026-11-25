-- ART-004: authored pixel clusters in editable Aseprite layers, not a resized concept image.
local root='C:/Dev/scp_desktop/Art/assets/characters/Scp049/'
local run='C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-004/run-001/'
local full=_G.art004Full==true or app.params.mode=='full'
for _,d in ipairs({'source','revisions/r001/source','revisions/r001/export','revisions/r001/review/frames'})do app.fs.makeAllDirectories(root..d)end
local pc=app.pixelColor
local hex={outline='14151d',ink='20212b',deep='292b38',cloth='373947',fold='494b5c',lit='5c5e70',edge='777688',seam='85808a',leather='302b30',leatherMid='484047',leatherLit='665963',boneShade='867b7b',boneMid='afa396',bone='d3c7b0',boneLight='eee4cc',ivory='fff2d8',eye='171a23',eyeEdge='565363',metal='a6a1a3',metalLight='e4dfd4',brass='b09b72',brassDark='74664f',glass='a6bcc0',fluid='6e9193'}
local C={};for k,v in pairs(hex)do C[k]=pc.rgba(tonumber(v:sub(1,2),16),tonumber(v:sub(3,4),16),tonumber(v:sub(5,6),16),255)end
local names={'Outline','Boots','Robe','Cape','Hood','Mask','Arms','BagTools'}
local function pixel(im,x,y,c) x=math.floor(x+.5);y=math.floor(y+.5);if x>=0 and x<64 and y>=0 and y<64 then im:drawPixel(x,y,C[c]or c)end end
local function line(im,x0,y0,x1,y1,c)
 local n=math.max(math.abs(x1-x0),math.abs(y1-y0));if n==0 then pixel(im,x0,y0,c);return end
 for i=0,n do local t=i/n;pixel(im,x0+(x1-x0)*t,y0+(y1-y0)*t,c)end
end
local function poly(im,pts,c,dx,dy)
 dx=dx or 0;dy=dy or 0
 for y=0,63 do local yy=y-dy+.5;local hits={}
  for i=1,#pts do local a,b=pts[i],pts[i%#pts+1];if(a[2]<=yy and b[2]>yy)or(b[2]<=yy and a[2]>yy)then hits[#hits+1]=a[1]+(yy-a[2])/(b[2]-a[2])*(b[1]-a[1])+dx end end
  table.sort(hits);for i=1,#hits-1,2 do for x=math.ceil(hits[i]-.5),math.floor(hits[i+1]-.5)do pixel(im,x,y,c)end end
 end
end
local clips={
 {name='idle',ms={300,160,300,160},loop=true},
 {name='move',ms={100,80,100,120,100,80,100,120},loop=true},
 {name='treat_start',ms={140,120,140,180}},
 {name='treat_loop',ms={160,120,180,160,120,180},loop=true},
 {name='treat_end',ms={130,130,160,180}},
 {name='takeoff',ms={100,80,120}},
 {name='fall',ms={140,120,140},loop=true},
 {name='land',ms={100,130,170}},
 {name='balance',ms={140,180,140,180},loop=true},
 {name='balance_recover',ms={120,140,180}}
}
local function pose(tag,i)
 local p={bob=0,lean=0,step=0,hem=0,arm=0,bag=0,tool=0,crouch=0,head=0}
 if tag=='idle'then p.head=({0,0,1,0})[i];p.arm=({0,0,1,0})[i]
 elseif tag=='move'then p.step=({-2,-1,0,1,2,1,0,-1})[i];p.bob=({0,-1,-1,0,0,-1,-1,0})[i];p.hem=p.step;p.bag=-math.floor(p.step/2);p.arm=-p.step
 elseif tag=='treat_start'then p.crouch=({0,1,2,2})[i];p.lean=({0,1,2,2})[i];p.arm=({0,2,4,6})[i];p.tool=i>=3 and 1 or 0;p.bag=1
 elseif tag=='treat_loop'then p.crouch=2;p.lean=2;p.arm=({6,7,5,6,7,5})[i];p.head=i%3==2 and 1 or 0;p.tool=i<=3 and 1 or 2;p.bag=1
 elseif tag=='treat_end'then p.crouch=({2,1,1,0})[i];p.lean=({2,1,0,0})[i];p.arm=({5,3,1,0})[i];p.tool=i==1 and 1 or 0;p.bag=1
 elseif tag=='takeoff'then p.crouch=({2,1,-2})[i];p.arm=({1,-1,-3})[i];p.hem=({0,1,2})[i]
 elseif tag=='fall'then p.bob=-2;p.arm=-3;p.hem=({-2,0,2})[i];p.head=i==2 and 1 or 0;p.bag=-2
 elseif tag=='land'then p.crouch=({3,1,0})[i];p.arm=({2,1,0})[i];p.hem=({2,1,0})[i]
 elseif tag=='balance'then p.lean=({-2,0,2,0})[i];p.arm=-2;p.bag=-p.lean;p.hem=p.lean
 elseif tag=='balance_recover'then p.lean=({2,1,0})[i];p.arm=({-2,-1,0})[i];p.head=i==2 and 1 or 0 end
 return p
end
local function draw(tag,i)
 local p=pose(tag,i);local L={};for _,n in ipairs(names)do L[n]=Image(64,64)end
 local by=p.bob+p.crouch;local hx=p.lean;local hy=by+p.head
 local function P(layer,pts,c,dx,dy)poly(L[layer],pts,c,dx,dy)end
 local function N(layer,x,y,xx,yy,c,dx,dy)line(L[layer],x+(dx or 0),y+(dy or 0),xx+(dx or 0),yy+(dy or 0),c)end
 -- Firm soles remain at y=55; world motion belongs to the engine.
 P('Boots',{{23,51},{29,51},{30,55},{21,55},{21,53}},'ink',p.step,0)
 P('Boots',{{33,51},{39,51},{42,54},{41,55},{33,55}},'leather',-p.step,0)
 N('Boots',22,53,28,53,'fold',p.step,0);N('Boots',34,53,39,53,'leatherLit',-p.step,0)
 -- Heavy split robe: broad folds, bright enough to read over dark desktops.
 P('Robe',{{22,29},{38,29},{42,38},{45+p.hem,51},{38,53},{31,51},{22,53},{18-p.hem,50}},'deep',0,by)
 P('Robe',{{24,33},{29,34},{27,50},{21,51}},'cloth',0,by)
 P('Robe',{{30,33},{35,33},{37,51},{31,49}},'fold',0,by)
 P('Robe',{{37,34},{39,37},{43+p.hem,50},{39,51}},'cloth',0,by)
 P('Robe',{{26,36},{27,38},{25,49},{23,50}},'fold',0,by)
 N('Robe',30,38,29,48,'ink',0,by);N('Robe',35,37,37,49,'lit',0,by)
 N('Robe',21,50,26,51,'edge',0,by);N('Robe',39,51,43+p.hem,50,'fold',0,by)
 N('Robe',25,35,37,35,'leather',0,by);N('Robe',26,36,37,36,'leatherMid',0,by)
 P('Robe',{{30,34},{34,34},{34,37},{30,37}},'brassDark',0,by);N('Robe',31,34,33,34,'brass',0,by);N('Robe',31,36,33,36,'brass',0,by)
 -- Two overlapping shoulder capes with a clear dark seam.
 P('Cape',{{23,25},{35,25},{40,28},{44,34},{38,36},{33,34},{27,36},{18,34},{20,29}},'ink',hx,by)
 P('Cape',{{22,27},{34,27},{38,29},{41,33},{36,33},{31,31},{27,34},{20,32}},'fold',hx,by)
 P('Cape',{{25,26},{32,26},{30,31},{23,31},{20,30}},'lit',hx,by)
 P('Cape',{{33,27},{37,28},{40,31},{35,31}},'cloth',hx,by)
 N('Cape',21,32,26,33,'edge',hx,by);N('Cape',34,32,40,33,'lit',hx,by)
 N('Cape',30,29,31,33,'ink',hx,by)
 -- Hood: large faceted silhouette and recessed face opening.
 P('Hood',{{18,12},{23,7},{32,5},{39,7},{44,12},{44,20},{41,27},{33,30},{23,28},{16,24},{16,18}},'deep',hx,hy)
 P('Hood',{{18,13},{24,8},{32,6},{37,8},{29,10},{24,15},{20,23},{17,22}},'fold',hx,hy)
 P('Hood',{{24,8},{32,6},{35,7},{27,10},{22,15}},'lit',hx,hy)
 P('Hood',{{32,10},{38,9},{42,13},{41,23},{35,27},{24,25},{24,18}},'ink',hx,hy)
 P('Hood',{{20,23},{25,25},{32,26},{30,28},{23,26}},'cloth',hx,hy)
 N('Hood',24,15,30,11,'edge',hx,hy);N('Hood',31,10,37,9,'lit',hx,hy)
 N('Hood',17,19,19,15,'lit',hx,hy);N('Hood',18,24,23,27,'fold',hx,hy)
 -- Ivory mask: cheek, eye socket, forehead ridge and hooked beak are separate clusters.
 P('Mask',{{30,13},{36,11},{41,12},{44,16},{42,20},{45,23},{46,28},{44,30},{40,25},{35,23},{29,22},{27,18}},'boneShade',hx,hy)
 P('Mask',{{30,14},{36,12},{40,13},{42,16},{39,19},{41,23},{43,27},{39,24},{34,22},{29,21},{28,18}},'bone',hx,hy)
 P('Mask',{{35,13},{38,13},{39,17},{38,20},{43,25},{44,28},{42,25},{36,21},{35,18}},'boneLight',hx,hy)
 P('Mask',{{39,13},{41,14},{43,16},{41,19},{40,21},{38,18}},'boneMid',hx,hy)
 N('Mask',36,13,37,17,'ivory',hx,hy);N('Mask',38,20,44,27,'ivory',hx,hy)
 P('Mask',{{29,16},{32,15},{35,16},{35,19},{32,20},{29,19}},'eyeEdge',hx,hy)
 P('Mask',{{29,16},{32,16},{34,17},{34,18},{31,19},{29,18}},'eye',hx,hy)
 P('Mask',{{40,16},{42,15},{42,18},{40,20}},'eye',hx,hy)
 N('Mask',29,20,33,21,'boneLight',hx,hy);pixel(L.Mask,36+hx,22+hy,'boneMid')
 N('Mask',41,25,44,29,'boneMid',hx,hy)
 -- Far arm holds the medical bag; near arm reaches for surgical treatment.
 P('Arms',{{20,32},{24,34},{23,41},{19,43},{17,40}},'deep',0,by-p.bag)
 N('Arms',19,34,18,38,'lit',0,by-p.bag);N('Arms',19,40,22,40,'fold',0,by-p.bag)
 P('Arms',{{18,41},{22,41},{23,44},{21,46},{18,45}},'leatherMid',0,by-p.bag)
 N('Arms',18,42,21,42,'leatherLit',0,by-p.bag);N('Arms',19,43,19,44,'ink',0,by-p.bag)
 if p.tool>0 then
  P('Arms',{{37,32},{41,33},{44,36},{43,39},{38,38},{35,35}},'cloth',hx,by)
  N('Arms',38,33,42,36,'lit',hx,by)
  P('Arms',{{41,36},{46,35},{49,36},{49,39},{44,40},{42,39}},'leatherMid',hx,by+(p.arm-6))
  N('Arms',44,36,47,36,'leatherLit',hx,by+(p.arm-6))
  local tx,ty=49+hx,38+by+(p.arm-6)
  if p.tool==1 then
   line(L.BagTools,tx-1,ty,tx+6,ty+2,'metal');line(L.BagTools,tx+3,ty+1,tx+6,ty+1,'metalLight');pixel(L.BagTools,tx+6,ty+3,'metal')
  else
   line(L.BagTools,tx,ty-3,tx+4,ty+1,'metal');line(L.BagTools,tx+1,ty-2,tx+4,ty+1,'glass');line(L.BagTools,tx+2,ty-1,tx+3,ty,'fluid');line(L.BagTools,tx+4,ty+1,tx+6,ty+3,'metalLight');line(L.BagTools,tx-1,ty-3,tx+1,ty-5,'metalLight')
  end
 else
  P('Arms',{{38,32},{41,34},{41,39},{37,41},{34,38},{35,35}},'cloth',hx,by+p.arm)
  N('Arms',39,34,40,37,'lit',hx,by+p.arm)
  P('Arms',{{34,36},{38,35},{40,37},{39,40},{35,40},{33,38}},'leatherMid',hx,by+p.arm)
  N('Arms',35,36,38,36,'leatherLit',hx,by+p.arm);N('Arms',35,38,38,38,'leatherLit',hx,by+p.arm)
 end
 -- Stitched doctor's case, restrained brass clasp; rests on floor during surgery.
 local bx=p.tool>0 and -2 or p.bag;local bagY=p.tool>0 and 2 or by-p.bag
 N('BagTools',17,44,17,42,'leatherLit',bx,bagY);N('BagTools',17,42,21,42,'leatherLit',bx,bagY);N('BagTools',21,42,21,44,'leatherLit',bx,bagY)
 P('BagTools',{{13,45},{24,44},{27,47},{26,53},{12,53},{11,50}},'leather',bx,bagY)
 P('BagTools',{{13,45},{23,45},{24,49},{12,49}},'leatherMid',bx,bagY)
 N('BagTools',13,46,22,46,'leatherLit',bx,bagY);N('BagTools',12,50,25,50,'ink',bx,bagY)
 N('BagTools',13,52,24,52,'leatherLit',bx,bagY);N('BagTools',25,47,25,51,'leatherMid',bx,bagY)
 N('BagTools',18,45,18,52,'brassDark',bx,bagY);P('BagTools',{{17,48},{20,48},{20,51},{17,51}},'brassDark',bx,bagY)
 N('BagTools',18,48,19,48,'brass',bx,bagY);pixel(L.BagTools,18+bx,50+bagY,'brass')
 -- Exterior outline generated from the actual composited coverage, exactly 1 native pixel.
 local composite=Image(64,64);for _,n in ipairs(names)do if n~='Outline'then composite:drawImage(L[n])end end
 for y=1,62 do for x=1,62 do if pc.rgbaA(composite:getPixel(x,y))>0 then
  for _,d in ipairs({{-1,0},{1,0},{0,-1},{0,1}})do local xx,yy=x+d[1],y+d[2];if pc.rgbaA(composite:getPixel(xx,yy))==0 then L.Outline:drawPixel(xx,yy,C.outline)end end
 end end end
 return L,{x=55+hx,y=41+by+(p.arm-6)},p
end
local selected={};local tags={}
if full then
 for _,clip in ipairs(clips)do local first=#selected;for i,ms in ipairs(clip.ms)do selected[#selected+1]={tag=clip.name,phase=i,ms=ms}end;tags[#tags+1]={name=clip.name,from=first,to=#selected-1,loop=clip.loop or false}end
else selected={{tag='idle',phase=1,ms=300},{tag='move',phase=2,ms=100},{tag='move',phase=6,ms=100},{tag='treat_loop',phase=1,ms=160},{tag='treat_loop',phase=4,ms=160}}end
local s=Sprite(64,64,ColorMode.RGB);s:assignColorSpace(ColorSpace{sRGB=true});local layers={}
for i,n in ipairs(names)do local l=i==1 and s.layers[1]or s:newLayer();l.name=n;layers[n]=l end
local palette=Palette(25);palette:setColor(0,Color{r=0,g=0,b=0,a=0});local keys={};for k in pairs(hex)do keys[#keys+1]=k end;table.sort(keys)
for i,k in ipairs(keys)do palette:setColor(i,Color{r=pc.rgbaR(C[k]),g=pc.rgbaG(C[k]),b=pc.rgbaB(C[k]),a=255})end;s:setPalette(palette)
local meta={assetId='Scp049',revision='r001',canvas={width=64,height=64},pivot={x=32,y=56},colorSpace='sRGB',alpha='straight RGBA; 0 or 255',lighting='Unshaded / Mix / neutral parent',frames={},tags=tags,sheet={columns=7,rows=6,trim=false,rotation=false,padding=0,mergeDuplicates=false},completeAnimation=full,flip={allowed=true,pivot={x=32,y=56},anchorFormula='x_mirrored = 64 - x; mirror tools and bag with body'},reference='ART-004 ref-001'}
for n,f in ipairs(selected)do
 if n>1 then s:newEmptyFrame(n)end;s.frames[n].duration=f.ms/1000
 local L,contact,p=draw(f.tag,f.phase)
 for _,name in ipairs(names)do s:newCel(layers[name],n,L[name],Point(0,0))end
 local im=Image(64,64);im:drawSprite(s,n)
 if full then im:saveAs(root..'revisions/r001/review/frames/frame-'..string.format('%02d',n-1)..'.png')end
 meta.frames[n]={index=n-1,asepriteFrame=n,tag=f.tag,phase=f.phase,durationMs=f.ms,frame={x=(n-1)%7*64,y=math.floor((n-1)/7)*64,w=64,h=64},pivot={x=32,y=56},treatmentAnchor=p.tool>0 and contact or {x=50,y=42},tool=p.tool==1 and 'scalpel'or p.tool==2 and 'syringe'or 'none',groundContact={x=32,y=56}}
end
for _,t in ipairs(tags)do local tag=s:newTag(t.from+1,t.to+1);tag.name=t.name;tag.repeats=t.loop and 0 or 1 end
s.data='ART-004 r001, ref-001. 64x64 native pixel art; right-facing three-quarter view; mirrored left. Unshaded opaque pigment with transparent outside. Treatment clock remains game-owned.'
s.filename=root..'source/Scp049'..(full and ''or '-samples')..'.aseprite';s:saveAs(s.filename)
local function write(path,data)local f=assert(io.open(path,'w'));f:write(json.encode(data));f:close()end
if full then
 s:saveCopyAs(root..'revisions/r001/source/Scp049.aseprite');app.activeSprite=s
 app.command.ExportSpriteSheet{ui=false,askOverwrite=false,type=SpriteSheetType.ROWS,columns=7,textureFilename=root..'revisions/r001/export/Scp049.png',dataFilename=root..'revisions/r001/export/Scp049.json',dataFormat=SpriteSheetDataFormat.JSON_ARRAY,trim=false,trimSprite=false,ignoreEmpty=false,mergeDuplicates=false,listTags=true,listLayers=true,borderPadding=0,shapePadding=0,innerPadding=0}
 write(root..'revisions/r001/export/Scp049.contract.json',meta)
else
 local board=Image(#selected*64,64);for n=1,#selected do local im=Image(64,64);im:drawSprite(s,n);board:drawImage(im,Point((n-1)*64,0))end
 board:saveAs(run..'samples-1x.png');board:resize(#selected*256,256);board:saveAs(run..'samples-4x.png')
end
write(run..(full and 'build-full.json'or 'build-samples.json'),{passed=true,frames=#selected,full=full,api=app.apiVersion,file=s.filename,layers=names})
local saved=s.filename
if app.isUIAvailable then app.activeSprite=s;app.activeFrame=s.frames[1];if app.editor then app.editor.zoom=8 end;app.refresh()end
return {passed=true,frames=#selected,full=full,file=saved}
