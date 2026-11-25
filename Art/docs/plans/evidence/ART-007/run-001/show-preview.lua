local a=assert(_G.art007);local s=assert(a.review);assert(app.isUIAvailable)
local elapsed=0;local total=0;local seen={};local order={}
for _,f in ipairs(s.frames)do total=total+f.duration end
app.activeSprite=s;app.activeFrame=s.frames[1];app.editor.zoom=.5;app.command.Timeline{close=true};app.refresh()
local timer
timer=Timer{interval=.02,ontick=function()
 elapsed=elapsed+.02;local phase=elapsed%total;local acc=0
 for n,f in ipairs(s.frames)do acc=acc+f.duration;if phase<acc then app.activeSprite=s;app.activeFrame=f;app.refresh();break end end
 local n=app.activeFrame.frameNumber;if not seen[n]then seen[n]=true;order[#order+1]=n end
 if elapsed>=12 then
  timer:stop();local out=assert(io.open(a.run..'playback.json','w'));out:write(json.encode({date='2026-11-25',ui=true,file=s.filename,timerSeconds=elapsed,intervalMs=20,observedFrames=order,totalFrames=#s.frames,allFramesObserved=#order==#s.frames,scope='Aseprite Live mock event playback; not Godot event/input/physics tests or user approval'}));out:close()
  app.activeFrame=s.frames[1];app.command.PlayAnimation()
 end
end};timer:start();_G.art007PreviewTimer=timer
return {started=true,review=s.filename,totalSeconds=total}
