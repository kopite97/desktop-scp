-- Run inside visible Aseprite with --script-param mode=samples or mode=full.
local root="C:/Dev/scp_desktop/Art/assets/vfx/Scp999Trail/"
local run="C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-003/run-001/"
local pc=app.pixelColor
local full=app.params.mode=="full"
for _,dir in ipairs({root.."source",root.."revisions/r001/source",root.."revisions/r001/export",root.."revisions/r001/review/frames"})do app.fs.makeAllDirectories(dir)end
local durations={80,100,200,140,140,140,120,100,80}
local fade={.65,1,1,.88,.70,.50,.30,.13,0}
local scales={{.70,.8},{1.05,.85},{1,1},{.92,1},{.80,.85},{.64,.75},{.48,.60},{.32,.5},{0,0}}
-- Each row is hand placed. Flat pooled liquid, not a miniature body.
local spans={
 {{5,12,19},{6,7,24},{7,4,27},{8,3,28},{9,5,26},{10,9,22}},
 {{5,18,22},{6,8,12},{6,16,24},{7,6,14},{7,15,25},{8,5,14},{8,16,24},{9,7,12},{9,18,22},{10,25,27},{11,24,28},{12,25,27}}
}
local function draw(kind,phase)
 local mask={};local sx,sy=table.unpack(scales[phase]);local fill,edge=Image(32,16),Image(32,16)
 if sx==0 then return fill,edge end
 local original={};for _,r in ipairs(spans[kind])do for x=r[2],r[3]do original[r[1]*32+x]=true end end
 for y=0,15 do for x=0,31 do
  local ox=math.floor((x-16+.5)/sx+16);local oy=math.floor((y-8+.5)/sy+8)
  if ox>=0 and ox<32 and oy>=0 and oy<16 and original[oy*32+ox]then mask[y*32+x]=true end
 end end
 local function has(x,y)return x>=0 and x<32 and y>=0 and y<16 and mask[y*32+x]end
 for y=0,15 do for x=0,31 do if has(x,y)then
  local boundary=not has(x-1,y)or not has(x+1,y)or not has(x,y-1)or not has(x,y+1)
  -- Both outline and fill fade together; no white specular dot or directional shade.
  if boundary then edge:drawPixel(x,y,pc.rgba(83,48,30,math.floor(148*fade[phase]+.5)))
  else fill:drawPixel(x,y,pc.rgba(247,138,33,math.floor(120*fade[phase]+.5)))end
 end end end
 return fill,edge
end
local s=Sprite(32,16,ColorMode.RGB);s:assignColorSpace(ColorSpace{sRGB=true})
local layers={s.layers[1],s:newLayer(),s:newLayer(),s:newLayer()}
for i,name in ipairs({"PuddleFill","PuddleOutline","DropletsFill","DropletsOutline"})do layers[i].name=name end
local guide=s:newLayer();guide.name="Guide";guide.isVisible=false
local palette=Palette(3);palette:setColor(0,Color{r=0,g=0,b=0,a=0});palette:setColor(1,Color{r=247,g=138,b=33,a=120});palette:setColor(2,Color{r=83,g=48,b=30,a=148});s:setPalette(palette)
local meta={assetId="Scp999Trail",revision=1,bodyReference={assetId="Scp999",revision=5},canvas={width=32,height=16},
 sheet={width=288,height=32,columns=9,rows=2,padding=0,spacing=0},pivot={x=16,y=8},colorMode="RGBA",colorSpace="sRGB",alpha="straight",frames={},tags={},
 lighting={mode="unlit",policy="Unshaded/Mix, neutral parent; low-alpha material, no engine tint or extra fade",maps="none",engineValidation="pending PLAN-005; Art composite only"},
 spawn={anchor="trailAnchor",anchorPolicy="snapshot at spawn; independent of body",followBody=false,drawOrder="behind body",flipAllowed=true},
 lifetime={holdFramePolicy="extend phase 3 externally; do not duplicate pixels",holdMs="TBD by PLAN-005",evaporateMs=720,engineFade=false},
 engineOwns={"spawn interval and cap","hold and total lifetime","platform attachment/removal","native window bounds","input passthrough","hide and cleanup"}}
local count=full and 18 or 2
for n=1,count do
 if n>1 then s:newEmptyFrame(n)end
 local kind=full and math.floor((n-1)/9)+1 or n;local phase=full and (n-1)%9+1 or 3
 s.frames[n].duration=durations[phase]/1000
 local fill,edge=draw(kind,phase);s:newCel(layers[(kind-1)*2+1],n,fill,Point(0,0));s:newCel(layers[(kind-1)*2+2],n,edge,Point(0,0))
 local im=Image(32,16);im:drawSprite(s,n)
 if full then
  im:saveAs(root.."revisions/r001/review/frames/frame-"..string.format("%02d",n-1)..".png")
  table.insert(meta.frames,{index=n-1,asepriteFrame=n,durationMs=durations[phase],frame={x=(phase-1)*32,y=(kind-1)*16,w=32,h=16},phase=phase<3 and "appear" or phase==3 and "hold" or "evaporate",fillAlpha=math.floor(120*fade[phase]+.5),outlineAlpha=math.floor(148*fade[phase]+.5)})
 end
end
for kind,name in ipairs({"puddle_a","puddle_b"})do
 local start=full and (kind-1)*9+1 or kind;local finish=full and kind*9 or kind
 local tag=s:newTag(start,finish);tag.name=name;tag.repeats=1
 table.insert(meta.tags,{name=name,from=start-1,to=finish-1,loop=false,repeats=1,direction="forward",holdFrame=(kind-1)*9+2,evaporateFrom=(kind-1)*9+3,evaporateTo=kind*9-1,demoDurationMs=1100,endState="transparent; remove instance"})
end
s.data="ART-003 r001. Scp999 r005 reference. Unlit low-alpha orange residue. Phase 3 hold controlled by engine. No baked world motion."
s.filename=full and root.."source/Scp999Trail.aseprite" or root.."source/Scp999Trail-samples.aseprite"
s:saveAs(s.filename)
if full then
 local f=assert(io.open(root.."revisions/r001/export/Scp999Trail.contract.json","w"));f:write(json.encode(meta));f:close()
 s:saveCopyAs(root.."revisions/r001/source/Scp999Trail.aseprite")
 app.activeSprite=s
 app.command.ExportSpriteSheet{ui=false,askOverwrite=false,type=SpriteSheetType.ROWS,columns=9,textureFilename=root.."revisions/r001/export/Scp999Trail.png",dataFilename=root.."revisions/r001/export/Scp999Trail.json",dataFormat=SpriteSheetDataFormat.JSON_ARRAY,trim=false,trimSprite=false,ignoreEmpty=false,mergeDuplicates=false,listTags=true,listLayers=true,borderPadding=0,shapePadding=0,innerPadding=0}
else
 local sample=Image(64,16);for n=1,2 do local im=Image(32,16);im:drawSprite(s,n);sample:drawImage(im,Point((n-1)*32,0))end
 sample:saveAs(run.."samples.png")
end
if app.isUIAvailable then
 app.activeSprite=s;app.activeFrame=s.frames[full and 3 or 1];app.editor.zoom=12;app.refresh()
else s:close()end
return meta
