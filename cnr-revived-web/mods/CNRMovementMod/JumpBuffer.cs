namespace CNRMovementMod {
public sealed class JumpBuffer {
 public const float Window=.2f;
 bool pending; int lastFrame=-1,landingFrame=-1; float pressedAt;
 public void Press(int frame,float now){if(frame==lastFrame)return;lastFrame=frame;pending=true;pressedAt=now;landingFrame=-1;}
 public bool Fresh(float now){return pending&&now>=pressedAt&&now-pressedAt<=Window;}
 public void MarkLanding(int frame,float now){if(Fresh(now))landingFrame=frame;}
 public bool Consume(bool grounded,int frame,float now){
  bool valid=Fresh(now)||(pending&&landingFrame>=0&&frame>=landingFrame&&frame-landingFrame<=1);
  if(!valid){pending=false;landingFrame=-1;return false;}
  if(!grounded)return false;
  Clear();return true;
 }
 public void Clear(){pending=false;landingFrame=-1;}
 public void Reset(){Clear();lastFrame=-1;}
}
}
