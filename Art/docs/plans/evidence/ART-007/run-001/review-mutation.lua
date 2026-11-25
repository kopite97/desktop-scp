local a=assert(_G.art007);local feet=assert(_G.art006);local pc=app.pixelColor
local function rgb(r,g,b,alpha)return pc.rgba(r,g,b,alpha or 255)end
local function rect(im,x,y,w,h,c)for yy=y,y+h-1 do for xx=x,x+w-1 do if xx>=0 and yy>=0 and xx<im.width and yy<im.height then im:drawPixel(xx,yy,c)end end end end
local font={A='010101111101101',B='110101110101110',C='011100100100011',D='110101101101110',E='111100110100111',F='111100110100100',G='011100101101011',H='101101111101101',I='111010010010111',J='001001001101010',K='101101110101101',L='100100100100111',M='101111111101101',N='101111111111101',O='010101101101010',P='110101110100100',Q='010101101111011',R='110101110101101',S='011100010001110',T='111010010010010',U='101101101101111',V='101101101101010',W='101101111111101',X='101101010101101',Y='101101010010010',Z='111001010100111',['0']='111101101101111',['1']='010110010010111',['2']='110001010100111',['3']='110001010001110',['4']='101101111001001',['5']='111100110001110',['6']='011100111101111',['7']='111001010010010',['8']='111101111101111',['9']='111101111001110',['-']='000000111000000',['/']='001001010100100',[':']='000010000010000',['.']='000000000000010',[' ']='000000000000000'}
local function text(im,str,x,y,scale,c)for i=1,#str do local bits=font[str:sub(i,i)]or font[' '];for k=1,15 do if bits:sub(k,k)=='1'then rect(im,x+(i-1)*4*scale+((k-1)%3)*scale,y+math.floor((k-1)/3)*scale,scale,scale,c)end end end end
local light=rgb(237,234,224);local dark=rgb(35,30,46)
local function scaled(im,factor)local out=Image(im);out:resize(im.width*factor,im.height*factor);return out end
local effects={};for i,im in ipairs(a.combined)do effects[i]=scaled(im,2)end
local function footFrame(elapsed)
 local tag=elapsed<500 and feet.tags[1]or feet.tags[3];local time=elapsed<500 and elapsed or((elapsed-500)%840);local sum=0
 for i,ms in ipairs(tag.ms)do sum=sum+ms;if time<sum then return tag.from+i-1 end end
 return tag.to
end
local function composite(k,meta,flip)
 local im=Image(96,96);local body=meta.zombie and feet.composite(k,footFrame(meta.zombieMs),flip)or feet.icons[k]
 im:drawImage(body,Point(16,16));if meta.effect then im:drawImage(effects[meta.effect])end;return im
