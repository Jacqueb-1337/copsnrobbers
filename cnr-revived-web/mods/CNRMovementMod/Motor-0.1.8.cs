using System;
namespace CNRMovementMod {
// Independent offline motor. Reference defaults are from the archived CODM
// constructor/fallback research; CNR unit scale and unresolved laws are tuning.
public sealed class Motor {
 public enum Mode { Walk, Sprint, Slide, Air, Crouch }
 public Mode State {get;private set;}
 public float X {get;private set;} public float Z {get;private set;} public float Y {get;private set;}
 public const float ReferenceWalk=4f, ReferenceStrafe=2f, ReferenceJumpHeight=.74f, ReferenceJumpVelocity=4.6858f;
 public float UnitScale=1.75f; // provisional: 4 reference units maps to prior CNR walk of 7.
 public float SprintRatio=1.3f, CrouchRatio=.55f, AirAcceleration=5f, SlideDuration=.82f; // provisional.
 public float Acceleration=36f, Brake=50f, FallingGravity=8.4f;
 public float JumpLimitTime=1f, JumpDecreaseRate=.2f, MinimumJumpHeight=.3f;
 public float SlideMaximum=8f, SlideMinimum=2f, SlideEntryMinimum=3f, SlideDrag=6f, SlideTurnDegrees=15f;
 public float SlideIdleTime=.5f;
 public float SlopeAcceleration=16f; // provisional gravity/terrain response, native law not fully resolved.
 public float LandingDip {get;private set;}
 float landingFeedbackTime,landingStrength;
 public float SlideBoost=1.28f; // provisional entry boost; not claimed as CODM default.
 public float Mobility=1f, SprintMobility=1f; // supplied by offline caller, no artificial upper cap.
 public float Walk {get{return ReferenceWalk*UnitScale*Mobility;}}
 public float Sprint {get{return Walk*SprintRatio*SprintMobility;}}
 public float Speed {get{return (float)Math.Sqrt(X*X+Z*Z);}}
 public float CurrentJumpHeight {get;private set;} // native units
 public float CurrentSlideLimit {get;private set;} // native units
 public float CurrentSlideIdle {get;private set;}
 float clock,lastJump=-100,slideRemaining,ascentGravity,landingRemaining; bool crouched,wasAirborne;
 public void Reset(){X=Z=Y=slideRemaining=clock=0;lastJump=-100;CurrentJumpHeight=0;CurrentSlideLimit=CurrentSlideIdle=0;ascentGravity=landingRemaining=landingFeedbackTime=landingStrength=LandingDip=0;State=Mode.Walk;crouched=wasAirborne=false;}
 static float Clamp(float v,float lo,float hi){return Math.Max(lo,Math.Min(hi,v));}
 void Approach(float x,float z,float rate,float dt){float dx=x-X,dz=z-Z;float len=(float)Math.Sqrt(dx*dx+dz*dz);float k=len>0?Math.Min(1,rate*dt/len):0;X+=dx*k;Z+=dz*k;}
 void ScaleSpeed(float speed){float s=Speed;if(s>0){X*=speed/s;Z*=speed/s;}}
 void BeginSlide(){
  // Each slide starts at the normal cap; no rapid-repeat exhaustion.
  CurrentSlideLimit=SlideMaximum;
  CurrentSlideIdle=SlideIdleTime;
  ScaleSpeed(Math.Min(Speed*SlideBoost,CurrentSlideLimit*UnitScale));
  slideRemaining=SlideDuration; crouched=true;
 }
 void BeginJump(){
  CurrentJumpHeight=clock-lastJump<=JumpLimitTime?Clamp(CurrentJumpHeight*(1-JumpDecreaseRate),MinimumJumpHeight,ReferenceJumpHeight):ReferenceJumpHeight;
  lastJump=clock;
  // Height-controlled ballistic approximation, NOT the reference ascent curve.
  ascentGravity=ReferenceJumpVelocity*ReferenceJumpVelocity/(2*ReferenceJumpHeight)*UnitScale;
  Y=(float)Math.Sqrt(2*ascentGravity*CurrentJumpHeight*UnitScale);
  slideRemaining=0;crouched=false;State=Mode.Air;
 }
 public void SetHorizontal(float x,float z){X=x;Z=z;}
 public void AddVertical(float impulse){Y+=impulse;if(Y>0)State=Mode.Air;}
 // Inputs are local to facing: +Z forward, X strafe. Caller transforms output to world.
 public void Tick(float dt,float x,float z,bool grounded,bool sprint,bool crouchPress,bool jump,bool headClear,float downhill=0f){
  dt=Clamp(dt,0,.05f);if(dt==0)return;clock+=dt;
  landingFeedbackTime=Math.Max(0,landingFeedbackTime-dt);
  LandingDip=landingStrength*(float)Math.Sin(Math.PI*landingFeedbackTime/.22f);
  float mag=(float)Math.Sqrt(x*x+z*z);if(mag>1){x/=mag;z/=mag;mag=1;}if(mag<.05f){x=z=mag=0;}
  if(!grounded){slideRemaining=0;State=Mode.Air;wasAirborne=true;} else if(wasAirborne){wasAirborne=false;landingRemaining=.1f;ScaleSpeed(Speed*.7f);landingStrength=Clamp((-Y-2f)/12f,0,1)*.075f*UnitScale;landingFeedbackTime=.22f;LandingDip=0;}
  if(grounded&&crouchPress&&slideRemaining<=0){
   // Judge the current sprint intent and actual forward movement, not a stale state.
   float entrySpeed=SlideEntryMinimum*UnitScale*Math.Min(1f,Math.Max(.1f,Mobility));
   if(sprint&&z>.7f&&Speed>=entrySpeed){BeginSlide();}
   else if(!crouched||headClear){crouched=!crouched;}
  }
  if(grounded&&jump&&headClear){BeginJump();return;}
  if(!grounded){
   if(mag>0)Approach(x*Walk,z*Walk,AirAcceleration*UnitScale*mag,dt);
   float up=ascentGravity>0?ascentGravity:FallingGravity*UnitScale, down=FallingGravity*UnitScale;
   if(Y>0&&Y<up*dt){float remaining=dt-Y/up;Y=-down*remaining;}
   else Y-=(Y>0?up:down)*dt;
   return;
  }
  Y=-2f;
  if(slideRemaining>0){
   State=Mode.Slide;slideRemaining=Math.Max(0,slideRemaining-dt);
   float speed=Speed;
   if(speed>0&&mag>0){
    // Bounded steering approximation: reference turn-angle semantics not yet verified.
    double from=Math.Atan2(X,Z), to=Math.Atan2(x,z), diff=to-from;
    while(diff>Math.PI)diff-=2*Math.PI;while(diff< -Math.PI)diff+=2*Math.PI;
    double step=SlideTurnDegrees*Math.PI/180*dt;
    double angle=from+Math.Max(-step,Math.Min(step,diff));X=(float)Math.Sin(angle)*speed;Z=(float)Math.Cos(angle)*speed;
   }
   float slope=Clamp(downhill,-1f,1f);
   ScaleSpeed(Clamp(speed+(SlopeAcceleration*slope-SlideDrag)*UnitScale*dt,0,CurrentSlideLimit*UnitScale));
   if(Speed<SlideMinimum*UnitScale)slideRemaining=0;
   return;
  }
  if((State==Mode.Slide||sprint&&mag>.7f&&z>0)&&headClear&&!crouchPress)crouched=false;
  State=crouched?Mode.Crouch:(sprint&&mag>.7f&&z>0?Mode.Sprint:Mode.Walk);
  float forward=State==Mode.Sprint?Sprint:Walk;
  float lateral=ReferenceStrafe*UnitScale*Mobility;
  // Reference landing window is 100 ms at 70% horizontal velocity; cap target rather than compound damping per frame.
  if(landingRemaining>0){forward*=.7f;lateral*=.7f;landingRemaining=Math.Max(0,landingRemaining-dt);}
  if(State==Mode.Crouch){forward*=CrouchRatio;lateral*=CrouchRatio;}
  Approach(x*lateral,z*forward,(mag==0?Brake:Acceleration)*UnitScale,dt);
 }
 public void FinishTraversal(bool grounded){X=Z=0;Y=grounded?-2f:0;slideRemaining=landingRemaining=0;crouched=wasAirborne=false;State=grounded?Mode.Walk:Mode.Air;}
 public void HitCeiling(){if(Y>0)Y=0;}
 public void HitWall(){X=Z=slideRemaining=0;}
}
}



