-- This script only acts on the ART-004 file supplied to this new Aseprite instance.
local s=app.activeSprite
assert(s and s.filename:find('Scp049',1,true) and app.isUIAvailable,'ART-004 UI file required')
local run='C:/Dev/scp_desktop/Art/docs/plans/evidence/ART-004/run-001/'
local elapsed=0;local visited={};local observed={};local total=0
for _,f in ipairs(s.frames)do total=total+f.duration end
app.activeFrame=s.frames[1];if app.editor then app.editor.zoom=3 end;app.refresh()
local timer
timer=Timer{interval=.05,ontick=function()
 elapsed=elapsed+.05
 local phase=elapsed%total;local accumulated=0
 for n,f in ipairs(s.frames)do accumulated=accumulated+f.duration;if phase<accumulated then app.activeSprite=s;app.activeFrame=f;app.refresh();break end end
 if app.activeSprite==s and app.activeFrame then local n=app.activeFrame.frameNumber;if not visited[n]then visited[n]=true;observed[#observed+1]=n end end
 if elapsed>=12 then
  print('ART004_GUI_PLAYBACK '..json.encode({ui=true,file=s.filename,observedFrames=observed,pollSeconds=elapsed,scope='Aseprite UI frame progression driven by 50ms timer; exact frame durations stored in GIF; not user visual approval'}));timer:stop();app.activeFrame=s.frames[1];app.command.PlayAnimation()
 end
end};timer:start();_G.art004PreviewTimer=timer
