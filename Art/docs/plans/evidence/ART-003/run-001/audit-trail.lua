local root="C:/Dev/scp_desktop/Art/assets/vfx/Scp999Trail/"
local run="C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-003/run-001/"
local work=Sprite{fromFile=root.."source/Scp999Trail.aseprite"}
local saved=Sprite{fromFile=root.."revisions/r001/source/Scp999Trail.aseprite"}
local png=Image{fromFile=root.."revisions/r001/export/Scp999Trail.png"}
local f=assert(io.open(root.."revisions/r001/export/Scp999Trail.contract.json","r"));local contract=json.decode(f:read("*a"));f:close()
local pc=app.pixelColor
local result={frames=#work.frames,tags=#work.tags,width=work.width,height=work.height,sourceMismatch=0,exportMismatch=0,clippedPixels=0,invalidColor=0,invalidAlpha=0,outlineErrors=0,overlapPixels=0,durationMismatch=0,evaporationGrowth=0,transparentEnds=true,frameInfo={},mode="saved file inspection; no Godot execution"}
assert(work.width==32 and work.height==16 and #work.frames==18 and #work.tags==2)
assert(png.width==288 and png.height==32)
local previous={}
for n=1,#work.frames do
 local a,b=Image(32,16),Image(32,16);a:drawSprite(work,n);b:drawSprite(saved,n)
 local kind=math.floor((n-1)/9)+1;local phase=(n-1)%9+1
 local fill=work.layers[(kind-1)*2+1]:cel(n);local edge=work.layers[(kind-1)*2+2]:cel(n)
 local function at(c,x,y)if not c then return 0 end;x=x-c.position.x;y=y-c.position.y;if x<0 or y<0 or x>=c.image.width or y>=c.image.height then return 0 end;return pc.rgbaA(c.image:getPixel(x,y))end
 local count=0;local mass=0;local fillCount=0
 for y=0,15 do for x=0,31 do
  local p=a:getPixel(x,y);local alpha=pc.rgbaA(p);local fa,ea=at(fill,x,y),at(edge,x,y)
  if p~=b:getPixel(x,y)then result.sourceMismatch=result.sourceMismatch+1 end
  if p~=png:getPixel((phase-1)*32+x,(kind-1)*16+y)then result.exportMismatch=result.exportMismatch+1 end
  if fa>0 and ea>0 then result.overlapPixels=result.overlapPixels+1 end
  if alpha>0 then
   count=count+1;mass=mass+alpha
   if x==0 or y==0 or x==31 or y==15 then result.clippedPixels=result.clippedPixels+1 end
   local isFill=pc.rgbaR(p)==247 and pc.rgbaG(p)==138 and pc.rgbaB(p)==33
   local isEdge=pc.rgbaR(p)==83 and pc.rgbaG(p)==48 and pc.rgbaB(p)==30
   if not isFill and not isEdge then result.invalidColor=result.invalidColor+1 end
   if alpha~=(isFill and contract.frames[n].fillAlpha or contract.frames[n].outlineAlpha)then result.invalidAlpha=result.invalidAlpha+1 end
   local boundary=false
   for _,d in ipairs({{-1,0},{1,0},{0,-1},{0,1}})do if at(fill,x+d[1],y+d[2])==0 and at(edge,x+d[1],y+d[2])==0 then boundary=true end end
   if boundary~=isEdge then result.outlineErrors=result.outlineErrors+1 end
   if isFill then fillCount=fillCount+1 end
  end
 end end
 if math.floor(work.frames[n].duration*1000+.5)~=contract.frames[n].durationMs then result.durationMismatch=result.durationMismatch+1 end
 if phase>=4 and (count>previous.count or mass>previous.mass)then result.evaporationGrowth=result.evaporationGrowth+1 end
 if phase==9 then result.transparentEnds=result.transparentEnds and count==0 end
 previous={count=count,mass=mass}
 local bounds=a:shrinkBounds();table.insert(result.frameInfo,{index=n-1,pixels=count,fillPixels=fillCount,alphaSum=mass,bounds={x=bounds.x,y=bounds.y,w=bounds.width,h=bounds.height},durationMs=math.floor(work.frames[n].duration*1000+.5)})
end
result.passed=result.sourceMismatch+result.exportMismatch+result.clippedPixels+result.invalidColor+result.invalidAlpha+result.outlineErrors+result.overlapPixels+result.durationMismatch+result.evaporationGrowth==0 and result.transparentEnds
local out=assert(io.open(run.."results.json","w"));out:write(json.encode(result));out:close()
work:close();saved:close()
assert(result.passed,"Trail audit failed; see results.json")
return result
