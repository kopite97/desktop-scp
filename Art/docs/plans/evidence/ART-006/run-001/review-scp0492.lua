local a=assert(_G.art006,'Run build first');local pc=app.pixelColor
local function rgb(r,g,b,alpha)return pc.rgba(r,g,b,alpha or 255)end
local function rect(im,x,y,w,h,c)for yy=y,y+h-1 do for xx=x,x+w-1 do if xx>=0 and yy>=0 and xx<im.width and yy<im.height then im:drawPixel(xx,yy,c)end end end end
local font={A='010101111101101',B='110101110101110',C='011100100100011',D='110101101101110',E='111100110100111',F='111100110100100',G='011100101101011',H='101101111101101',I='111010010010111',J='001001001101010',K='101101110101101',L='100100100100111',M='101111111101101',N='101111111111101',O='010101101101010',P='110101110100100',Q='010101101111011',R='110101110101101',S='011100010001110',T='111010010010010',U='101101101101111',V='101101101101010',W='101101111111101',X='101101010101101',Y='101101010010010',Z='111001010100111',['0']='111101101101111',['1']='010110010010111',['2']='110001010100111',['3']='110001010001110',['4']='101101111001001',['5']='111100110001110',['6']='011100111101111',['7']='111001010010010',['8']='111101111101111',['9']='111101111001110',['-']='000000111000000',['/']='001001010100100',[':']='000010000010000',['.']='000000000000010',[' ']='000000000000000'}
local function text(im,str,x,y,scale,c)for i=1,#str do local bits=font[str:sub(i,i)]or font[' '];for k=1,15 do if bits:sub(k,k)=='1'then rect(im,x+(i-1)*4*scale+((k-1)%3)*scale,y+math.floor((k-1)/3)*scale,scale,scale,c)end end end end
local dark=rgb(35,30,46);local light=rgb(237,234,224)
local function fixture(kind)
 local im=Image(48,48)
 local function disk(cx,cy,r,c)for y=0,47 do for x=0,47 do if (x-cx)^2+(y-cy)^2<=r*r then im:drawPixel(x,y,c)end end end end
 if kind==1 then disk(23.5,23.5,22,dark);disk(23.5,23.5,20,rgb(78,149,191));rect(im,17,13,5,22,light);rect(im,22,13,10,5,light);rect(im,22,30,10,5,light)
 elseif kind==2 then rect(im,3,3,42,42,dark);rect(im,5,5,38,38,rgb(200,126,87));rect(im,12,12,24,3,light);rect(im,12,20,17,3,light);rect(im,12,28,22,3,light)
 elseif kind==3 then rect(im,7,5,12,39,dark);rect(im,19,18,22,26,dark);rect(im,9,7,8,35,rgb(185,156,207));rect(im,17,20,22,22,rgb(185,156,207));rect(im,14,27,16,4,light)
 elseif kind==4 then disk(23.5,23.5,22,dark);disk(23.5,23.5,20,rgb(104,183,159));disk(23.5,23.5,11,dark);disk(23.5,23.5,9,0)
 elseif kind==5 then rect(im,16,16,17,17,dark);rect(im,18,18,13,13,rgb(224,179,80));rect(im,21,21,7,7,light)
 else rect(im,20,4,8,40,dark);rect(im,22,6,4,36,rgb(130,180,211));rect(im,22,14,4,3,light)end
 return im
