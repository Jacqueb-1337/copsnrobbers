using System;
using System.Reflection;
using UnityEngine;
namespace CNRMovementMod {
// The existing CNRMod environment controllers own their corrections; never run the
// experimental motor in parallel with them. No compile-time CNRMod dependency.
public sealed class EnvironmentPriority {
 Type ladderType,waterType,registryType;
 PropertyInfo climbing;
 FieldInfo wasSwimming;
 Func<Bounds,bool> overlaps;
 float nextResolve;
 const BindingFlags Instance=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 const BindingFlags Static=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 void Resolve(){
  if(Time.realtimeSinceStartup<nextResolve)return;
  nextResolve=Time.realtimeSinceStartup+1f;
  foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()){
   if(ladderType==null)ladderType=a.GetType("CNRMods.CNRMinecraftLadderController");
   if(waterType==null)waterType=a.GetType("CNRMods.CNRMinecraftWaterController");
   if(registryType==null)registryType=a.GetType("CNRMods.CNRMinecraftWaterRegistry");
  }
  if(ladderType!=null&&climbing==null)climbing=ladderType.GetProperty("IsClimbing",Instance);
  if(waterType!=null&&wasSwimming==null)wasSwimming=waterType.GetField("_wasSwimming",Instance);
  if(registryType!=null&&overlaps==null){
   MethodInfo method=registryType.GetMethod("Overlaps",Static,null,new[]{typeof(Bounds)},null);
   if(method!=null)overlaps=(Func<Bounds,bool>)Delegate.CreateDelegate(typeof(Func<Bounds,bool>),method);
  }
 }
 static Behaviour FindController(CharacterController cc,Type type){
  if(type==null)return null;
  for(Transform t=cc.transform;t!=null;t=t.parent){
   Behaviour b=t.GetComponent(type) as Behaviour;
   if(b!=null&&b.enabled&&b.gameObject.activeInHierarchy)return b;
  }
  return null;
 }
 public bool Owns(CharacterController cc){
  if(cc==null)return false;
  Resolve();
  // Climbable contact, including CNRMod's exit grace period, wins first.
  Behaviour ladder=FindController(cc,ladderType);
  if(ladder!=null&&(climbing==null||(bool)climbing.GetValue(ladder,null)))return true;
  Behaviour water=FindController(cc,waterType);
  if(water==null)return false;
  // Query the actual water volume before the custom LateUpdate, not a
  // stale global speed multiplier. Retain ownership through its exit cleanup.
  if(wasSwimming==null||overlaps==null)return true;
  if((bool)wasSwimming.GetValue(water))return true;
  Bounds body=cc.bounds;
  float bottom=body.min.y+Mathf.Max(body.size.y,.1f)*.1f;
  Bounds swimBody=new Bounds(new Vector3(body.center.x,(bottom+body.max.y)*.5f,body.center.z),
    new Vector3(body.size.x,Mathf.Max(body.max.y-bottom,.01f),body.size.z));
  return overlaps(swimBody);
 }
}
}

