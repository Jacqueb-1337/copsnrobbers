using System;
using System.Reflection;
using UnityEngine;
namespace CNRMovementMod {
public sealed class MovementSettings {
 float nextTry;
 MethodInfo section,hint,toggle,space;
 bool registered;
 public void Tick(){
  if(registered||Time.realtimeSinceStartup<nextTry)return;
  nextTry=Time.realtimeSinceStartup+1f;
  try{
   foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()){
    Type tabs=a.GetType("CNRSettingsMod.SettingsExternalTabs"),ui=a.GetType("CNRSettingsMod.SettingsUI");
    if(tabs==null||ui==null)continue;
    MethodInfo add=tabs.GetMethod("RegisterTab",new[]{typeof(string),typeof(string),typeof(Action<float>)});
    section=ui.GetMethod("Section",new[]{typeof(string)});
    hint=ui.GetMethod("Hint",new[]{typeof(string)});
    toggle=ui.GetMethod("Toggle",new[]{typeof(string),typeof(bool)});
    space=ui.GetMethod("Space",new[]{typeof(float)});
    if(add==null||section==null||hint==null||toggle==null||space==null)continue;
    add.Invoke(null,new object[]{"CNRMovementMod","Movement",new Action<float>(Draw)});
    registered=true;Debug.Log("[CNRMovement] Movement tab registered through Settings API");return;
   }
  }catch(Exception e){Debug.LogWarning("[CNRMovement] settings registration: "+e.Message);}
 }
 void Toggle(string label,string key,int defaultValue){
  bool old=PlayerPrefs.GetInt(key,defaultValue)==1;
  bool next=(bool)toggle.Invoke(null,new object[]{label,old});
  if(old!=next){PlayerPrefs.SetInt(key,next?1:0);PlayerPrefs.Save();}
 }
 void Draw(float width){
  section.Invoke(null,new object[]{"Experimental movement"});
  Toggle("CODM-inspired movement","CNRMovementExperimental",0);
  hint.Invoke(null,new object[]{"Optional sprint, slide, crouch, jump and camera posture. CNRMod water and ladder physics take priority."});
  space.Invoke(null,new object[]{10f});
  Toggle("Forward auto-sprint","CNRMovementAutoSprint",1);
  hint.Invoke(null,new object[]{"Push the joystick forward to sprint. Tap Sneak / Slide to crouch or slide."});
 }
}
}

