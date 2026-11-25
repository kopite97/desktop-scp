using SiteDesktop.Platforms;
namespace SiteDesktop.Companions;
// Presentation state owns clip timing; behavior receives only readiness signals.
public sealed class CompanionAnimation
{
    private readonly PlatformWorld Physics;
    private readonly SpriteAnimationSet Body;
    public string Tag { get; private set; }="idle";
    public double TagAge { get; private set; }
    public CompanionMotion Motion { get; private set; }
    public bool Flipped { get; private set; }
    public bool CanMove=>Body.Clips[Tag].Loop||TagAge>=Body.Duration(Tag);
    public bool AcceptsReaction=>Tag is not ("pet_recover" or "land" or "balance_recover");
    public CompanionAnimation(PlatformWorld physics,SpriteAnimationSet body){Physics=physics;Body=body;}
    public void Advance(double delta)=>TagAge+=delta;
    public void Play(string tag){if(Tag==tag)return;Tag=tag;TagAge=0;}
    public void Face(bool left)=>Flipped=left;
    public void Reset(){Tag="idle";TagAge=0;Motion=CompanionMotion.Grounded;}
    public void SyncMotion(bool force=false,bool dragging=false)
    {
        if(dragging)return;
        var motion=Physics.Body.Motion switch {PlatformMotion.Takeoff=>CompanionMotion.Takeoff,PlatformMotion.Falling=>CompanionMotion.Falling,PlatformMotion.Landing=>CompanionMotion.Landing,PlatformMotion.Balancing=>CompanionMotion.Balancing,_=>CompanionMotion.Grounded};
        if(motion==CompanionMotion.Grounded&&Tag=="land"&&TagAge<Body.Duration("land"))return;
        if(!force&&motion==Motion)return;
        var previous=Motion;Motion=motion;
        Play(motion switch {CompanionMotion.Takeoff=>"takeoff",CompanionMotion.Falling=>"fall",CompanionMotion.Landing=>"land",CompanionMotion.Balancing=>"balance",_=>previous==CompanionMotion.Balancing?"balance_recover":"idle"});
    }
}
