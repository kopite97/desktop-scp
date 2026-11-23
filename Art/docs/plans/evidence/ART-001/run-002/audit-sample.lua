local root="C:/Dev/scp_desktop/Art/assets/characters/Scp999/"
local r1=Sprite{fromFile=root.."revisions/r001/source/Scp999.aseprite"}
local s=sprite
local pc=app.pixelColor
local sources={1,7,9,12}
local sheets={Image{fromFile=root.."revisions/r002/export/Scp999_color.png"},Image{fromFile=root.."revisions/r002/export/Scp999_normal.png"},Image{fromFile=root.."revisions/r002/export/Scp999_specular.png"}}
local result={frames=4,sourceFrames=sources,outlineMismatches=0,faceMismatches=0,colorCoverageMismatches=0,nonUniformBody=0,exportMismatches=0,invalidNormalLength=0,invalidSpecular=0,clippedFrames=0,normalLengthMin=2,normalLengthMax=0,engineValidation="not executed",userApproval="pending"}
local function pixel(layer,n,x,y)
 local c=layer:cel(n);local xx,yy=x-c.position.x,y-c.position.y
 if xx<0 or yy<0 or xx>=c.image.width or yy>=c.image.height then return 0 end
 return c.image:getPixel(xx,yy)
end
local board=Image(512,512)
local backgrounds=Image(384,256)
local function drawScaled(dst,src,dx,dy,scale)
 local im=src:clone();im:resize(im.width*scale,im.height*scale);dst:drawImage(im,Point(dx,dy))
end
for y=0,511 do for x=0,511 do local c=(math.floor(x/8)+math.floor(y/8))%2==0 and 54 or 65;board:drawPixel(x,y,pc.rgba(c,c+4,c+10,255)) end end
for y=0,255 do for x=0,383 do
 local b=x<128 and {244,241,233} or x<256 and {26,30,40} or ((math.floor(x/8)+math.floor(y/8))%2==0 and {194,173,146} or {126,153,164})
 backgrounds:drawPixel(x,y,pc.rgba(b[1],b[2],b[3],255))
end end
for n,source in ipairs(sources) do
 local im=Image(64,64);im:drawSprite(s,n)
 local old=Image(64,64);old:drawSprite(r1,source)
 local bounds=im:shrinkBounds()
 if bounds.x<=0 or bounds.y<=0 or bounds.x+bounds.width>=64 or bounds.y+bounds.height>=64 then result.clippedFrames=result.clippedFrames+1 end
 for y=0,63 do for x=0,63 do
  if pixel(s.layers[2],n,x,y)~=pixel(r1.layers[2],source,x,y) then result.outlineMismatches=result.outlineMismatches+1 end
  if pixel(s.layers[3],n,x,y)~=pixel(r1.layers[3],source,x,y) then result.faceMismatches=result.faceMismatches+1 end
  if (pc.rgbaA(im:getPixel(x,y))>0)~=(pc.rgbaA(old:getPixel(x,y))>0) then result.colorCoverageMismatches=result.colorCoverageMismatches+1 end
  local b=pixel(s.layers[1],n,x,y)
  if pc.rgbaA(b)>0 and b~=pc.rgba(247,138,33,211) then result.nonUniformBody=result.nonUniformBody+1 end
  local v=pixel(s.layers[4],n,x,y)
  local nx,ny,nz=pc.rgbaR(v)/255*2-1,pc.rgbaG(v)/255*2-1,pc.rgbaB(v)/255*2-1
  local length=math.sqrt(nx*nx+ny*ny+nz*nz)
  result.normalLengthMin=math.min(result.normalLengthMin,length);result.normalLengthMax=math.max(result.normalLengthMax,length)
  if math.abs(length-1)>.015 or nz<=0 or pc.rgbaA(v)~=255 then result.invalidNormalLength=result.invalidNormalLength+1 end
  local sp=pixel(s.layers[5],n,x,y);local value=pc.rgbaR(sp)
  local expected=pc.rgbaA(im:getPixel(x,y))>0 and pc.rgbaA(pixel(s.layers[2],n,x,y))==0 and pc.rgbaA(pixel(s.layers[3],n,x,y))==0 and 180 or 0
  if value~=expected or pc.rgbaG(sp)~=value or pc.rgbaB(sp)~=value or pc.rgbaA(sp)~=255 then result.invalidSpecular=result.invalidSpecular+1 end
  for i,p in ipairs({im:getPixel(x,y),v,sp}) do if p~=sheets[i]:getPixel((n-1)*64+x,y) then result.exportMismatches=result.exportMismatches+1 end end
 end end
 drawScaled(board,old,(n-1)*128,0,2);drawScaled(board,im,(n-1)*128,128,2)
 local norm=Image(64,64);norm:drawImage(s.layers[4]:cel(n).image,s.layers[4]:cel(n).position)
 local spec=Image(64,64);spec:drawImage(s.layers[5]:cel(n).image,s.layers[5]:cel(n).position)
 drawScaled(board,norm,(n-1)*128,256,2);drawScaled(board,spec,(n-1)*128,384,2)
 if n==1 or n==3 then for col=0,2 do drawScaled(backgrounds,im,col*128,n==1 and 0 or 128,2) end end
end
board:saveAs(root.."revisions/r002/review/map-comparison.png")
backgrounds:saveAs(root.."revisions/r002/review/base-backgrounds.png")
app.activeSprite=s;app.activeFrame=s.frames[1];app.editor.zoom=4;app.editor.scroll={x=32,y=32};app.refresh()
result.status="PASS"
for _,key in ipairs({"outlineMismatches","faceMismatches","colorCoverageMismatches","nonUniformBody","exportMismatches","invalidNormalLength","invalidSpecular","clippedFrames"}) do if result[key]~=0 then result.status="FAIL" end end
return result