end
local timeline={};local sections={}
local function add(meta)timeline[#timeline+1]=meta end
local function treatment(name,total)
 sections[#sections+1]={name=name,totalMs=total,start=#timeline+1};local elapsed=0
 while elapsed<total do local index=math.floor(elapsed/120)%8+1;local ms=math.min(120,total-elapsed);add{stage=name,ms=ms,effect=index,zombie=false,elapsedMs=elapsed};elapsed=elapsed+ms end
 sections[#sections].finish=#timeline
end
local function success(name)
 for i=1,8 do add{stage=name..' / TRANSFORM',ms=100,effect=8+i,zombie=true,zombieMs=(i-1)*100}end
 for i=1,6 do add{stage=name..' / DONE',ms=100,zombie=true,zombieMs=800+(i-1)*100}end
end
treatment('NORMAL 3 SEC',3000);success('NORMAL')
treatment('OVERLOAD 2 SEC',2000);success('OVERLOAD')
treatment('CANCEL TEST',480)
for i=1,4 do add{stage='CANCELLED / NO FEET',ms=120,zombie=false}end
treatment('RESTART AT FRAME 1',480)
for i=1,4 do add{stage='REMOVED / CLEAR VFX',ms=120,zombie=false,removed=true}end
local base=Image(920,650);rect(base,0,0,920,650,light)
text(base,'ART-007 / TREAT AND TRANSFORM',28,22,4,dark)
text(base,'MOCK EVENT PREVIEW / NOT THE RUNNING GAME',28,86,2,rgb(100,87,103))
local names={'ROUND','SQUARE','ASYMMETRIC','HOLE','PADDING','NARROW'}
for k=1,6 do text(base,names[k],106+(k-1)*130,108,2,dark)end
for row=1,3 do local y=130+(row-1)*112;rect(base,20,y,880,104,row==1 and rgb(255,251,243)or(row==2 and rgb(35,39,52)or rgb(178,181,185)))
 if row==3 then for yy=y,y+103,8 do for xx=20,899,8 do if (math.floor((xx-20)/8)+math.floor((yy-y)/8))%2==0 then rect(base,xx,yy,8,8,rgb(210,213,217))end end end end
 text(base,row==1 and 'LIGHT'or(row==2 and 'DARK'or'GRID'),28,y+8,2,row==2 and light or dark)
end
text(base,'8 OVERLAP / TWO DIRECTIONS',28,484,2,dark);text(base,'1X SOURCE SCALE',526,484,2,dark)
text(base,'2X GAME SCALE / CLEAR ICON CENTER',526,594,2,dark)
text(base,'TREAT LOOPS UNTIL GAME EVENT / TRANSFORM 800 MS',28,626,2,rgb(100,87,103))
local review=Sprite(920,650,ColorMode.RGB);review:assignColorSpace(ColorSpace{sRGB=true});review.layers[1].name='Generic mock event preview - not runtime capture'
local overallMs=0
for f,meta in ipairs(timeline)do
 local im=Image(base);text(im,'R001 / '..meta.stage,28,62,2,dark)
 if not meta.removed then
  for row=1,3 do for k=1,6 do im:drawImage(composite(k,meta,row==2),Point(96+(k-1)*130,134+(row-1)*112))end end
  for j=1,8 do im:drawImage(composite((j-1)%6+1,meta,j%2==0),Point(20+(j-1)*32,512))end
  for k=1,6 do local half=composite(k,meta,false);half:resize(48,48);im:drawImage(half,Point(520+(k-1)*62,534))end
 end
 if f>1 then review:newEmptyFrame(f)end;review.frames[f].duration=meta.ms/1000;review:newCel(review.layers[1],f,im);overallMs=overallMs+meta.ms
 if f==3 then im:saveAs(a.root..'revisions/r001/review/Scp049Mutation-treat.png')end
 if f==28 then im:saveAs(a.root..'revisions/r001/review/Scp049Mutation-transform.png')end
end
review:saveAs(a.root..'revisions/r001/review/Scp049Mutation-review.aseprite');review:saveCopyAs(a.root..'revisions/r001/review/Scp049Mutation-review.gif')
-- Work-area clipping is illustrated explicitly; it is not an OS integration test.
local edge=Image(640,320);rect(edge,0,0,640,320,light);text(edge,'WORK AREA MASK / MOCK ONLY',20,18,3,dark)
local eg={effect=11,zombie=true,zombieMs=200};local board=Image(280,220);rect(board,0,0,280,220,rgb(35,39,52))
for _,p in ipairs({{-24,-18},{208,-18},{-24,140},{208,140}})do board:drawImage(composite(1,eg,false),Point(p[1],p[2]))end
edge:drawImage(board,Point(20,70));local full=composite(2,eg,false);edge:drawImage(scaled(full,2),Point(390,80))
text(edge,'CLIP AT WORK AREA',20,300,2,dark);text(edge,'FULL 96X96 CELL',386,300,2,dark)
edge:saveAs(a.root..'revisions/r001/review/Scp049Mutation-boundaries.png')
local out=assert(io.open(a.run..'mock-timeline.json','w'));out:write(json.encode({totalMs=overallMs,sections=sections,frames=timeline,scope='Art review only; simulated events'}));out:close()
a.review=review;a.timeline=timeline;a.sections=sections;a.composite=composite;a.effects=effects
if _G.art007Sample then _G.art007Sample:close();_G.art007Sample=nil end
app.activeSprite=review;app.activeFrame=review.frames[3];app.editor.zoom=.5;app.refresh()
return {review=review.filename,frames=#timeline,totalMs=overallMs}
