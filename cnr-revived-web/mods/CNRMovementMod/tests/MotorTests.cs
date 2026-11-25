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
  Console.WriteLine("TOTAL "+count);
 }
}



