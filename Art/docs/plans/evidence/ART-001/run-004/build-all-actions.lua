local root="C:/Dev/scp_desktop/Art/assets/characters/Scp999/"
local src=Sprite{fromFile=root.."revisions/r001/source/Scp999.aseprite"}
local out=Sprite(64,64,ColorMode.RGB)
out:assignColorSpace(ColorSpace{sRGB=true})
out.filename=root.."source/Scp999-lighting.aseprite"
local names={"BodyColor","Outline","Face","NormalData","SpecularData","ReferenceR001"}
local layers={out.layers[1]}
for i=2,#names do layers[i]=out:newLayer() end
for i,l in ipairs(layers) do l.name=names[i]; l.blendMode=BlendMode.NORMAL;l.isVisible=i<=3 end
local pc=app.pixelColor
local selected={};for i=1,50 do selected[i]=i end
local meta={assetId="Scp999",revision=4,kind="full-animation-lighting",completeAnimation=true,
 canvas={width=64,height=64},pivot={x=32,y=56},sourceRevision=1,frames={},
 normal={encoding="RGB tangent vector in [0,255]; X right,Y up,Z toward viewer",neutral={128,128,255},alpha="opaque; color map owns coverage",method="silhouette distance height field, smoothed gradient; preliminary"},
 specular={encoding="RGB equal grayscale; 180 body, 0 face/outline/background",alpha="opaque; ignored as coverage",interpretation="CanvasTexture specular/shininess input candidate; actual engine parameter response unverified"},
 color={bodyRGBA={247,138,33,211},directionalShading=false,highlightsIncluded=false},
 engineValidation="PLAN-005 stage A: r002 tested; r004 playback pending"}
local colorsheet=Image(3200,64)
local normalsheet=Image(3200,64)
local specsheet=Image(3200,64)
local refsheet=Image(3200,64)
local results={frames={},coverageChanges=0}
local function rgbaAt(layer,n,x,y)
 local cel=layer:cel(n);if not cel then return 0 end
 local xx,yy=x-cel.position.x,y-cel.position.y
 if xx<0 or yy<0 or xx>=cel.image.width or yy>=cel.image.height then return 0 end
 return cel.image:getPixel(xx,yy)
