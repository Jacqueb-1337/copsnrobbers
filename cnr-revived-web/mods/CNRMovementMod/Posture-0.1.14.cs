using System;
namespace CNRMovementMod {
// Pure presentation envelope. Timings are provisional, not recovered curves.
public sealed class Posture {
 public float Blend {get;private set;}
 public float LowerTime=.25f, RaiseTime=.25f;
 public float CrouchFraction=.28f, SlideFraction=.42f;
 public float Target(Motor.Mode mode,bool headClear){
  if(mode==Motor.Mode.Slide)return SlideFraction;
  if(mode==Motor.Mode.Crouch)return CrouchFraction;
  return headClear?0f:Math.Max(Blend,CrouchFraction);
 }
 public float Tick(float dt,Motor.Mode mode,bool headClear){
  float target=Target(mode,headClear);
  float duration=target>Blend?LowerTime:RaiseTime;
  float step=Math.Max(0,dt)*SlideFraction/Math.Max(.001f,duration);
  Blend=Blend<target?Math.Min(target,Blend+step):Math.Max(target,Blend-step);
  return Blend;
 }
 public void Reset(){Blend=0;}
}
}

