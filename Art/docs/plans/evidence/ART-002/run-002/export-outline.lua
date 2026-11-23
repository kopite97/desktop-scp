local root="C:/Dev/scp_desktop/Art/assets/characters/Scp999/"
local src=Sprite{fromFile=root.."revisions/r004/source/Scp999-lighting.aseprite"}
local s=Sprite(64,64,ColorMode.RGB);s:assignColorSpace(ColorSpace{sRGB=true});s.filename=args.sourcePath
local names={"BodyColor","OutlineUnlit","FaceLit","NormalData","SpecularData","ReferenceR004"}
local layers={s.layers[1]};for i=2,6 do layers[i]=s:newLayer()end
for i,l in ipairs(layers)do l.name=names[i];l.isVisible=i<=3 end
local pc=app.pixelColor
local sheets={};for _,n in ipairs({"color","outline","normal","specular","combined"})do sheets[n]=Image(#args.frames*64,64)end
local audit={frames=#args.frames,recompositionErrors=0,overlappingOutputPixels=0,partialOutlineAlpha=0,faceOverlap=0,outlinePixels=0,framesInfo={}}
local function layerImage(layer,n)local im=Image(64,64);local cel=layer:cel(n);if cel then im:drawImage(cel.image,cel.position)end;return im end
for i,n in ipairs(args.frames)do
 if i>1 then s:newEmptyFrame(i)end;s.frames[i].duration=src.frames[n].duration
 local body=layerImage(src.layers[1],n);local outline=layerImage(src.layers[2],n);local face=layerImage(src.layers[3],n)
 local normal=layerImage(src.layers[4],n);local spec=layerImage(src.layers[5],n);local combined=Image(64,64);combined:drawSprite(src,n)
 for y=0,63 do for x=0,63 do
  local oa=pc.rgbaA(outline:getPixel(x,y))
  if oa>0 then
   audit.outlinePixels=audit.outlinePixels+1
   if oa~=255 then audit.partialOutlineAlpha=audit.partialOutlineAlpha+1 end
   if pc.rgbaA(face:getPixel(x,y))>0 then audit.faceOverlap=audit.faceOverlap+1 end
   body:drawPixel(x,y,0)
  end
 end end
 assert(audit.partialOutlineAlpha==0 and audit.faceOverlap==0,"Requires partial-alpha/face overlap handling")
 local lit=body:clone();lit:drawImage(face,Point(0,0))
 local joined=lit:clone();joined:drawImage(outline,Point(0,0))
 for y=0,63 do for x=0,63 do
  if joined:getPixel(x,y)~=combined:getPixel(x,y) then audit.recompositionErrors=audit.recompositionErrors+1 end
  if pc.rgbaA(lit:getPixel(x,y))>0 and pc.rgbaA(outline:getPixel(x,y))>0 then audit.overlappingOutputPixels=audit.overlappingOutputPixels+1 end
 end end
 for k,im in ipairs({body,outline,face,normal,spec,combined})do s:newCel(layers[k],i,im,Point(0,0))end
 for key,im in pairs({color=lit,outline=outline,normal=normal,specular=spec,combined=combined})do sheets[key]:drawImage(im,Point((i-1)*64,0))end
 table.insert(audit.framesInfo,{index=i-1,sourceFrame=n,durationMs=math.floor(s.frames[i].duration*1000+.5)})
end
for _,v in ipairs(args.tags)do local t=s:newTag(v.from+1,v.to+1);t.name=v.name;t.repeats=v.loop and 0 or 1 end
s.data="ART-002 follow-up of completed ART-001. Split lit body/face and unlit outline, no overlapping pixels. Source Scp999 r004."
s:setPalette(src.palettes[1]);s:saveAs(s.filename)
if args.submissionPath then s:saveCopyAs(args.submissionPath)end
for name,im in pairs(sheets)do im:saveAs(args.outputDir.."/Scp999_"..name..".png")end
src:close();app.activeSprite=s;app.activeFrame=s.frames[1];app.activeLayer=layers[2];app.editor.zoom=4;app.refresh()
return audit
