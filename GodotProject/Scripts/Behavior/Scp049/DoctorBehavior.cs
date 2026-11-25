using Godot;
using SiteDesktop.Platforms;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Behavior.Scp049;
public sealed class DoctorBehavior
{
    private readonly PlatformWorld _body;
    private readonly DesktopObjectState _world;
    private readonly PlagueSettings _settings;
    private readonly string _owner;
    private readonly IdleState _idle;
    private readonly MovingState _moving;
    private readonly ActionState _treat;
    private sealed record Failure(long Revision,Vector2 Position,int Attempts,double Until);
    private readonly Dictionary<int,Failure> _failures=new();
    private double _searchWait,_remoteDelay,_journeyAge;
    private MonitorRoute? _route;
    private SiteDesktop.Desktop.DesktopScreen? _dragScreen;
    private bool _waitForLanding;
    public PortalTransferState Portal { get; }
    public MonitorRoutePlanner Routes { get; }
    public double RemoteDelay=>_remoteDelay;
    public StateMachine Machine { get; }=new();
    public MovementController Movement { get; }
    public int? TargetId { get; private set; }
    public double TreatmentAge { get; private set; }
    public double? TreatmentStartedAt { get; private set; }
    public double Clock { get; private set; }
    public float TreatmentRate { get; set; }=1;
    public DoctorPhase Phase=>_body.Body.Motion==PlatformMotion.Dragging?DoctorPhase.Dragging:TargetId==null?DoctorPhase.Idle:Portal.Active?DoctorPhase.Transferring:Machine.Current==_treat?DoctorPhase.Treating:DoctorPhase.Seeking;
    public DoctorBehavior(PlatformWorld body,DesktopObjectState world,PlagueSettings settings,string owner)
    {
        _body=body;_world=world;_settings=settings;_owner=owner;Movement=new(body,new(1.5,settings.TargetTimeout,true));Routes=new(body,Movement.Navigator);
        Portal=new(body,route=>TargetGoal() is {} goal&&Routes.Valid(route,goal),success=>
        {
            if(!success){Cancel(true);return;}
            _route=null;_remoteDelay=Portal!.Settings.RemoteDelaySeconds;Machine.Request(_moving!,"portal arrived");
        });
        _idle=new(body,Search);
        _moving=new(Movement,Goal,OnMovement);
        _treat=new("Treating",dt=>TreatmentAge+=dt*TreatmentRate,()=>TreatmentStartedAt=Clock);
        Machine.Request(_idle,"created");
    }
    private static bool Settled(PlatformWorld p)=>p.Body.SupportId!=null&&p.Body.Motion==PlatformMotion.Grounded;
    private MovementGoal? Goal()=>_route?.Departure??TargetGoal();
    private MovementGoal? TargetGoal()=>TargetId is int id&&_world.Objects.TryGetValue(id,out var target)?new(target.Foot,target.Physics.Body.SupportId,_settings.TreatmentRange):null;
    public bool InRange(DummyObject item)=>Settled(_body)&&Settled(item.Physics)&&_body.Body.SupportId==item.Physics.Body.SupportId&&Math.Abs(_body.Body.Foot.X-item.Foot.X)<=_settings.TreatmentRange&&Math.Abs(_body.Body.Foot.Y-item.Foot.Y)<1;
    private bool RetryAllowed(DummyObject item)
    {
        if(!_failures.TryGetValue(item.Id,out var failure))return true;
        if(failure.Revision!=_body.Revision||failure.Position.DistanceSquaredTo(item.Foot)>64){_failures.Remove(item.Id);return true;}
        return failure.Attempts<3&&Clock>=failure.Until;
    }
    private void Failed(int id)
    {
        if(!_world.Objects.TryGetValue(id,out var item))return;
        int attempts=_failures.TryGetValue(id,out var previous)&&previous.Revision==_body.Revision?previous.Attempts+1:1;
        _failures[id]=new(_body.Revision,item.Foot,attempts,Clock+_settings.RetrySeconds*attempts);
    }
    private void Search(double dt)
    {
        _searchWait-=dt;if(_searchWait>0||!Settled(_body))return;_searchWait=.2;
        if(_world.Objects.Values.Count(o=>o.Kind==DummyKind.Zombie)>=_world.Settings.Zombies.Maximum)return;
        var here=Routes.ScreenAt(_body.Body.Foot);
        foreach(var item in _world.Objects.Values.Where(o=>o.Kind!=DummyKind.Zombie&&o.BrokenAge==null&&o.ClaimedBy==null&&Settled(o.Physics)&&RetryAllowed(o))
            .OrderBy(o=>Routes.ScreenAt(o.Foot)==here?0:1).ThenBy(o=>o.Foot.DistanceSquaredTo(_body.Body.Foot)).ThenBy(o=>o.Id))
        {
            var goal=new MovementGoal(item.Foot,item.Physics.Body.SupportId,_settings.TreatmentRange);
            bool remote=Routes.ScreenAt(item.Foot)!=here;
            if(remote&&(_remoteDelay>0||_waitForLanding))continue;
            var route=remote?Routes.Plan(goal):null;
            if(remote?route==null:!Movement.CanReach(goal)){Failed(item.Id);continue;}
            if(!_world.TryClaim(item.Id,_owner))continue;
            _route=route;_journeyAge=0;TargetId=item.Id;Machine.Request(InRange(item)?_treat:_moving,"target reserved");return;
        }
    }

