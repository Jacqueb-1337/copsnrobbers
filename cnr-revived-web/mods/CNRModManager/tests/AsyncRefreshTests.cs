using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Threading;
class Test {
 static int checks;
 static void Check(bool b,string s){if(!b)throw new Exception(s);checks++;Console.WriteLine("PASS "+s);}
 static Type T;
 static IList Parse(string s){object[] a={s,null};var r=(IList)T.GetMethod("ParseRepoJson",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,a);if(r.Count==0)Console.WriteLine("status="+a[1]);return r;}
 static string F(object o,string key){return (string)o.GetType().GetField(key).GetValue(o);}
 static void Main(string[] args){
  AppDomain.CurrentDomain.AssemblyResolve += delegate(object o,ResolveEventArgs e){string f=Path.Combine(args[0],new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
  T=Assembly.LoadFrom(args[1]).GetType("CNRModManager.ModManagerHook",true);
  string fixture="{\"mods\":[{\"id\":\"x\",\"name\":\"X\",\"filename\":\"X.dll\",\"latestVersion\":\"2.1\",\"latestUrl\":\"https://example.com/x.dll\",\"dependencies\":[{\"id\":\"d\",\"minVersion\":\"5.0\"}],\"versions\":[{\"version\":\"2.1\",\"url\":\"https://example.com/x.dll\",\"changelog\":\"test\"}]},{\"id\":\"dup\",\"filename\":\"X.dll\"}]}";
  var m=Parse(fixture);Check(m.Count==1,"deduplication");Check(F(m[0],"latestVersion")=="2.1","version");Check(F(m[0],"minVersion")=="","nested dependency not treated as mod minimum");
  Check(Parse("{}").Count==0,"invalid manifest");
  var jt=T.GetNestedType("CatalogParseJob",BindingFlags.NonPublic);
  foreach(string path in new[]{args[2],args[3]}){
   string json=File.ReadAllText(path);
   var sync=Parse(json);Check(sync.Count>0,"live manifest parses "+Path.GetFileName(path));
   var j=Activator.CreateInstance(jt,true);
   jt.GetField("json").SetValue(j,json);
   var run=(WaitCallback)Delegate.CreateDelegate(typeof(WaitCallback),j,jt.GetMethod("Run"));
   Check(ThreadPool.QueueUserWorkItem(run),"worker accepted");
   var stop=DateTime.UtcNow.AddSeconds(15);
   while(!(bool)jt.GetField("done").GetValue(j)&&DateTime.UtcNow<stop)Thread.Sleep(1);
   Check((bool)jt.GetField("done").GetValue(j),"worker completes");
   var async=(IList)jt.GetField("mods").GetValue(j);
   Check(async.Count==sync.Count,"worker result count equals sync");
   int versions=0;
   for(int i=0;i<sync.Count;i++){Check(F(async[i],"filename")==F(sync[i],"filename"),"worker result "+F(sync[i],"filename"));versions+=((IList)sync[i].GetType().GetField("versions").GetValue(sync[i])).Count;}
   Console.WriteLine("CATALOG "+sync.Count+" MODS "+versions+" VERSIONS");
  }
  Console.WriteLine("TOTAL "+checks);
 }
}