using UnityEngine;
namespace CNRMovementMod {
// Offline candidate only. Camera is selected strictly from the local player's descendants.
// FOV is deliberately not owned here; CNRSettingsMod owns scope/aim FOV.
public sealed class PostureAdapter {
 readonly Posture posture=new Posture();
 CharacterController cc; Transform view;
 float height; Vector3 center, lastCameraPosition, appliedCameraOffset;
 bool cameraApplied;
 public bool Bind(CharacterController controller){
  if(cc==controller)return true; if(!Restore())return false; cc=controller; height=cc.height; center=cc.center;
  Camera main=Camera.main;
  if(main!=null&&main.transform.IsChildOf(cc.transform))view=main.transform;
  if(view==null){
   Camera[] cams=cc.GetComponentsInChildren<Camera>();
   foreach(Camera cam in cams)if(cam.enabled&&!cam.orthographic){if(view!=null){view=null;break;}view=cam.transform;}
  }
  return true;
 }
 public float StandingHeight {get{return height;}}
 public bool CanStand(){
  if(cc==null)return true;
  float remaining=height-cc.height;if(remaining<=.001f)return true;
  Vector3 scale=cc.transform.lossyScale;
  float radius=Mathf.Max(.01f,(cc.radius*.95f)*Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.z)));
  Vector3 up=cc.transform.up;
  Vector3 top=cc.transform.TransformPoint(cc.center+Vector3.up*Mathf.Max(0,cc.height*.5f-cc.radius));
  RaycastHit[] hits=Physics.SphereCastAll(top,radius,up,remaining*Mathf.Abs(scale.y));
  foreach(RaycastHit hit in hits){
   Collider col=hit.collider;
   if(col!=null&&!col.isTrigger&&col.transform!=cc.transform&&!col.transform.IsChildOf(cc.transform))return false;
  }
  return true;
 }
 public float Downhill(Vector3 direction){
  if(cc==null||direction.sqrMagnitude<.001f)return 0;
  Vector3 centerWorld=cc.transform.TransformPoint(cc.center);
  float half=cc.height*.5f*Mathf.Abs(cc.transform.lossyScale.y);
  RaycastHit[] hits=Physics.RaycastAll(centerWorld,Vector3.down,half+.35f);
  float best=float.MaxValue;Vector3 normal=Vector3.up;
  foreach(RaycastHit hit in hits){
   Collider col=hit.collider;
   if(col==null||col.isTrigger||col.transform==cc.transform||col.transform.IsChildOf(cc.transform)||hit.normal.y<.35f)continue;
   if(hit.distance<best){best=hit.distance;normal=hit.normal;}
  }
  direction.y=0;return Vector3.Dot(direction.normalized,normal);
 }
 public void Apply(float dt,Motor.Mode mode,bool clear){
  if(cc==null)return;
  float fraction=posture.Tick(dt,mode,clear);
  float wanted=Mathf.Max(cc.radius*2,height*(1-fraction));
  if(wanted>cc.height&&!clear)wanted=cc.height;
  if(Mathf.Abs(wanted-cc.height)>.0001f){
   cc.height=wanted;
   cc.center=center+Vector3.up*((wanted-height)*.5f);
  }
 }
 public void Present(float landingDip){
  if(cc==null||view==null)return;
  RemoveCameraOffset();
  // Convert world up to the camera parent's local coordinates.
  Vector3 drop=-cc.transform.up*((height-cc.height)*.9f*Mathf.Abs(cc.transform.lossyScale.y)+landingDip);
  appliedCameraOffset=view.parent!=null?view.parent.InverseTransformVector(drop):drop;
  view.localPosition+=appliedCameraOffset;lastCameraPosition=view.localPosition;cameraApplied=true;
 }
 void RemoveCameraOffset(){
  if(view!=null&&cameraApplied&&(view.localPosition-lastCameraPosition).sqrMagnitude<.000001f)view.localPosition-=appliedCameraOffset;
  cameraApplied=false;
 }
 public bool Restore(){
  if(cc!=null&&!CanStand())return false;
  RemoveCameraOffset();
  if(cc!=null){cc.height=height;cc.center=center;}
  cc=null;view=null;posture.Reset();return true;
 }
}
}