    private void OnMovement(MovementStatus result)
    {
        if(result==MovementStatus.Arrived)
        {
            if(_route is {} route){Portal.Prepare(route);Machine.Request(Portal,"departure reached");}
            else Machine.Request(_treat,"arrived");return;
        }
        if(result!=MovementStatus.Moving)Cancel(true);
    }
    public void Tick(double dt)
    {
        Clock+=dt;if(Phase==DoctorPhase.Dragging)return;
        if(_waitForLanding){if(Settled(_body)){_waitForLanding=false;_remoteDelay=Portal.Settings.RemoteDelaySeconds;}}
        else _remoteDelay=Math.Max(0,_remoteDelay-dt);
        if(TargetId is int id)
        {
            _journeyAge+=dt;
            if(_journeyAge>=360||(_route!=null&&(TargetGoal() is not {} current||!Routes.Valid(_route,current)))){Cancel(true);Machine.Tick(dt);return;}
            if(!_world.Objects.TryGetValue(id,out var item)||item.Kind==DummyKind.Zombie||item.BrokenAge!=null||item.ClaimedBy!=_owner)Cancel();
            else if(Phase==DoctorPhase.Treating&&!InRange(item))Cancel();
            else if(!Portal.Active&&_route==null&&InRange(item))Machine.Request(_treat,"treatment range");
        }
        Machine.Tick(dt);
    }
    public void DragStarted(){_dragScreen=Routes.ScreenAt(_body.Body.Foot);Cancel();}
    public void DragEnded(){if(Routes.ScreenAt(_body.Body.Foot)!=_dragScreen){_waitForLanding=true;_remoteDelay=Portal.Settings.RemoteDelaySeconds;}_failures.Clear();}
    public void Revalidate(){if(_route!=null&&(TargetGoal() is not {} goal||!Routes.Valid(_route,goal))){Cancel(true);return;}if(Phase==DoctorPhase.Treating&&TargetId is int id&&(!_world.Objects.TryGetValue(id,out var item)||!InRange(item)))Cancel();}
    public bool Complete(int id,string owner)
    {
        if(!_body.Visible||_body.Stopped||owner!=_owner||TargetId!=id||Phase!=DoctorPhase.Treating||TreatmentAge+1e-8<_settings.TreatmentSeconds||!_world.Objects.TryGetValue(id,out var item)||!InRange(item))return false;
        if(!_world.Transform(id,owner))return false;Cancel();return true;
    }
    public void Cancel(bool retry=false)
    {
        if(TargetId is int id){if(retry)Failed(id);_world.Release(id,_owner);}
        TargetId=null;_route=null;TreatmentAge=0;TreatmentStartedAt=null;Movement.Cancel();Machine.Cancel();Machine.Request(_idle,retry?"movement failed":"cancelled");_searchWait=0;
    }
    public void Invalidated(int id){if(TargetId==id)Cancel();_failures.Remove(id);}
    public void Reset(){Cancel();_failures.Clear();_remoteDelay=0;_waitForLanding=false;_dragScreen=null;}
}