end
local icons={};local fixtures={};local attachment={}
for k=1,6 do
 local raw=fixture(k);local b=raw:shrinkBounds();assert(b.width>0)
 local im=Image(64,64);local ox=math.floor(32-(b.x+b.width/2)+.5);local oy=56-(b.y+b.height)
 im:drawImage(raw,Point(ox,oy));icons[k]=im
 local support={};for x=0,63 do for y=52,55 do if pc.rgbaA(im:getPixel(x,y))>0 then support[#support+1]=x;break end end end
 local half=math.min(9,math.max(3,math.floor(b.width*.18+.5)))
 local function nearest(target,exclude)local best=nil;local cost=10000;for _,x in ipairs(support)do if not exclude or math.abs(x-exclude)>=2 then local q=math.abs(x-target)*100+math.abs(x-32);if q<cost then best=x;cost=q end end end;return best or support[1]end
 local l=nearest(32-half);local r=nearest(32+half,l)
 attachment[k]={left=l,right=r,y=56,width=b.width,rawBounds={x=b.x,y=b.y,w=b.width,h=b.height},offset={x=ox,y=oy}}
 fixtures[k]={name=({'ROUND','SQUARE','ASYMMETRIC','HOLE','PADDING','NARROW'})[k],attachment=attachment[k]}
end
local function scaled(im,factor)local out=Image(im);out:resize(im.width*factor,im.height*factor);return out end
local function composite(k,f,flip)
 local im=Image(64,64);local slots={attachment[k].left,attachment[k].right}
 for j,src in ipairs({a.left[f],a.right[f]})do
  for it in src:pixels()do local v=it();if pc.rgbaA(v)>0 then
   -- Mirror in attachment space, not about the 31/2 cell midpoint.
   local xx=slots[j]+(flip and 16-it.x or it.x-16)*2;local yy=56+(it.y-28)*2
   rect(im,xx,yy,2,2,v)
  end end
 end
 local body=Image(icons[k]);local alpha=a.frames[f].bodyAlpha
 if alpha~=1 then for it in body:pixels()do local v=it();if pc.rgbaA(v)>0 then it(rgb(pc.rgbaR(v),pc.rgbaG(v),pc.rgbaB(v),math.floor(pc.rgbaA(v)*alpha+.5)))end end end
 im:drawImage(body);return im
end
local review=Sprite(920,620,ColorMode.RGB);review:assignColorSpace(ColorSpace{sRGB=true});review.layers[1].name='Generic fixture review - NOT exported game art'
for f,meta in ipairs(a.frames)do
 local im=Image(920,620);rect(im,0,0,920,620,light)
 text(im,'ART-006 / TWO LITTLE FEET',28,22,4,dark)
 text(im,'R001  /  '..string.upper(meta.tag)..'  /  '..meta.durationMs..' MS',28,62,2,dark)
 text(im,'ORIGINAL ICON + FEET ONLY / GENERIC TEST ICONS',28,85,2,rgb(100,87,103))
 for k=1,6 do text(im,fixtures[k].name,106+(k-1)*130,112,2,dark)end
 for row=1,3 do
  local by=138+(row-1)*108
  local bg=row==1 and rgb(255,251,243)or(row==2 and rgb(35,39,52)or rgb(178,181,185))
  rect(im,20,by,880,96,bg)
  if row==3 then for yy=by,by+95,8 do for xx=20,899,8 do if (math.floor((xx-20)/8)+math.floor((yy-by)/8))%2==0 then rect(im,xx,yy,8,8,rgb(210,213,217))end end end end
  text(im,row==1 and 'LIGHT'or(row==2 and 'DARK'or'GRID'),28,by+8,2,row==2 and light or dark)
  for k=1,6 do im:drawImage(composite(k,f,row==2),Point(112+(k-1)*130,by+24))end
 end
 text(im,'8 OVERLAP / ALTERNATING DIRECTIONS',28,474,2,dark)
 for j=1,8 do im:drawImage(composite((j-1)%6+1,f,j%2==0),Point(32+(j-1)*27,500))end
 text(im,'1X FOOT SOURCE SCALE',526,474,2,dark)
 for k=1,6 do local half=composite(k,f,false);half:resize(32,32);im:drawImage(half,Point(530+(k-1)*58,520))end
 text(im,'2X GAME SCALE IN THE THREE ROWS ABOVE',526,568,2,dark)
 text(im,'ICON BODY NEVER FLIPS OR BOUNCES',28,596,2,rgb(100,87,103))
 if f>1 then review:newEmptyFrame(f)end;review.frames[f].duration=meta.durationMs/1000;review:newCel(review.layers[1],f,im)
 if f==10 then im:saveAs(a.root..'revisions/r001/review/Scp0492-overview.png')end
end
-- No tags on this review: continuous playback intentionally shows every state.
review:saveAs(a.root..'revisions/r001/review/Scp0492-review.aseprite')
review:saveCopyAs(a.root..'revisions/r001/review/Scp0492-review.gif')
local detail=Image(720,360);rect(detail,0,0,720,360,light);text(detail,'FOOT DETAILS / 6X SOURCE',24,20,3,dark)
for j,f in ipairs({4,10,17,19,23,29})do local dx=((j-1)%3)*240;local dy=70+math.floor((j-1)/3)*145;text(detail,string.upper(a.frames[f].tag),dx+20,dy,2,dark);local cut=Image(32,10);cut:drawImage(a.left[f],Point(-5,-23));cut:drawImage(a.right[f],Point(6,-23));detail:drawImage(scaled(cut,6),Point(dx+20,dy+28))end
detail:saveAs(a.root..'revisions/r001/review/Scp0492-details.png')
local ff=assert(io.open(a.run..'fixtures.json','w'));ff:write(json.encode(fixtures));ff:close()
_G.art006.review=review;_G.art006.icons=icons;_G.art006.attachments=attachment;_G.art006.composite=composite
app.activeSprite=review;app.activeFrame=review.frames[10];app.editor.zoom=1;app.refresh()
return {review=review.filename,frames=#review.frames,fixtures=fixtures}
