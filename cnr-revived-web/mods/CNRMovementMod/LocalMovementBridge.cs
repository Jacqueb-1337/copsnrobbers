using System;
using System.Reflection;
using UnityEngine;
namespace CNRMovementMod {
// DLL-only controller. Never replaces or patches the game's assemblies.
// Owns only the verified local PlayerLogic controller while opted in.
public sealed class LocalMovementBridge : MonoBehaviour {
 readonly EnvironmentPriority environment=new EnvironmentPriority(); readonly Motor motor=new Motor(); readonly PostureAdapter posture=new PostureAdapter(); readonly SneakHud hud=new SneakHud();
 CharacterController bound; JoyStickController owner; SingleModeLadderPlayer ladder; CRJoyStickController crOwner; CRLadderPlayer crLadder; FPScontroller fpsOwner; Camera inputCam; FieldInfo crSpeedField,crControllerField;
 FieldInfo moveField,jumpField,ladderField,stopField,ccField,camField,fireField,lastFallField,lastHeightField;
 FieldInfo[] hooks; Delegate[] handlers; MethodInfo registerHud,hudBlocked,sprintBlocked,fallMethod;
 System.Action fallTick; bool ready,ownsController,failed; float nextBind,nextFind,footTime,pendingImpulse; int acquiredFrame=-1,queuedFrame=-100,slideFrame=-100;
 Quaternion previousYaw; bool hasYaw;
 const BindingFlags StaticPublic=BindingFlags.Public|BindingFlags.Static;
 const BindingFlags InstanceFields=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
 static Type Find(string name){foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()){Type t=a.GetType(name);if(t!=null)return t;}return null;}
 static bool Enabled(){return PlayerPrefs.GetInt("CNRMovementExperimental",0)==1;}
 static bool LocalPlayer(GameObject go){if(go==null||PlayerLogic.mInstance==null)return false;Transform t=PlayerLogic.mInstance.transform;return t==go.transform||t.IsChildOf(go.transform)||go.transform.IsChildOf(t); }
 bool CrCanMove(){return ready&&!failed&&Enabled()&&crOwner!=null&&crOwner==CRJoyStickController.mInstance&&CRPlayer.mInstance!=null&&crOwner.gameObject==CRPlayer.mInstance.gameObject&&CRGame.mInstance!=null&&CRGame.mInstance.m_Gamestate==CRGameState.InGame&&CRPlayer.mInstance.getLifeState()!=CRLifeState.Died&&bound!=null&&bound.enabled&&crOwner.gameObject.activeInHierarchy&&crLadder!=null&&!crLadder.latchedToLadder;}
 bool FpsCanMove(){return ready&&!failed&&Enabled()&&fpsOwner!=null&&bound!=null&&bound.enabled&&fpsOwner.gameObject.activeInHierarchy&&fpsOwner.gameObject.name=="ExampleCharacter"&&fpsOwner.canControl&&!fpsOwner.onLadder&&fpsOwner.movement!=null&&fpsOwner.jumping!=null&&(PlayerLogic.mInstance==null||!PlayerLogic.mInstance.bDied)&&(SingleModePauseControl.mInstance==null||!SingleModePauseControl.mInstance.PauseState)&&(fpsOwner.movingPlatform==null||fpsOwner.movingPlatform.activePlatform==null);}
 bool CanMove(){if(bound!=null&&environment.Owns(bound))return false;if(fpsOwner!=null)return FpsCanMove();if(crOwner!=null)return CrCanMove();return ready&&!failed&&Enabled()&&owner!=null&&bound!=null&&owner.gameObject.activeInHierarchy&&LocalPlayer(owner.gameObject)&&bound.enabled&&ladder!=null&&!ladder.latchedToLadder&&!(bool)stopField.GetValue(owner)&&PlayerLogic.mInstance!=null&&!PlayerLogic.mInstance.bDied&&(SingleModePauseControl.mInstance==null||!SingleModePauseControl.mInstance.PauseState);}
 bool Owns(){return ownsController&&CanMove()&&(fpsOwner!=null?!fpsOwner.enabled:crOwner!=null?!crOwner.enabled:owner!=null&&!owner.enabled);}
 bool QueueJump(){if(!Owns())return false;if(!HudBlocked()&&bound.isGrounded)queuedFrame=Time.frameCount;return true;}
 float Vertical(){return motor.Y+pendingImpulse;}
 void AddVertical(float v){if(Owns()&&!float.IsNaN(v)&&!float.IsInfinity(v))pendingImpulse+=v;}
 public void RequestSlide(){if(Owns()&&!HudBlocked())slideFrame=Time.frameCount;}
 bool HudBlocked(){return hudBlocked!=null&&(bool)hudBlocked.Invoke(null,null);}
 void RegisterHud(GameObject go){if(registerHud!=null)registerHud.Invoke(null,new object[]{go});}
 void TryBind(){
  try{
   Type j=typeof(JoyStickController),s=Find("CNRSettingsMod.SettingsModHook");if(s==null)return;
   registerHud=s.GetMethod("RegisterExperimentalMovementHud",StaticPublic);hudBlocked=s.GetMethod("ExperimentalMovementHudInputBlocked",StaticPublic);sprintBlocked=s.GetMethod("ExperimentalMovementSprintBlocked",StaticPublic);
   crSpeedField=typeof(CRJoyStickController).GetField("moveSpeed",InstanceFields);crControllerField=typeof(CRJoyStickController).GetField("charactercontroller",InstanceFields);
   moveField=j.GetField("moveSpeed",InstanceFields);jumpField=j.GetField("isJumping",InstanceFields);ladderField=j.GetField("isonladder",InstanceFields);stopField=j.GetField("cannotMove",InstanceFields);
   ccField=j.GetField("charactercontroller",InstanceFields);camField=j.GetField("cam",InstanceFields);fireField=j.GetField("fireStatusHoldTimeCount",InstanceFields);lastFallField=j.GetField("lastFallTime",InstanceFields);lastHeightField=j.GetField("lastHeightPosition",InstanceFields);fallMethod=j.GetMethod("UpdateFallDamage",InstanceFields);
   if(moveField==null||jumpField==null||ladderField==null||stopField==null||ccField==null||camField==null||fireField==null||lastFallField==null||lastHeightField==null||fallMethod==null||registerHud==null||hudBlocked==null||sprintBlocked==null)return;
   hooks=new FieldInfo[]{s.GetField("ExperimentalMovementQueueJump",StaticPublic),s.GetField("ExperimentalMovementOwnsJump",StaticPublic),s.GetField("ExperimentalMovementGetVertical",StaticPublic),s.GetField("ExperimentalMovementAddVertical",StaticPublic)};
   string[] names={"QueueJump","Owns","Vertical","AddVertical"};
   handlers=new Delegate[hooks.Length];for(int i=0;i<hooks.Length;i++){if(hooks[i]==null||hooks[i].GetValue(null)!=null)return;handlers[i]=Delegate.CreateDelegate(hooks[i].FieldType,this,GetType().GetMethod(names[i],InstanceFields));}
   for(int i=0;i<hooks.Length;i++)hooks[i].SetValue(null,handlers[i]);
   ready=true;Debug.Log("[CNRMovement] DLL-only bridge ready; global opt-in");
  }catch(Exception e){Unbind();Debug.LogWarning("[CNRMovement] binding failed: "+e.Message);}
 }
 void Unbind(){ready=false;if(hooks!=null&&handlers!=null)for(int i=0;i<hooks.Length;i++)if(hooks[i]!=null&&handlers[i]!=null&&object.Equals(hooks[i].GetValue(null),handlers[i]))hooks[i].SetValue(null,null);}
 void Release(){
  if(ownsController&&owner!=null){try{jumpField.SetValue(owner,false);ladderField.SetValue(owner,ladder!=null&&ladder.latchedToLadder);lastFallField.SetValue(owner,Time.time);lastHeightField.SetValue(owner,owner.transform.position.y);}finally{owner.enabled=true;}}
  if(ownsController&&crOwner!=null){crOwner.actionState=crLadder!=null&&crLadder.latchedToLadder?playerActionState.onladder:playerActionState.idle;crOwner.enabled=true;}
  if(ownsController&&fpsOwner!=null){try{
   if(fpsOwner.movement!=null){fpsOwner.movement.velocity=bound!=null?bound.velocity:Vector3.zero;}
   fpsOwner.grounded=bound!=null&&bound.isGrounded;
   if(fpsOwner.jumping!=null)fpsOwner.jumping.jumping=!fpsOwner.grounded&&motor.Y>0;
   fpsOwner.Running=false;fpsOwner.Walking=false;fpsOwner.crouch=false;
  }finally{fpsOwner.enabled=true;}}
  ownsController=false;posture.Restore();fpsOwner=null;crOwner=null;crLadder=null;owner=null;bound=null;ladder=null;inputCam=null;fallTick=null;motor.Reset();hasYaw=false;pendingImpulse=footTime=0;queuedFrame=slideFrame=-100;
 }
 void Fail(Exception e){failed=true;try{Release();}catch(Exception restore){Debug.LogWarning("[CNRMovement] restore: "+restore.Message);}Debug.LogWarning("[CNRMovement] disabled after error: "+e);}
 void Update(){try{
  if(!ready&&Time.realtimeSinceStartup>=nextBind){nextBind=Time.realtimeSinceStartup+1;TryBind();}
  if(ownsController&&(!CanMove()||(fpsOwner!=null?fpsOwner.enabled:crOwner!=null?crOwner.enabled:owner!=null&&owner.enabled)))Release();
  if(!ownsController)posture.Restore();
 }catch(Exception e){Fail(e);}}
 bool Acquire(){
  if(!ready||failed||!Enabled()||Time.realtimeSinceStartup<nextFind||HudBlocked())return false;
  nextFind=Time.realtimeSinceStartup+.25f;
  // Strictly one active stock controller; never steal a controller from another mod.
  JoyStickController candidate=null;
  foreach(UnityEngine.Object o in UnityEngine.Object.FindObjectsOfType(typeof(JoyStickController))){
   JoyStickController j=o as JoyStickController;if(j==null||!j.enabled||!j.gameObject.activeInHierarchy||!LocalPlayer(j.gameObject))continue;
   if(candidate!=null)return false;candidate=j;
  }
  if(candidate==null)return AcquireCR()||AcquireFPS();
  CharacterController cc=ccField.GetValue(candidate) as CharacterController;
  SingleModeLadderPlayer lp=candidate.GetComponent<SingleModeLadderPlayer>();
  if(cc==null||cc.gameObject!=candidate.gameObject||!cc.enabled||!cc.isGrounded||lp==null||lp.latchedToLadder||!(camField.GetValue(candidate) is Camera)||VCAnalogJoystickBase.GetInstance("stick")==null)return false;
  owner=candidate;bound=cc;ladder=lp;inputCam=(Camera)camField.GetValue(candidate);
  if(!CanMove()||!posture.Bind(cc)){owner=null;bound=null;ladder=null;inputCam=null;return false;}
  fallTick=(System.Action)Delegate.CreateDelegate(typeof(System.Action),owner,fallMethod);
  jumpField.SetValue(owner,false);ladderField.SetValue(owner,false);
  owner.enabled=false;ownsController=true;acquiredFrame=Time.frameCount;Debug.Log("[CNRMovement] local controller acquired without APK patch");return true;
 }
 bool AcquireCR(){
  CRJoyStickController j=CRJoyStickController.mInstance;
  if(j==null||!j.enabled||CRPlayer.mInstance==null||j.gameObject!=CRPlayer.mInstance.gameObject||crControllerField==null||crSpeedField==null)return false;
  CharacterController cc=crControllerField.GetValue(j) as CharacterController;CRLadderPlayer lp=j.GetComponent<CRLadderPlayer>();
  if(cc==null||cc.gameObject!=j.gameObject||!cc.enabled||!cc.isGrounded||lp==null||lp.latchedToLadder||VCAnalogJoystickBase.GetInstance("stick")==null)return false;
  crOwner=j;crLadder=lp;bound=cc;
  if(!CanMove()||!posture.Bind(cc)){crOwner=null;crLadder=null;bound=null;return false;}
  j.enabled=false;ownsController=true;acquiredFrame=Time.frameCount;Debug.Log("[CNRMovement] CR local controller acquired without APK patch");return true;
 }
 bool AcquireFPS(){
  GameObject local=GameObject.Find("ExampleCharacter");if(local==null)return false;
  FPScontroller fps=local.GetComponent<FPScontroller>();CharacterController cc=local.GetComponent<CharacterController>();
  if(fps==null||!fps.enabled||cc==null||!cc.enabled||!cc.isGrounded||fps.crouch||fps.prone||fps.movement==null)return false;
  // Do not take over a character driven by another enabled stock controller.
  JoyStickController j=local.GetComponent<JoyStickController>();CRJoyStickController cr=local.GetComponent<CRJoyStickController>();
  if((j!=null&&j.enabled)||(cr!=null&&cr.enabled))return false;
  fpsOwner=fps;bound=cc;
  if(!CanMove()||!posture.Bind(cc)){fpsOwner=null;bound=null;return false;}
  fps.enabled=false;ownsController=true;acquiredFrame=Time.frameCount;
  Debug.Log("[CNRMovement] FPS local controller acquired without APK patch");return true;
 }
 void LateUpdate(){try{
  if(!ownsController)Acquire();
  bool active=Owns();
  hud.Tick(ready&&!failed&&Enabled()&&(owner!=null||crOwner!=null||fpsOwner!=null||PlayerLogic.mInstance!=null||CRPlayer.mInstance!=null),motor.State==Motor.Mode.Slide||motor.State==Motor.Mode.Crouch,RequestSlide,RegisterHud);
  if(!active&&ownsController){Release();return;}
  if(!active||acquiredFrame==Time.frameCount)return; // stock Update already ran on acquisition frame
  if(!HudBlocked())Step();else{queuedFrame=slideFrame=-100;PlayerPrefs.SetInt("OnJump",0);}
  posture.Present(motor.LandingDip);
 }catch(Exception e){Fail(e);}}
 void OnLevelWasLoaded(int level){Release();failed=false;nextFind=0;}
 void OnDisable(){Release();hud.Remove();}
 void OnDestroy(){Release();hud.Dispose();Unbind();}
 void Step(){
  float dt=Mathf.Min(Time.deltaTime,.05f);if(dt<=0)return;
  VCAnalogJoystickBase stick=VCAnalogJoystickBase.GetInstance("stick");if(stick==null&&fpsOwner==null){Release();return;}
  bool cr=crOwner!=null,fps=fpsOwner!=null;
  float speed=fps?fpsOwner.movement.WalkSpeed:(float)(cr?crSpeedField.GetValue(crOwner):moveField.GetValue(owner));if(fps&&speed<=0)speed=fpsOwner.movement.maxForwardSpeed;if(speed<0||float.IsNaN(speed)||float.IsInfinity(speed)){Release();return;}
  if(fallTick!=null)fallTick();
  if(!CanMove()){Release();return;}
  Quaternion yaw=Quaternion.Euler(0,bound.transform.eulerAngles.y,0);
  if(hasYaw){Vector3 v=Quaternion.Inverse(yaw)*previousYaw*new Vector3(motor.X,0,motor.Z);motor.SetHorizontal(v.x,v.z);}
  previousYaw=yaw;hasYaw=true;motor.Mobility=speed/(Motor.ReferenceWalk*motor.UnitScale);
  bool grounded=bound.isGrounded;
  bool jump=grounded&&(Time.frameCount-queuedFrame<=1||(!cr&&!fps&&(bool)jumpField.GetValue(owner))||PlayerPrefs.GetInt("OnJump",0)==1||(cr&&(Input.GetButton("Jump")||(CRInput.mInstance!=null&&CRInput.mInstance.m_bJump)))||(fps&&fpsOwner.inputJump));
  if(jump){queuedFrame=-100;PlayerPrefs.SetInt("OnJump",0);}
  if(!cr&&!fps)jumpField.SetValue(owner,false);
  Vector3 input=fps?Quaternion.Inverse(yaw)*fpsOwner.inputMoveDirection:Vector3.zero;
  float x=stick!=null?stick.AxisX:input.x,z=stick!=null?stick.AxisY:input.z;
  bool firing=cr?UpdateCRStatus(x,z):fps?false:UpdateStatus(dt,x,z);
  bool sprint=(Input.GetKey(KeyCode.LeftShift)||(PlayerPrefs.GetInt("CNRMovementAutoSprint",1)==1&&z>.85f))&&!firing;
  if(sprint&&sprintBlocked!=null&&(bool)sprintBlocked.Invoke(null,null))sprint=false;
  bool slide=Input.GetKeyDown(KeyCode.LeftControl)||Time.frameCount-slideFrame<=1;if(slide)slideFrame=-100;
  float before=motor.Y;bool headClear=posture.CanStand();
  float slope=grounded&&(motor.State==Motor.Mode.Slide||slide)?posture.Downhill(yaw*new Vector3(motor.X,0,motor.Z)):0;
  motor.Tick(dt,x,z,grounded,sprint,slide,jump,headClear,slope);
  posture.Apply(dt,motor.State,headClear);
  float dy=(!grounded&&!jump)?(before+motor.Y)*.5f:motor.Y;
  if(pendingImpulse!=0){motor.AddVertical(pendingImpulse);dy+=pendingImpulse;pendingImpulse=0;}
  Vector3 delta=yaw*new Vector3(motor.X,0,motor.Z)*dt;delta.y=dy*dt;
  CollisionFlags flags=bound.Move(delta);
  if((flags&CollisionFlags.Above)!=0)motor.HitCeiling();
  if(cr&&motor.State==Motor.Mode.Air&&!firing)crOwner.actionState=playerActionState.jump;
  if(fps){fpsOwner.movement.velocity=bound.velocity;fpsOwner.movement.collisionFlags=flags;fpsOwner.grounded=(flags&CollisionFlags.Below)!=0;fpsOwner.jumping.jumping=!fpsOwner.grounded&&motor.Y>0;fpsOwner.Walking=motor.Speed>.05f;fpsOwner.Running=motor.State==Motor.Mode.Sprint;fpsOwner.crouch=motor.State==Motor.Mode.Crouch||motor.State==Motor.Mode.Slide;}
 }
 bool UpdateCRStatus(float x,float z){
  bool firing=crOwner.fireFlag||(CRInput.mInstance!=null&&CRInput.mInstance.m_bFire);
  bool moving=x*x+z*z>.0025f;
  crOwner.actionState=moving?(firing?playerActionState.walkattack:playerActionState.walk):(firing?playerActionState.idleattack:playerActionState.idle);
  crOwner.fireFlag=false;return firing;
 }
 // Keep the stock controller's non-movement bookkeeping while its Update is disabled.
 bool UpdateStatus(float dt,float x,float z){
  bool walking=x*x+z*z>.0025f;PlayerPrefs.SetInt("moveStatus",walking?1:0);
  if(walking){footTime+=dt;if(footTime>=.4f){owner.playFootStepAudio();footTime=0;}}else footTime=0;
  float hold=Mathf.Max(0,(float)fireField.GetValue(owner)-dt);bool fire=false;
  if(inputCam!=null&&NGUI.mInstance!=null&&!NGUI.mInstance.noClips)
   for(int i=0;i<Input.touchCount;i++){Touch t=Input.GetTouch(i);if(t.phase==TouchPhase.Ended||t.phase==TouchPhase.Canceled)continue;
    RaycastHit hit;if(Physics.Raycast(inputCam.ScreenPointToRay(t.position),out hit,18f)&&hit.collider.gameObject.name=="FireButton"){fire=true;hold=.235f;break;}}
  if(fire){WeaponType w=PlayerLogic.mInstance.mWeaponType;PlayerLogic.mInstance.mStatus=(w==WeaponType.BallisticKnife||w==WeaponType.GingerbreadKnife)?PlayerStatus.knifeFire:PlayerStatus.fire;}
  else if(hold<=0)PlayerLogic.mInstance.mStatus=walking?PlayerStatus.walk:PlayerStatus.idle;
  fireField.SetValue(owner,hold);return fire||hold>0;
 }
}
}


