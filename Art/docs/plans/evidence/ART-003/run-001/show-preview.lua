-- Visible Aseprite playback without Computer Use or a network bridge.
local root="C:/Dev/scp_desktop/Art/assets/vfx/Scp999Trail/"
local s=app.activeSprite
assert(s and app.isUIAvailable,"Visible Aseprite UI required")
local run="C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-003/run-001/"
local elapsed=0;local visited={};local observed={};local started=false
local timer
timer=Timer{interval=.1,ontick=function()
 if not started then
  app.activeSprite=s;app.activeFrame=s.frames[1]
  if app.editor then app.editor.zoom=2 end
  app.refresh();app.command.PlayAnimation();started=true
 end
 elapsed=elapsed+.1
 local current=app.activeSprite
 if current==s and app.activeFrame then
  local frame=app.activeFrame.frameNumber
  if not visited[frame]then visited[frame]=true;table.insert(observed,frame)end
 end
 if elapsed>=6 then
  local f=assert(io.open(run.."gui-playback.json","w"))
  f:write(json.encode({ui=app.isUIAvailable,started=started,observedFrames=observed,pollSeconds=elapsed,scope="Aseprite UI frame progression sampled at 100ms, not visual approval or precise wall-clock timing",file=s.filename}));f:close();timer:stop()
 end
end}
timer:start()
_G.art003PreviewTimer=timer
