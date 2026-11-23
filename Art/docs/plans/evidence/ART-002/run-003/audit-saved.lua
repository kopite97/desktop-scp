local root="C:/Dev/scp_desktop/Art/assets/characters/Scp999/"
local active=sprite
local s=Sprite{fromFile=root.."revisions/r005/source/Scp999-outline-unlit.aseprite"}
local im={}
for _,k in ipairs({"color","outline","normal","specular","combined"})do im[k]=Image{fromFile=root.."revisions/r005/export/Scp999_"..k..".png"}end
local old={};for _,k in ipairs({"color","normal","specular"})do old[k]=Image{fromFile=root.."revisions/r004/export/Scp999_"..k..".png"}end
local checks={frames=#s.frames,tags=#s.tags,layers=#s.layers,sourceMismatch=0,combinedMismatch=0,normalMismatch=0,specularMismatch=0,durationMismatch=0,tagMismatch=0}
assert(s.width==64 and s.height==64 and #s.frames==50 and #s.tags==10 and #s.layers==6)
local board=Image(512,512)
for y=0,511 do for x=0,511 do local v=(math.floor(x/8)+math.floor(y/8))%2==0 and 48 or 61;board:drawPixel(x,y,app.pixelColor.rgba(v,v+4,v+10,255))end end
for n=1,50 do
 local actual=Image(64,64);actual:drawSprite(s,n)
 if math.floor(s.frames[n].duration*1000+.5)~=args.frames[n].durationMs then checks.durationMismatch=checks.durationMismatch+1 end
 for y=0,63 do for x=0,63 do
  local sx=(n-1)*64+x
  if actual:getPixel(x,y)~=im.combined:getPixel(sx,y)then checks.sourceMismatch=checks.sourceMismatch+1 end
  if im.combined:getPixel(sx,y)~=old.color:getPixel(sx,y)then checks.combinedMismatch=checks.combinedMismatch+1 end
  if im.normal:getPixel(sx,y)~=old.normal:getPixel(sx,y)then checks.normalMismatch=checks.normalMismatch+1 end
  if im.specular:getPixel(sx,y)~=old.specular:getPixel(sx,y)then checks.specularMismatch=checks.specularMismatch+1 end
 end end
end
for i,t in ipairs(s.tags)do local want=args.tags[i];if t.name~=want.name or t.fromFrame.frameNumber~=want.from+1 or t.toFrame.frameNumber~=want.to+1 then checks.tagMismatch=checks.tagMismatch+1 end end
for col,n in ipairs({1,7,9,12})do
 for row,k in ipairs({"combined","color","outline","normal"})do local cell=Image(im[k],Rectangle((n-1)*64,0,64,64));cell:resize(128,128);board:drawImage(cell,Point((col-1)*128,(row-1)*128))end
end
board:saveAs(root.."revisions/r005/review/layer-comparison.png")
s:close();app.activeSprite=active;app.activeFrame=active.frames[1];app.activeLayer=active.layers[2];app.refresh()
return checks
