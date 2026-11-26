using System;
using System.Collections;
using System.IO;
using System.Reflection;
class WireTest {
 static void Check(bool b,string s){if(!b)throw new Exception(s);Console.WriteLine("PASS "+s);}
 static int comparisons;
 static void Equal(object a,object b,string path) {
   if(a==null || b==null){if(a!=b)throw new Exception(path+" null");return;}
   IList al=a as IList,bl=b as IList;
   if(al!=null){if(bl==null||al.Count!=bl.Count)throw new Exception(path+" count "+al.Count+" vs "+(bl==null?-1:bl.Count));for(int i=0;i<al.Count;i++)Equal(al[i],bl[i],path+"["+i+"]");return;}
   Type t=a.GetType();
   if(t==typeof(string)||t.IsPrimitive){if(!a.Equals(b))throw new Exception(path+" mismatch: "+a+" vs "+b);comparisons++;return;}
   foreach(FieldInfo f in t.GetFields(BindingFlags.Public|BindingFlags.Instance))Equal(f.GetValue(a),f.GetValue(b),path+"."+f.Name);
 }
 static void Main(string[] args){
  AppDomain.CurrentDomain.AssemblyResolve+=delegate(object o,ResolveEventArgs e){string f=Path.Combine(args[0],new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
  Type t=Assembly.LoadFrom(args[1]).GetType("CNRModManager.ModManagerHook",true);
  Type wire=t.GetNestedType("JsonWire",BindingFlags.NonPublic);
  MethodInfo read=wire.GetMethod("Read");
  for(int i=2;i<args.Length;i+=2){
   byte[] data=File.ReadAllBytes(args[i+1]); object root=read.Invoke(null,new object[]{data});
   object[] direct={File.ReadAllText(args[i]),null};object[] native={root,null};
   IList a=(IList)t.GetMethod("ParseRepoJson",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,direct);
   IList b=(IList)t.GetMethod("ParseRepoObject",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,native);
   Check(a.Count==10 && b.Count==10,"both paths parse 10 mods");
   Equal(a,b,"catalog");
   int versions=0;foreach(object v in b)versions+=((IList)v.GetType().GetField("versions").GetValue(v)).Count;
   Check(versions==828,"all 828 version entries preserved");
   Console.WriteLine("PASS full field parity for "+Path.GetFileName(args[i]));
   try {read.Invoke(null,new object[]{new byte[]{80,86,74,1,5,0,0}});throw new Exception("truncated input accepted");}
   catch(TargetInvocationException){Console.WriteLine("PASS truncated wire rejected");}
  }
  var nj=t.GetNestedType("NativeJobs",BindingFlags.NonPublic);
  uint id=(uint)nj.GetMethod("TrySubmitJson").Invoke(null,new object[]{new byte[]{123,125}});
  Check(id==0,"legacy runtime without native ABI safely falls back");
  Console.WriteLine("COMPARED "+comparisons+" scalar fields");
 }
}