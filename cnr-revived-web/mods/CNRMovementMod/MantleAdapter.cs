using UnityEngine;
namespace CNRMovementMod {
// Collision-respecting local traversal. All geometry is in world units;
// reference dimensions are scaled by the experimental motor's UnitScale.
public sealed class MantleAdapter {
 CharacterController cc;
 Vector3 start,raised,across,destination;
 float elapsed,duration,radius,height,nextProbe,referenceHeight,worldScale;
 Vector3 lastHorizontal; public Vector3 ExitHorizontal {get;private set;} public bool Completed {get;private set;}
 public bool Active {get;private set;}
 public MantlePolicy.Kind Kind {get;private set;}
 public void Cancel(){Active=false;Kind=MantlePolicy.Kind.None;Completed=false;ExitHorizontal=Vector3.zero;lastHorizontal=Vector3.zero;}
 bool Solid(Collider c){
  return c!=null&&!c.isTrigger&&c!=cc&&!c.transform.IsChildOf(cc.transform)
    &&!cc.transform.IsChildOf(c.transform)&&!Physics.GetIgnoreLayerCollision(cc.gameObject.layer,c.gameObject.layer);
 }
 bool StaticSurface(Collider c){return Solid(c)&&c.attachedRigidbody==null&&!(c is CharacterController);}
 bool Ray(Vector3 origin,Vector3 direction,float distance,out RaycastHit nearest){
  nearest=new RaycastHit();float best=float.MaxValue;
  foreach(RaycastHit h in Physics.RaycastAll(origin,direction,distance)){
   if(Solid(h.collider)&&h.distance<best){nearest=h;best=h.distance;}
  }
  return best<float.MaxValue;
 }
 Vector3 Foot(){return cc.transform.TransformPoint(cc.center)-Vector3.up*(cc.height*.5f*Mathf.Abs(cc.transform.lossyScale.y));}
 bool Clear(Vector3 feet){
  float r=Mathf.Max(.02f,radius-.035f);
  float a=r+.065f,b=Mathf.Max(a,height-r-.035f);
  int n=Mathf.Max(1,Mathf.CeilToInt((b-a)/(r*.65f)));
  for(int i=0;i<=n;i++)
   foreach(Collider c in Physics.OverlapSphere(feet+Vector3.up*Mathf.Lerp(a,b,(float)i/n),r))
    if(Solid(c))return false;
  return true;
 }
 bool SegmentClear(Vector3 from,Vector3 to){
  Vector3 delta=to-from;float length=delta.magnitude;
  if(length<.001f)return Clear(to);
  float r=Mathf.Max(.02f,radius-.035f);
  float a=r+.065f,b=Mathf.Max(a,height-r-.035f);
  int n=Mathf.Max(1,Mathf.CeilToInt((b-a)/(r*.65f)));
  for(int i=0;i<=n;i++)
   foreach(RaycastHit h in Physics.SphereCastAll(from+Vector3.up*Mathf.Lerp(a,b,(float)i/n),r,delta/length,length))
    if(Solid(h.collider))return false;
  return Clear(to);
 }
 bool Surface(Vector3 p,float top,float bottom,out RaycastHit hit){
  Vector3 origin=new Vector3(p.x,top,p.z);
  return Ray(origin,Vector3.down,top-bottom,out hit)&&StaticSurface(hit.collider)&&hit.normal.y>=.7f;
 }
 bool Support(Vector3 p,Vector3 right,float y,float scale){
  RaycastHit left,rightHit;
  return Surface(p-right*radius*.65f,y+.12f*scale,y-.15f*scale,out left)
      &&Surface(p+right*radius*.65f,y+.12f*scale,y-.15f*scale,out rightHit);
 }
 bool MakePath(Vector3 foot,Vector3 end,float top,float scale){
  start=foot;destination=end+Vector3.up*.075f;
  raised=new Vector3(foot.x,top+.12f*scale,foot.z);
  across=new Vector3(end.x,raised.y,end.z);
  return Clear(raised)&&SegmentClear(start,raised)&&SegmentClear(raised,across)&&SegmentClear(across,destination);
 }
 public bool TryStart(CharacterController controller,Vector3 forward,float scale,float standingHeight){
  if(Active||Time.realtimeSinceStartup<nextProbe)return false;
  nextProbe=Time.realtimeSinceStartup+.10f;
  cc=controller;if(cc==null||!cc.enabled||scale<=0)return false;worldScale=scale;
  forward.y=0;if(forward.sqrMagnitude<.01f)return false;forward.Normalize();
  Vector3 right=Vector3.Cross(Vector3.up,forward);
  Vector3 lossy=cc.transform.lossyScale;
  radius=cc.radius*Mathf.Max(Mathf.Abs(lossy.x),Mathf.Abs(lossy.z));
  height=Mathf.Max(standingHeight*Mathf.Abs(lossy.y),radius*2);
  Vector3 foot=Foot();float maximum=2.3f*scale;
  RaycastHit face=new RaycastHit();
  bool found=false;float nearest=float.MaxValue;
  // Two height samples catch low barriers and taller ledges.
  foreach(float probe in new[]{Mathf.Max(.25f*scale,cc.stepOffset+.05f),Mathf.Min(height*.65f,1.15f*scale)}){
   RaycastHit h;
   if(Ray(foot+Vector3.up*probe,forward,radius+1f*scale,out h)&&StaticSurface(h.collider)
       &&h.normal.y<.45f&&Vector3.Dot(h.normal,forward)<-.4f&&h.distance<nearest){
    face=h;nearest=h.distance;found=true;
   }
  }
  if(!found)return false;
  Vector3 edge=new Vector3(face.point.x,foot.y,face.point.z);
  RaycastHit top;
  if(!Surface(edge+forward*.10f*scale,foot.y+maximum+.06f,foot.y+.15f*scale,out top))return false;
  float rise=top.point.y-foot.y;
  if(rise<Mathf.Max(cc.stepOffset+.06f,.20f*scale)||rise>maximum)return false;
  // Measure the top's continuous depth, rejecting gaps and steep/nonstatic surfaces.
  float depth=.10f*scale,increment=.10f*scale,limit=Mathf.Max(1.2f*scale+radius,2*radius+.25f*scale);
  for(float d=depth+increment;d<=limit;d+=increment){
   RaycastHit h;
   if(!Surface(edge+forward*d,top.point.y+.10f*scale,top.point.y-.12f*scale,out h))break;
   depth=d;
  }
  // A low obstacle with a clear, supported far-side landing is a vault.
  float offset=depth+radius+.15f*scale;
  if(MantleTiming.VaultHeight(rise/scale)&&offset<=1.2f*scale){
   Vector3 far=edge+forward*offset;RaycastHit landing;
   if(Surface(far,top.point.y-.16f*scale,foot.y-.55f*scale,out landing)&&Support(far,right,landing.point.y,scale)
      &&MakePath(foot,landing.point,top.point.y,scale)){
    var c=new MantlePolicy.Candidate{Height=rise,Width=depth,ForwardOffset=offset,
      ApproachClear=true,TopClear=true,CeilingClear=true,DestinationClear=true,LandingClear=true,PathClear=true,VaultEligible=true};
    if(MantlePolicy.Choose(c,scale)==MantlePolicy.Kind.Vault){Begin(MantlePolicy.Kind.Vault,rise/scale);return true;}
   }
  }
  // Climb-up requires a stable landing far enough onto the top for the full capsule.
  float standDepth=Mathf.Max(radius+.12f*scale,.25f*scale);
  Vector3 stand=edge+forward*standDepth;RaycastHit support;
  if(depth<standDepth||!Surface(stand,top.point.y+.1f*scale,top.point.y-.12f*scale,out support)
    ||!Support(stand,right,support.point.y,scale)||!MakePath(foot,support.point,top.point.y,scale))return false;
  var climb=new MantlePolicy.Candidate{Height=rise,Width=depth,ApproachClear=true,TopClear=true,
   CeilingClear=true,DestinationClear=true,PathClear=true,LandingClear=true};
  if(MantlePolicy.Choose(climb,scale)!=MantlePolicy.Kind.ClimbUp)return false;
  Begin(MantlePolicy.Kind.ClimbUp,rise/scale);return true;
 }
 void Begin(MantlePolicy.Kind kind,float referenceHeight){
  Kind=kind;Active=true;elapsed=0;this.referenceHeight=referenceHeight;Completed=false;ExitHorizontal=lastHorizontal=Vector3.zero;
  duration=MantleTiming.Duration(kind,referenceHeight);
  Debug.Log("[CNRMovement] "+kind+" height="+referenceHeight.ToString("F2"));
 }
 Vector3 Sample(float t){
  return t<.45f?Vector3.Lerp(start,raised,Mathf.SmoothStep(0,1,t/.45f)):
   t<.82f?Vector3.Lerp(raised,across,Mathf.SmoothStep(0,1,(t-.45f)/.37f)):
   Vector3.Lerp(across,destination,Mathf.SmoothStep(0,1,(t-.82f)/.18f));
 }
 bool AtDestination(Vector3 feet){
  Vector3 delta=feet-destination;
  return delta.x*delta.x+delta.z*delta.z<=.035f*.035f
   &&Mathf.Abs(delta.y)<=.20f*worldScale;
 }
 void Complete(bool moving){
  Completed=true;Active=false;Kind=MantlePolicy.Kind.None;
  ExitHorizontal=moving?lastHorizontal:Vector3.zero;
 }
 public CollisionFlags Tick(float dt,bool movementInput){
  if(!Active||cc==null||!cc.enabled||duration<=0){Cancel();return CollisionFlags.None;}
  float progress=elapsed/duration;
  float end=Mathf.Min(1f,(elapsed+dt)/duration);
  CollisionFlags flags=CollisionFlags.None;
  // Integrate authored path deltas, rather than correcting toward an absolute point after a collision.
  // Split at phase boundaries so low FPS cannot shortcut the collision-checked path.
  while(progress<end){
   float next=MantleTiming.NextSample(progress,end);
   Vector3 delta=Sample(next)-Sample(progress);
   Vector3 before=Foot();
   CollisionFlags stepFlags=cc.Move(delta);flags|=stepFlags;
   Vector3 actual=Foot(),moved=actual-before;
   if((moved-delta).magnitude>Mathf.Max(.035f,radius*.12f)
     ||(stepFlags&CollisionFlags.Above)!=0){Cancel();return flags;}
   float slice=(next-progress)*duration;
   Vector3 planar=new Vector3(moved.x,0,moved.z);
   if(slice>.0001f&&planar.sqrMagnitude>.0001f)lastHorizontal=planar/slice;
   progress=next;
  }
  elapsed=end*duration;
  Vector3 foot=Foot();
  if(Kind==MantlePolicy.Kind.ClimbUp&&movementInput&&AtDestination(foot)
      &&end>.45f){
   // Only hand off early once the destination is reached and actually supports the capsule.
   float settle=Mathf.Max(0,foot.y-destination.y+.10f);
   if(settle<=.20f*worldScale+.10f){
    RaycastHit support;
    if(Surface(foot,foot.y+.02f,foot.y-settle-.04f,out support)
      &&MantleTiming.CanResumeClimb(referenceHeight,end,movementInput,true,true)){
     flags|=cc.Move(Vector3.down*settle);
     if(cc.isGrounded){Complete(true);return flags;}
    }
   }
  }
  if(end>=1f){
   flags|=cc.Move(Vector3.down*.10f);
   if(AtDestination(Foot())&&cc.isGrounded)Complete(movementInput);
   else Cancel();
  }
  return flags;
 }
}
}
