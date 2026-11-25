using UnityEngine;
namespace CNRMovementMod {
public static class MovementEntry {
 public const string Version = "0.1.4";
 public static void Load(){
  if(GameObject.Find("CNRMovementExperimentalRoot")!=null)return;
  Debug.Log("[CNRMovement] "+Version+" loaded; global opt-in");
  var go=new GameObject("CNRMovementExperimentalRoot");
  go.AddComponent<LocalMovementBridge>();UnityEngine.Object.DontDestroyOnLoad(go);
 }
 // Optional local UI can call this; local input only.
 public static void RequestSlide(){LocalMovementBridge b=UnityEngine.Object.FindObjectOfType(typeof(LocalMovementBridge)) as LocalMovementBridge;if(b!=null)b.RequestSlide();}
}
}

