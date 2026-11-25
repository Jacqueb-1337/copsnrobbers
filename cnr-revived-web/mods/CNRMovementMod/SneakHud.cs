using System;
using UnityEngine;
namespace CNRMovementMod {
// Native NGUI button, not an IMGUI overlay. No original jump event handlers are copied.
public sealed class SneakHud {
 public const string Name="Image Button(Sneak)";
 GameObject button; UITexture widget; Texture2D texture; float nextTry;
 public void Tick(bool active, bool lowered, System.Action request, System.Action<GameObject> register, System.Func<GameObject> resolveFire){
  if(!active){Remove();return;}
  if(button!=null){
   if(widget!=null)widget.color=lowered?new Color(.72f,.94f,1f,1f):Color.white;
   if(register!=null)register(button);
   return;
  }
  if(Time.realtimeSinceStartup<nextTry)return;
  nextTry=Time.realtimeSinceStartup+.5f;
  GameObject panel=GameObject.Find("Panel(Control)");
  GameObject fire=resolveFire!=null?resolveFire():null;
  if(fire==null&&panel==null)return;
  Transform parent=panel!=null?panel.transform:fire.transform.parent;
  string[] fireNames={"FireButton","Image Button(Shoot)","Button(Shoot)","Image Button(Fire)","Button(Fire)"};
  if(fire==null&&panel!=null)foreach(Transform child in panel.GetComponentsInChildren<Transform>(true)){
   if(child==parent)continue;
   foreach(string name in fireNames)if(child.name==name){fire=child.gameObject;break;}
   if(fire!=null)break;
  }
  if(fire==null){foreach(string name in fireNames){fire=GameObject.Find(name);if(fire!=null)break;}}
  if(fire==null||fire.transform.parent==null)return;
  parent=fire.transform.parent;
  Bounds b=NGUIMath.CalculateRelativeWidgetBounds(parent,fire.transform);
  float size=Mathf.Min(Mathf.Abs(b.size.x),Mathf.Abs(b.size.y))*.85f;
  if(size<.0001f){
   Vector3 scale=fire.transform.localScale;
   size=Mathf.Min(Mathf.Abs(scale.x),Mathf.Abs(scale.y))*.85f;
   b=new Bounds(fire.transform.localPosition,new Vector3(Mathf.Abs(scale.x),Mathf.Abs(scale.y),0));
  }
  if(size<.0001f)return;
  Shader shader=Shader.Find("Unlit/Transparent Colored");
  if(shader==null)shader=Shader.Find("Unlit/Transparent");
  if(shader==null){Debug.LogWarning("[CNRMovement] Sneak HUD: no transparent shader");return;}
  // Factory position: left edge of fire, with a small gap, same vertical center.
  Vector3 center=b.center+Vector3.left*(b.extents.x+size*.65f);
  GameObject existing=GameObject.Find(Name);if(existing!=null)return;
  widget=NGUITools.AddWidget<UITexture>(parent.gameObject);button=widget.gameObject;button.name=Name;
  button.layer=fire.layer;
  UIWidget reference=fire.GetComponentInChildren<UIWidget>();
  if(reference!=null)widget.depth=reference.depth+1;
  button.transform.localPosition=center;button.transform.localScale=new Vector3(size,size,1);
  if(texture==null)texture=MakeIcon();
  widget.shader=shader;widget.mainTexture=texture;
  NGUITools.AddWidgetCollider(button);
  UIButton btn=button.AddComponent<UIButton>();btn.tweenTarget=button;btn.hover=Color.white;btn.pressed=new Color(.65f,.8f,1f,1f);
  UIEventListener.Get(button).onPress=delegate(GameObject go,bool down){if(down&&btn.enabled&&request!=null)request();};
  if(register!=null)register(button);
  Debug.Log("[CNRMovement] Sneak HUD created beside "+fire.name+" size="+size+" parent="+parent.name);
 }
 static float Segment(float x,float y,float ax,float ay,float bx,float by){
  float dx=bx-ax,dy=by-ay,t=Mathf.Clamp01(((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy));
  float ex=x-ax-t*dx,ey=y-ay-t*dy;return Mathf.Sqrt(ex*ex+ey*ey);
 }
 static Texture2D MakeIcon(){
  const int n=96;Texture2D t=new Texture2D(n,n,TextureFormat.RGBA32,false);t.name="CNR_SneakIcon";t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;
  Color32[] pixels=new Color32[n*n];
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){
   float px=x+.5f,py=y+.5f,r=Vector2.Distance(new Vector2(px,py),new Vector2(48,48));
   float a=Mathf.Clamp01(44-r);float ring=Mathf.Clamp01(2-Mathf.Abs(r-41));
   float head=Vector2.Distance(new Vector2(px,py),new Vector2(63,64))-8;
   float body=Mathf.Min(Segment(px,py,58,53,41,42),Segment(px,py,41,42,28,27));
   float leg=Mathf.Min(Segment(px,py,41,42,59,34),Segment(px,py,59,34,72,27));
   float arm=Mathf.Min(Segment(px,py,56,51,63,41),Segment(px,py,63,41,76,41));
   float ink=Mathf.Clamp01(4-Mathf.Min(Mathf.Min(body,leg),arm));
   ink=Mathf.Max(ink,Mathf.Clamp01(-head+1));
   byte v=(byte)Mathf.RoundToInt(Mathf.Lerp(28,235,Mathf.Max(ring*.7f,ink)));
   pixels[y*n+x]=new Color32(v,v,v,(byte)Mathf.RoundToInt(a*Mathf.Lerp(148,245,Mathf.Max(ring,ink))));
  }
  t.SetPixels32(pixels);t.Apply(false,true);return t;
 }
 public void Remove(){if(button!=null){button.SetActive(false);UnityEngine.Object.Destroy(button);}button=null;widget=null;}
 public void Dispose(){Remove();if(texture!=null)UnityEngine.Object.Destroy(texture);texture=null;}
}
}