end
local function idx(x,y)return y*64+x+1 end
for k,n in ipairs(selected) do
 if k>1 then out:newEmptyFrame(k) end
 out.frames[k].duration=src.frames[n].duration
 local body,edge,face,normal,spec,ref=Image(64,64),Image(64,64),Image(64,64),Image(64,64),Image(64,64),Image(64,64)
 ref:drawSprite(src,n)
 local mask,boundary,height={},{},{}
 local function contains(x,y) return x>=0 and x<64 and y>=0 and y<64 and pc.rgbaA(ref:getPixel(x,y))>0 end
 for y=0,63 do for x=0,63 do
  local b=rgbaAt(src.layers[1],n,x,y)
  if pc.rgbaA(b)>0 then body:drawPixel(x,y,pc.rgba(247,138,33,211)) end
  edge:drawPixel(x,y,rgbaAt(src.layers[2],n,x,y))
  face:drawPixel(x,y,rgbaAt(src.layers[3],n,x,y))
  mask[idx(x,y)]=contains(x,y)
  if contains(x,y) and (not contains(x-1,y) or not contains(x+1,y) or not contains(x,y-1) or not contains(x,y+1)) then table.insert(boundary,{x,y}) end
 end end
 for y=0,63 do for x=0,63 do
  local v=0
  if mask[idx(x,y)] then
   local distance=4096
   for _,p in ipairs(boundary) do local d=(x-p[1])^2+(y-p[2])^2;if d<distance then distance=d end end
   v=5.5*math.sqrt(1-math.exp(-(math.sqrt(distance)+0.5)/5))
  end
  height[idx(x,y)]=v
 end end
 for pass=1,2 do
  local smooth={}
  for y=0,63 do for x=0,63 do
   local total=0
   for yy=-1,1 do for xx=-1,1 do local qx,qy=x+xx,y+yy
    if qx>=0 and qx<64 and qy>=0 and qy<64 then total=total+height[idx(qx,qy)]*(xx==0 and 2 or 1)*(yy==0 and 2 or 1) end
   end end
   smooth[idx(x,y)]=total/16
  end end
  height=smooth
 end
 local function H(x,y)if x<0 or y<0 or x>63 or y>63 then return 0 end return height[idx(x,y)] end
 local color=Image(64,64);color:drawImage(body);color:drawImage(edge);color:drawImage(face)
 local bounds=color:shrinkBounds()
 local count=0
 for y=0,63 do for x=0,63 do
  local nx,ny,nz=0,0,1
  if mask[idx(x,y)] then nx=-(H(x+1,y)-H(x-1,y))*0.675;ny=(H(x,y+1)-H(x,y-1))*0.675 end
  local length=math.sqrt(nx*nx+ny*ny+nz*nz);nx,ny,nz=nx/length,ny/length,nz/length
  normal:drawPixel(x,y,pc.rgba(math.floor((nx*.5+.5)*255+.5),math.floor((ny*.5+.5)*255+.5),math.floor((nz*.5+.5)*255+.5),255))
  local gloss=mask[idx(x,y)] and pc.rgbaA(edge:getPixel(x,y))==0 and pc.rgbaA(face:getPixel(x,y))==0 and 180 or 0
  spec:drawPixel(x,y,pc.rgba(gloss,gloss,gloss,255))
  if (pc.rgbaA(color:getPixel(x,y))>0)~=mask[idx(x,y)] then results.coverageChanges=results.coverageChanges+1 end
  if mask[idx(x,y)] then count=count+1 end
 end end
 local imgs={body,edge,face,normal,spec,ref}
 for i,l in ipairs(layers) do out:newCel(l,k,imgs[i],Point(0,0)) end
 colorsheet:drawImage(color,Point((k-1)*64,0))
 normalsheet:drawImage(normal,Point((k-1)*64,0))
 specsheet:drawImage(spec,Point((k-1)*64,0))
 refsheet:drawImage(ref,Point((k-1)*64,0))
 table.insert(meta.frames,{sampleIndex=k-1,sampleAsepriteFrame=k,sourceAsepriteFrame=n,sourceIndex=n-1,sourceTag=(function() for _,tag in ipairs(src.tags) do if n>=tag.fromFrame.frameNumber and n<=tag.toFrame.frameNumber then return tag.name end end end)(),durationMs=math.floor(src.frames[n].duration*1000+.5),frame={x=(k-1)*64,y=0,w=64,h=64},pivot={x=32,y=56}})
 table.insert(results.frames,{sample=k,source=n,visiblePixels=count,bounds={x=bounds.x,y=bounds.y,width=bounds.width,height=bounds.height}})
end
for _,oldtag in ipairs(src.tags) do local t=out:newTag(oldtag.fromFrame.frameNumber,oldtag.toFrame.frameNumber);t.name=oldtag.name;t.repeats=oldtag.repeats;t.aniDir=oldtag.aniDir end
out.data="ART-001 r004 lighting samples only, source r001 frames 1-50. Full 10 action tags; original durations and poses preserved."
out:saveAs(out.filename)
out:saveCopyAs(root.."revisions/r004/source/Scp999-lighting.aseprite")
colorsheet:saveAs(root.."revisions/r004/export/Scp999_color.png")
normalsheet:saveAs(root.."revisions/r004/export/Scp999_normal.png")
specsheet:saveAs(root.."revisions/r004/export/Scp999_specular.png")
refsheet:saveAs(root.."revisions/r004/review/r001-reference.png")
local pal=Palette(6)
for i,c in ipairs({pc.rgba(0,0,0,0),pc.rgba(247,138,33,211),pc.rgba(67,34,39,255),pc.rgba(63,32,39,255),pc.rgba(255,247,207,255),pc.rgba(255,121,69,240)}) do pal:setColor(i-1,Color(c)) end
-- Palette is an editor convenience; normal data is never quantized.
out:setPalette(pal)
out:saveAs(out.filename)
out:saveCopyAs(root.."revisions/r004/source/Scp999-lighting.aseprite")
app.activeSprite=out;app.activeFrame=out.frames[1]
app.editor.zoom=4;app.editor.scroll={x=32,y=32};app.refresh()
return {contract=meta,checks=results}
