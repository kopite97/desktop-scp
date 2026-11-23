local root="C:/Dev/scp_desktop/Art/assets/vfx/Scp999Reaction/"
local s=sprite;local pc=app.pixelColor
local png=Image{fromFile=root.."revisions/r001/export/Scp999Reaction.png"}
local saved=Sprite{fromFile=root.."revisions/r001/source/Scp999Reaction.aseprite"}
local result={frames=#s.frames,width=s.width,height=s.height,sheet={width=png.width,height=png.height},exportMismatches=0,savedMismatches=0,clippedPixels=0,invalidAlpha=0,invalidColor=0,outlineErrors=0,framesInfo={},tags={},transparentEnds=true,userApproval="pending",productIntegration=false}
local alpha={160,230,255,255,230,150,80,0}
local allowed={}
for _,v in ipairs({{81,43,63},{239,112,147},{255,191,200},{36,65,76},{95,206,195},{178,244,223}})do allowed[v[1]..","..v[2]..","..v[3]]=true end
local board=Image(512,128)
for y=0,127 do for x=0,511 do local v=(math.floor(x/8)+math.floor(y/8))%2==0 and 220 or 241;board:drawPixel(x,y,pc.rgba(v,v,v,255))end end
for n=1,#s.frames do
 local a,b=Image(32,32),Image(32,32);a:drawSprite(s,n);b:drawSprite(saved,n)
 local count=0;local expectedAlpha=alpha[(n-1)%8+1]
 local fillLayer=s.layers[n<=8 and 1 or 3];local edgeLayer=s.layers[n<=8 and 2 or 4]
 local function at(layer,x,y)
  local cel=layer:cel(n);if not cel then return 0 end
  x=x-cel.position.x;y=y-cel.position.y
  if x<0 or y<0 or x>=cel.image.width or y>=cel.image.height then return 0 end
  return pc.rgbaA(cel.image:getPixel(x,y))
 end
 for y=0,31 do for x=0,31 do
  local p=a:getPixel(x,y);local pa=pc.rgbaA(p)
  if p~=png:getPixel(((n-1)%8)*32+x,math.floor((n-1)/8)*32+y)then result.exportMismatches=result.exportMismatches+1 end
  if p~=b:getPixel(x,y)then result.savedMismatches=result.savedMismatches+1 end
  if pa>0 then
   count=count+1
   if x==0 or y==0 or x==31 or y==31 then result.clippedPixels=result.clippedPixels+1 end
   if pa~=expectedAlpha then result.invalidAlpha=result.invalidAlpha+1 end
   if not allowed[pc.rgbaR(p)..","..pc.rgbaG(p)..","..pc.rgbaB(p)]then result.invalidColor=result.invalidColor+1 end
  end
  if at(fillLayer,x,y)>0 then
   for _,d in ipairs({{-1,0},{1,0},{0,-1},{0,1}})do
    if at(fillLayer,x+d[1],y+d[2])==0 and at(edgeLayer,x+d[1],y+d[2])==0 then result.outlineErrors=result.outlineErrors+1 end
   end
  end
  if at(edgeLayer,x,y)>0 and at(fillLayer,x-1,y)==0 and at(fillLayer,x+1,y)==0 and at(fillLayer,x,y-1)==0 and at(fillLayer,x,y+1)==0 then result.outlineErrors=result.outlineErrors+1 end
 end end
 if n==8 or n==16 then result.transparentEnds=result.transparentEnds and count==0 end
 local bnd=a:shrinkBounds();table.insert(result.framesInfo,{frame=n,ms=math.floor(s.frames[n].duration*1000+.5),visiblePixels=count,bounds={x=bnd.x,y=bnd.y,w=bnd.width,h=bnd.height},alpha=expectedAlpha})
 local big=a:clone();big:resize(64,64);board:drawImage(big,Point((n-1)%8*64,math.floor((n-1)/8)*64))
end
for _,t in ipairs(s.tags)do table.insert(result.tags,{name=t.name,from=t.fromFrame.frameNumber,to=t.toFrame.frameNumber,repeats=t.repeats})end
board:saveAs(root.."revisions/r001/review/contact-sheet.png")
saved:close();app.activeSprite=s;app.activeFrame=s.frames[4];app.activeLayer=s.layers[1];app.refresh()
return result
