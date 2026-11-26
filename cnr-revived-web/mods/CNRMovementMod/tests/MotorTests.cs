using System;
using CNRMovementMod;
class Tests {
 static int count;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);count++;Console.WriteLine("PASS "+name);}
 static void Near(float a,float b,float tol,string n){Check(Math.Abs(a-b)<tol,n+" "+a);}
 static void Advance(Motor m,float dt,float sec,float x,float z,bool sprint){for(int i=0;i<(int)Math.Round(sec/dt);i++)m.Tick(dt,x,z,true,sprint,false,false,true);}
 static void Main(){
 foreach(int fps in new[]{30,60,120}){
  float dt=1f/fps; Motor m=new Motor();
  Advance(m,dt,.5f,0,1,false);
  m.Tick(dt,0,1,true,true,true,false,true);
  Check(m.State==Motor.Mode.Slide,"same-frame sprint slide "+fps);
  float edgeSpeed=m.Speed;
  Check(!m.TryBeginSlideFall(true,false)&&m.State==Motor.Mode.Slide,"grounded slide not edge "+fps);
  Check(!m.TryBeginSlideFall(false,true)&&m.State==Motor.Mode.Slide,"near support not edge "+fps);
  Check(m.TryBeginSlideFall(false,false)&&m.State==Motor.Mode.Air,"slide edge handoff "+fps);
  Near(m.Speed,edgeSpeed,.0001f,"edge retains horizontal speed "+fps);
  Near(m.Y,0,.0001f,"edge resets vertical speed "+fps);
  Check(!m.TryBeginSlideFall(false,false),"edge handoff once "+fps);
  m.Tick(dt,0,0,false,false,false,false,true);
  Near(m.Y,-m.FallingGravity*m.UnitScale*dt,.001f,"edge resumes normal gravity "+fps);
  Near(m.Speed,edgeSpeed,.0001f,"edge no-input air momentum "+fps);
  for(int i=0;i<fps/2;i++)m.Tick(dt,0,1,false,true,false,false,true);
  Near(m.Speed,edgeSpeed,.001f,"edge full forward retains momentum "+fps);
  m.Tick(dt,0,-1,false,false,false,false,true);
  Check(m.Speed<edgeSpeed,"edge intentional reversal brakes "+fps);
  m.Tick(dt,0,1,true,true,false,false,true);
  Check(m.Speed<edgeSpeed,"edge landing slows "+fps);
  m.Reset();Advance(m,dt,.5f,0,1,true);
  float ordinarySpeed=m.Speed;m.Tick(dt,0,1,false,true,false,false,true);
  Check(m.Speed<ordinarySpeed,"ordinary fall unchanged "+fps);
  m.Reset();Check(!m.TryBeginSlideFall(false,false),"walking cannot edge launch "+fps);
  m.Reset();m.Mobility=.5f;Advance(m,dt,.5f,0,1,true);
  m.Tick(dt,0,1,true,true,true,false,true);
  Check(m.State==Motor.Mode.Slide,"low mobility slide "+fps);
  m.Reset();m.Mobility=1;Advance(m,dt,.5f,0,1,false);
  m.Tick(dt,0,1,true,false,true,false,true);
  Check(m.State==Motor.Mode.Crouch,"non-sprint stays crouch "+fps);
  m.Reset();
  Advance(m,dt,1,0,1,false);Near(m.Speed,7,.03f,"forward "+fps);
  Advance(m,dt,1,1,0,false);Near(m.Speed,3.5f,.03f,"strafe "+fps);
  Advance(m,dt,1,0,-1,true);Near(m.Speed,7,.03f,"backward "+fps);Check(m.State!=Motor.Mode.Sprint,"no reverse sprint "+fps);
  Advance(m,dt,1,0,1,true);Near(m.Speed,m.Sprint,.03f,"sprint "+fps);
  m.Tick(dt,0,1,true,true,true,false,true);Check(m.State==Motor.Mode.Slide&&m.Speed>m.Sprint,"slide entry "+fps);
  Near(m.CurrentSlideLimit,8,.001f,"initial slide cap "+fps);
  float s=m.Speed;m.Tick(dt,0,1,true,false,false,true,true);Check(m.State==Motor.Mode.Air&&m.Speed==s&&m.Y>0,"slide jump momentum "+fps);
  float h=0,peak=0;for(int i=0;i<fps;i++){float before=m.Y;m.Tick(dt,0,0,false,false,false,false,true);h+=(before+m.Y)*.5f*dt;peak=Math.Max(h,peak);}
  Near(peak,.74f*m.UnitScale,.08f,"jump apex "+fps);
  m.Reset();m.Tick(dt,0,1,true,false,false,true,true);Near(m.CurrentJumpHeight,.74f,.001f,"fresh jump "+fps);
  m.Tick(dt,0,1,true,false,false,true,true);Near(m.CurrentJumpHeight,.592f,.001f,"repeat penalty "+fps);
  for(int i=0;i<8;i++)m.Tick(dt,0,1,true,false,false,true,true);
  Near(m.CurrentJumpHeight,.3f,.001f,"jump penalty floor "+fps);
  Advance(m,dt,1.1f,0,0,false);m.Tick(dt,0,1,true,false,false,true,true);Near(m.CurrentJumpHeight,.74f,.001f,"jump reset "+fps);
  m.Reset();Advance(m,dt,.5f,0,1,true);m.Tick(dt,0,1,true,true,true,false,true);
  Advance(m,dt,1.2f,0,1,true);m.Tick(dt,0,1,true,true,true,false,true);
  Near(m.CurrentSlideLimit,8f,.001f,"repeat slide full cap "+fps);Near(m.CurrentSlideIdle,.5f,.001f,"repeat slide full idle "+fps);
  m.Reset();m.Tick(dt,0,0,true,false,true,false,true);Check(m.State==Motor.Mode.Crouch,"crouch "+fps);
  m.Tick(dt,0,0,true,false,true,false,false);Check(m.State==Motor.Mode.Crouch,"blocked stand "+fps);
  m.Reset();var pose=new Posture();Advance(m,dt,1,0,1,true);m.Tick(dt,0,1,true,true,true,false,true);
  for(int i=0;i<(int)(fps*.35f);i++){m.Tick(dt,0,1,true,true,false,false,true);pose.Tick(dt,m.State,true);}
  Check(m.State==Motor.Mode.Slide&&pose.Blend>.4f,"slide camera lowered "+fps);
  for(int i=0;i<fps;i++){m.Tick(dt,0,1,true,true,false,false,true);pose.Tick(dt,m.State,true);}
  Check(m.State==Motor.Mode.Sprint&&pose.Blend<.001f,"resume sprint and restore "+fps);
  m.Reset();pose.Reset();Advance(m,dt,1,0,1,true);m.Tick(dt,0,1,true,true,true,false,true);
  for(int i=0;i<fps;i++){m.Tick(dt,0,1,true,true,false,false,false);pose.Tick(dt,m.State,false);}
  Check(m.State==Motor.Mode.Crouch&&pose.Blend>.25f,"blocked slide recovery "+fps);
  for(int i=0;i<fps;i++){m.Tick(dt,0,1,true,true,false,false,true);pose.Tick(dt,m.State,true);}
  Check(m.State==Motor.Mode.Sprint&&pose.Blend<.001f,"clearance recovery "+fps);
  m.Reset();Advance(m,dt,1,0,1,true);
  m.Tick(dt,0,0,false,false,false,false,true);float beforeLanding=m.Speed;
  m.Tick(dt,0,1,true,true,false,false,true);
  Check(m.Speed<=beforeLanding*.7f+.03f,"landing slowdown "+fps);
  Advance(m,dt,.3f,0,1,true);Near(m.Speed,m.Sprint,.03f,"landing recovery "+fps);
  m.Reset();Advance(m,dt,1,0,1,true);
  m.Tick(dt,0,1,true,false,false,false,true);Check(m.State==Motor.Mode.Walk,"sprint interrupted "+fps);
  Advance(m,dt,.4f,0,1,true);Check(m.State==Motor.Mode.Sprint,"sprint recovered "+fps);
  Motor flat=new Motor(),down=new Motor(),up=new Motor();
  foreach(Motor q in new[]{flat,down,up}){Advance(q,dt,1,0,1,true);q.Tick(dt,0,1,true,true,true,false,true);}
  for(int i=0;i<(int)(fps*.15f);i++){flat.Tick(dt,0,1,true,true,false,false,true,0);down.Tick(dt,0,1,true,true,false,false,true,.5f);up.Tick(dt,0,1,true,true,false,false,true,-.5f);}
  Check(down.Speed>flat.Speed&&flat.Speed>up.Speed,"slope response "+fps);
  Check(down.Speed<=down.CurrentSlideLimit*down.UnitScale+.001f,"downhill capped "+fps);
  m.Reset();for(int i=0;i<fps/2;i++)m.Tick(dt,0,0,false,false,false,false,true);
  m.Tick(dt,0,0,true,false,false,false,true);m.Tick(dt,0,0,true,false,false,false,true);
  Check(m.LandingDip>0,"landing camera dip "+fps);
  Advance(m,dt,.4f,0,0,false);Near(m.LandingDip,0,.001f,"landing dip reset "+fps);
  m.Reset();m.Mobility=2.5f;Advance(m,dt,1,0,1,false);Near(m.Speed,17.5f,.03f,"uncapped mobility "+fps);
  m.AddVertical(8);Check(m.Y>0,"external recoil "+fps);m.HitCeiling();Near(m.Y,0,.001f,"ceiling "+fps);
 }
   var c=new MantlePolicy.Candidate{Height=2,Width=1,ForwardOffset=1,ApproachClear=true,TopClear=true,CeilingClear=true,DestinationClear=true,LandingClear=true,PathClear=true,VaultEligible=true};
  Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.Vault,"vault clear");
  c.LandingClear=false;Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.ClimbUp,"climb-up fallback");
  c.CeilingClear=false;Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.None,"blocked ceiling");
  c.CeilingClear=true;c.PathClear=false;Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.None,"blocked path");
  c.PathClear=true;c.Height=5;Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.None,"too high");
  c.Height=2;c.Width=.1f;Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.None,"too narrow");
  c.Height=2;c.Width=1;c.PathClear=true;c.TopClear=true;c.DestinationClear=true;c.CeilingClear=true;c.ApproachClear=true;c.LandingClear=true;c.VaultEligible=true;c.ForwardOffset=3;
  Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.ClimbUp,"vault out of reach falls back to climb");
  c.Width=.75f;c.ForwardOffset=1;
  Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.Vault,"narrow supported vault");
  c.LandingClear=false;
  Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.None,"narrow ledge without landing rejected");
  c.Width=1;c.DestinationClear=false;
  Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.None,"blocked destination rejected");
  c.DestinationClear=true;c.ApproachClear=false;
  Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.None,"blocked approach rejected");
  c.ApproachClear=true;c.TopClear=false;
  Check(MantlePolicy.Choose(c,1.75f)==MantlePolicy.Kind.None,"blocked top rejected");
  var buffer=new JumpBuffer();
  foreach(int fps in new[]{30,60,120}){
   buffer.Reset();buffer.Press(123,0f);
   Check(buffer.Fresh(.05f)&&!buffer.Consume(false,124,.05f),"airborne intent "+fps);
   Check(buffer.Consume(true,125,.19f)&&!buffer.Consume(true,126,.19f),"buffer once "+fps);
   buffer.Press(127,0f);Check(!buffer.Consume(true,127+fps,.5f),"early double tap expires "+fps);
   buffer.Press(200,0f);buffer.MarkLanding(201,.199f);
   Check(buffer.Consume(true,202,.199f+1f/fps),"landing within window "+fps);
   buffer.Press(203,0f);buffer.MarkLanding(204,.201f);
   Check(!buffer.Consume(true,205,.201f+1f/fps),"late landing rejected "+fps);
   buffer.Press(206,0f);buffer.Press(206,.15f);
   Check(!buffer.Fresh(.21f),"same frame cannot extend "+fps);
   buffer.Press(207,0f);buffer.Clear();
   Check(!buffer.Consume(true,208,.1f),"mantle consumes intent "+fps);
  }
  Check(MantlePolicy.WithinReach(.2f,3f,1.75f,2f,true),"higher airborne reach");
  Check(!MantlePolicy.WithinReach(.2f,3.4f,1.75f,2f,true),"airborne body cap");
  Check(!MantlePolicy.WithinReach(.2f,4.1f,1.75f,4f,true),"airborne reference cap");
  var traversal=new Motor();traversal.SetHorizontal(9,8);traversal.AddVertical(10);traversal.FinishTraversal(false);
  Check(traversal.State==Motor.Mode.Air&&traversal.Speed==0&&traversal.Y==0,"cancel traversal resumes air physics");
  traversal.FinishTraversal(true);Check(traversal.State==Motor.Mode.Walk&&traversal.Y==-2,"finish traversal resumes ground physics");
  Check(MantleTiming.HeightIndex(.75f)==0&&MantleTiming.HeightIndex(.76f)==1,"first height band");
  Check(MantleTiming.HeightIndex(1.05f)==1&&MantleTiming.HeightIndex(1.06f)==2,"second height band");
  Check(MantleTiming.HeightIndex(1.55f)==2&&MantleTiming.HeightIndex(1.56f)==3,"third height band");
  Check(MantleTiming.HeightIndex(1.8f)==3&&MantleTiming.HeightIndex(2.3f)==4,"upper height bands");
  Check(MantleTiming.HeightIndex(2.31f)==-1&&MantleTiming.HeightIndex(float.NaN)==-1,"invalid height");
  Check(MantleTiming.VaultHeight(.8f)&&!MantleTiming.VaultHeight(1.1f),"vault height category");
  Near(MantleTiming.Duration(MantlePolicy.Kind.ClimbUp,.6f),.6f,.0001f,"low climb timing");
  Near(MantleTiming.Duration(MantlePolicy.Kind.ClimbUp,2.1f),1.4f,.0001f,"high climb timing");
  Near(MantleTiming.Duration(MantlePolicy.Kind.Vault,1f),.8f,.0001f,"ADV vault fallback timing");
  Check(!MantleTiming.CanResumeClimb(.6f,.5f,true,true,true),"stop threshold strict");
  Check(MantleTiming.CanResumeClimb(.6f,.51f,true,true,true),"low climb resume threshold");
  Check(!MantleTiming.CanResumeClimb(2.1f,.79f,true,true,true),"high climb threshold");
  Check(MantleTiming.CanResumeClimb(2.1f,.81f,true,true,true),"high climb resume threshold");
  Check(!MantleTiming.CanResumeClimb(.6f,.9f,false,true,true),"resume requires movement");
  Check(!MantleTiming.CanResumeClimb(.6f,.9f,true,false,true),"resume requires support");
  Check(!MantleTiming.CanResumeClimb(.6f,.9f,true,true,false),"resume requires destination");
  Near(MantleTiming.NextSample(.4f,.9f),.45f,.0001f,"split raise boundary");
  Near(MantleTiming.NextSample(.45f,.9f),.82f,.0001f,"split traverse boundary");
  Near(MantleTiming.NextSample(.82f,.9f),.9f,.0001f,"no unnecessary split");
  Check(MantlePolicy.WithinReach(.2f,2f,1.75f,2f),"near reachable mantle");
  Check(!MantlePolicy.WithinReach(.9f,2f,1.75f,2f),"remote wall rejected");
  Check(!MantlePolicy.WithinReach(.2f,3f,1.75f,2f),"above body reach rejected");
  Check(!MantlePolicy.WithinReach(.2f,3.6f,1.75f,4f),"reference height cap");
  Check(MantlePolicy.WithinReach(.75f,2.6f,1.75f,2f),"moderate reach accepted");
  Console.WriteLine("TOTAL "+count);
 }
}



