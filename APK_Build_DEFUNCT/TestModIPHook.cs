using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CNRMods
{
/// <summary>
/// Test mod that hooks Photon connection to use custom LAN server IP
/// </summary>
public class TestModIPHook : MonoBehaviour
{
private static bool _initialized = false;
private static string _customIP = "";
private static bool _showIPDialog = false;
private static string _inputBuffer = "";
private static TestModIPHook _instance = null;
private static bool _ipOverrideAttempted = false;

public static void Initialize()
{
string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Initialize() called\n");
}
catch { }

if (_initialized)
{
try { System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Already initialized\n"); } catch { }
return;
}

_initialized = true;
try { System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Creating GameObject\n"); } catch { }

Debug.Log("[TestModIPHook] Initializing IP hook mod");

try
{
var managerObj = new GameObject("CNRModIPManager");
_instance = managerObj.AddComponent<TestModIPHook>();
managerObj.AddComponent<IPDialogGUI>();
GameObject.DontDestroyOnLoad(managerObj);
Debug.Log("[TestModIPHook] Added GUI manager and IP hook");
try { System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] GameObject created successfully\n"); } catch { }
}
catch (Exception ex)
{
Debug.LogError("[TestModIPHook] Failed to initialize: " + ex.Message);
try { System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] ERROR in Initialize: " + ex.ToString() + "\n"); } catch { }
}
}

private void OnEnable()
{
string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] *** OnEnable ENTRY ***\n");
try { System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] OnEnable called\n"); } catch { }
Debug.Log("[TestModIPHook] OnEnable called");
// For testing, set a default IP right away so we can test reflection dump
SetCustomIP("192.168.193.116");
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] SetCustomIP completed, _customIP = " + _customIP + "\n");
try { System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Set default test IP\n"); } catch { }
// Immediately attempt override
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] About to call ImplementPhotonOverride()\n");
try
{
ImplementPhotonOverride();
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] ImplementPhotonOverride() returned successfully\n");
}
catch (System.Exception ex)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Exception calling ImplementPhotonOverride: " + ex.Message + " | " + ex.StackTrace + "\n");
}
}

private void Start()
{
Debug.Log("[TestModIPHook] Start called");
// Don't call anything here - OnEnable already did the override
}

private void Update()
{
// Keep trying until override succeeds - but don't keep retrying if already done
if (!string.IsNullOrEmpty(_customIP) && !_ipOverrideAttempted)
{
try
{
ImplementPhotonOverride();
}
catch (System.Exception ex)
{
Debug.LogError("[TestModIPHook] Update() exception: " + ex.ToString());
}
}

// Look for callbacks or events related to room joining
TryHookRoomCallbacks();
HookApplicationLoadLevel();

// Monitor for secondary connection attempts (when room game loads)
MonitorPhotonState();
}

private static bool _secondaryConnectAttempted = false;

private static void MonitorPhotonState()
{
string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
try
{
// Check if PhotonNetwork reports as connected to a different IP
var photonNetworkType = typeof(PhotonNetwork);
var connectedProp = photonNetworkType.GetProperty("connected", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var serverAddressProp = photonNetworkType.GetProperty("ServerAddress", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

if (connectedProp != null)
{
object connectedVal = connectedProp.GetValue(null);
if (connectedVal != null && connectedVal is bool && (bool)connectedVal)
{
// We're connected, log the server address if possible
if (serverAddressProp != null)
{
object serverAddr = serverAddressProp.GetValue(null);
if (serverAddr != null && !_secondaryConnectAttempted)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Photon Connected - Server: " + serverAddr.ToString() + "\n");
_secondaryConnectAttempted = true;
}
}
}
}
}
catch (System.Exception ex)
{
// Silently ignore - this is just monitoring
}
}

private static bool _roomCallbacksHooked = false;
private static bool _applicationHooked = false;

private static void HookApplicationLoadLevel()
{
if (_applicationHooked)
return;

string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === HOOKING APPLICATION.LOADLEVEL ===\n");
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] When room game loads, watch for PhotonNetwork reconnect calls\n");
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Update() will catch any new Connect() calls and redirect to LAN server\n");

_applicationHooked = true;
}
catch (System.Exception ex)
{
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Error: " + ex.ToString() + "\n");
}
catch { }
}
}

private static void TryHookRoomCallbacks()
{
if (_roomCallbacksHooked)
return;

string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
try
{
// Try to find and hook room-related callbacks
var photonNetworkType = typeof(PhotonNetwork);

// Look for a static OnJoinedRoom or similar callback
var properties = photonNetworkType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var fields = photonNetworkType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === SEARCHING FOR ROOM CALLBACKS ===\n");

// Check for room-related properties
foreach (var prop in properties)
{
if (prop.Name.ToLower().Contains("room") || prop.Name.ToLower().Contains("callback"))
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Property: " + prop.Name + " : " + prop.PropertyType.Name + "\n");
}
}

// Find the Room type and dump its methods/events
var assembly = photonNetworkType.Assembly;
var allTypes = assembly.GetTypes();
System.Type roomType = null;
foreach (var t in allTypes)
{
if (t.Name == "Room")
{
roomType = t;
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Found Room type: " + t.FullName + "\n");
var roomMethods = t.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly);
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Room has " + roomMethods.Length + " declared methods:\n");
foreach (var m in roomMethods)
{
var paramStr = "";
var parms = m.GetParameters();
foreach (var p in parms)
{
if (paramStr != "") paramStr += ", ";
paramStr += p.ParameterType.Name;
}
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "]   " + m.ReturnType.Name + " " + m.Name + "(" + paramStr + ")\n");
}

// Also check for events on Room type
var events = t.GetEvents();
if (events.Length > 0)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Room has " + events.Length + " events:\n");
foreach (var evt in events)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "]   EVENT: " + evt.Name + "\n");
}
}
break;
}
}

// Find callback interfaces (IPunOwnershipCallbacks, IPunObservable, etc)
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === CALLBACK INTERFACES ===\n");
int ifaceCount = 0;
foreach (var iface in allTypes)
{
string ifaceName = iface.Name;
if (ifaceName.StartsWith("I"))
{
ifaceCount++;
if (ifaceCount < 20 || ifaceName.Contains("Callback") || ifaceName.Contains("Observer") || ifaceName.Contains("Event") || ifaceName.Contains("Room"))
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Interface: " + iface.FullName + "\n");
var methods = iface.GetMethods();
if (methods.Length > 0)
{
foreach (var m in methods)
{
var paramStr = "";
var parms = m.GetParameters();
foreach (var p in parms)
{
if (paramStr != "") paramStr += ", ";
paramStr += p.ParameterType.Name;
}
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "]   " + m.Name + "(" + paramStr + ")\n");
}
}
}
}
}
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Total interfaces found: " + ifaceCount + "\n");

_roomCallbacksHooked = true;
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === END ROOM CALLBACK SEARCH ===\n");
}
catch (System.Exception ex)
{
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Error searching for room callbacks: " + ex.ToString() + "\n");
}
catch { }
}
}

private static void ImplementPhotonOverride()
{
string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
Debug.Log("[TestModIPHook] ImplementPhotonOverride() called - DEBUG TEST");

// Wrapped in try-catch to catch file write errors
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] *** ImplementPhotonOverride() CALLED ***\n");
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Check 1: _ipOverrideAttempted = " + _ipOverrideAttempted + "\n");
if (_ipOverrideAttempted)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Already attempted, returning\n");
return;
}

System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Check 2: _customIP = '" + _customIP + "'\n");
if (string.IsNullOrEmpty(_customIP))
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] No custom IP set, returning\n");
return;
}

_ipOverrideAttempted = true;

System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === IMPLEMENTING PHOTON OVERRIDE ===\n");
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Calling PhotonNetwork.Connect(" + _customIP + ", 5055, ...)\n");

// Disconnect first if connected
if (PhotonNetwork.connected)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Disconnecting from current server\n");
PhotonNetwork.Disconnect();
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Disconnected\n");
}

// Call PhotonNetwork.Connect() with custom server
// PhotonNetwork.Connect(String serverAddress, Int32 port, String appID, String gameVersion)
var photonNetworkType = typeof(PhotonNetwork);
var connectMethod = photonNetworkType.GetMethod("Connect", new System.Type[] { 
    typeof(string), 
    typeof(int), 
    typeof(string), 
    typeof(string) 
});

object isNull = (object)connectMethod;
if (isNull != null)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Connect() method found\n");
// Use correct appID "CopsNRobbers" from PhotonMessageSerializer
string appID = "CopsNRobbers";
string gameVersion = "1.0";
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Invoking Connect(" + _customIP + ", 5055, " + appID + ", " + gameVersion + ")\n");
connectMethod.Invoke(null, new object[] { _customIP, 5055, appID, gameVersion });
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Connect() invoked successfully!\n");
}
else
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] ERROR: Connect() method not found\n");
}
}
catch (System.Exception ex)
{
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] ERROR in ImplementPhotonOverride: " + ex.ToString() + "\n");
}
catch { }
Debug.LogError("[TestModIPHook] ImplementPhotonOverride error: " + ex.ToString());
}
}

private static void DumpPhotonRoomEvents()
{
string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === PHOTON ROOM EVENTS REFLECTION DUMP ===\n");

var photonNetworkType = typeof(PhotonNetwork);
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] PhotonNetwork Type: " + photonNetworkType.FullName + "\n");

// List all public static events
var bindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase;
var events = photonNetworkType.GetEvents(bindingFlags);
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] [EVENTS] PhotonNetwork has " + events.Length + " public static events:\n");
foreach (var evt in events)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "]   EVENT: " + evt.Name + " : " + evt.EventHandlerType + "\n");
}

// List all public static methods again, looking for room/join related ones
var publicMethods = photonNetworkType.GetMethods(bindingFlags);
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] [METHODS] Looking for room/join/connect related methods:\n");
foreach (var method in publicMethods)
{
string methodName = method.Name.ToLower();
if (methodName.Contains("room") || methodName.Contains("join") || methodName.Contains("create") || methodName.Contains("connect") || methodName.Contains("enter"))
{
var paramStr = "";
var parms = method.GetParameters();
foreach (var p in parms)
{
if (paramStr != "") paramStr += ", ";
paramStr += p.ParameterType.Name + " " + p.Name;
}
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "]   METHOD: " + method.ReturnType.Name + " " + method.Name + "(" + paramStr + ")\n");
}
}

// Look for IPunPrefabFactory and related room connection stuff
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Looking for room-related classes...\n");
var assembly = photonNetworkType.Assembly;
var types = assembly.GetTypes();
foreach (var t in types)
{
string typeName = t.Name.ToLower();
if (typeName.Contains("room") && (typeName.Contains("manager") || typeName.Contains("handler") || typeName.Contains("join")))
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "]   FOUND TYPE: " + t.FullName + "\n");
}
}

System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === END REFLECTION DUMP ===\n");
}
catch (System.Exception ex)
{
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] ERROR in DumpPhotonRoomEvents: " + ex.ToString() + "\n");
}
catch { }
Debug.LogError("[TestModIPHook] DumpPhotonRoomEvents error: " + ex.ToString());
}
}

private static void AttemptPhotonOverride()
{
string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
try
{
if (_ipOverrideAttempted)
return;

if (string.IsNullOrEmpty(_customIP))
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] AttemptPhotonOverride: No custom IP set yet\n");
return;
}

_ipOverrideAttempted = true;

System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === ATTEMPTING PHOTON OVERRIDE ===\n");
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] Target IP: " + _customIP + ":5055\n");
DumpPhotonRoomEvents();

// REFLECTION DUMP: Get all info about PhotonNetwork
var photonNetworkType = typeof(PhotonNetwork);

System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === PHOTON REFLECTION DUMP ===\n");
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] PhotonNetwork Type: " + photonNetworkType.FullName + "\n");

// List all public static fields
var staticFields = photonNetworkType.GetFields(BindingFlags.Public | BindingFlags.Static);
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] [FIELDS] PhotonNetwork has " + staticFields.Length + " public static fields:\n");
foreach (var field in staticFields)
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "]   FIELD: " + field.Name + " : " + field.FieldType.Name + "\n");
}

// List all public static properties
var staticProperties = photonNetworkType.GetProperties(BindingFlags.Public | BindingFlags.Static);
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] [PROPERTIES] PhotonNetwork has " + staticProperties.Length + " public static properties:\n");
foreach (var prop in staticProperties)
{
string line = "[" + System.DateTime.Now + "]   PROPERTY: " + prop.Name + " : " + prop.PropertyType.Name;
if (prop.Name.ToLower().Contains("server") || prop.Name.ToLower().Contains("setting"))
{
line += " <<< POTENTIAL TARGET";
}
System.IO.File.AppendAllText(logPath, line + "\n");
}

// List ALL public methods
var publicMethods = photonNetworkType.GetMethods(BindingFlags.Public | BindingFlags.Static);
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] [METHODS] PhotonNetwork has " + publicMethods.Length + " public static methods\n");
int methodCount = 0;
foreach (var method in publicMethods)
{
var paramStr = "";
var parms = method.GetParameters();
foreach (var p in parms)
{
if (paramStr != "") paramStr += ", ";
paramStr += p.ParameterType.Name + " " + p.Name;
}
string line = "[" + System.DateTime.Now + "]   METHOD: " + method.ReturnType.Name + " " + method.Name + "(" + paramStr + ")";
if (method.Name.ToLower().Contains("connect") || method.Name.ToLower().Contains("server") || method.Name.ToLower().Contains("address"))
{
line += " <<< POTENTIAL";
}
System.IO.File.AppendAllText(logPath, line + "\n");
methodCount++;
}

System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] === END REFLECTION DUMP ===\n");
}
catch (System.Exception ex)
{
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] ERROR in AttemptPhotonOverride: " + ex.ToString() + "\n");
}
catch { }
Debug.LogError("[TestModIPHook] AttemptPhotonOverride error: " + ex.ToString());
}
}

public static string GetCustomIP()
{
return _customIP;
}

public static void ShowIPDialog()
{
_showIPDialog = true;
_inputBuffer = string.IsNullOrEmpty(_customIP) ? "" : _customIP;
}

public static bool IsShowingDialog()
{
return _showIPDialog;
}

public static void SetCustomIP(string ip)
{
_customIP = ip;
_ipOverrideAttempted = false;  // Allow retry with new IP
Debug.Log("[TestModIPHook] Custom IP set to: " + ip);
}

public static void HideDialog()
{
_showIPDialog = false;
}

public static void CreateCustomTestRoom()
{
Debug.Log("[TestModIPHook] === Custom IP ready: " + _customIP + " ===");
Debug.Log("[TestModIPHook] Game will connect to this IP when you join a room");
Debug.Log("[TestModIPHook] Select a room and join it now");
}
}

/// <summary>
/// GUI component for IP input dialog
/// </summary>
public class IPDialogGUI : MonoBehaviour
{
private string ipInput = "192.168.193.116";
private bool dialogActive = false;
private int dialogWidth = 600;
private int dialogHeight = 200;
private Rect dialogRect;

private void Start()
{
dialogRect = new Rect(Screen.width / 2 - dialogWidth / 2, Screen.height / 2 - dialogHeight / 2, dialogWidth, dialogHeight);
Debug.Log("[IPDialogGUI] Initialized");
TestModIPHook.ShowIPDialog();
Debug.Log("[IPDialogGUI] Dialog shown on startup");
}

private void Update()
{
// Press 'I' to show IP dialog
if (Input.GetKeyDown(KeyCode.I) && !TestModIPHook.IsShowingDialog())
{
TestModIPHook.ShowIPDialog();
Debug.Log("[IPDialogGUI] Dialog reopened");
}
}

private void OnGUI()
{
if (!TestModIPHook.IsShowingDialog())
return;

// Draw background
GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

// Draw dialog
GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
dialogRect = GUI.Window(0, dialogRect, DrawDialog, "[LAN Server IP Input]");
}

private void DrawDialog(int windowID)
{
GUILayout.BeginVertical();

GUILayout.Label("LAN Server IP:", GUILayout.Height(30));
ipInput = GUILayout.TextField(ipInput, GUILayout.Height(40), GUILayout.ExpandWidth(true));

GUILayout.Space(20);
GUILayout.BeginHorizontal();

if (GUILayout.Button("Connect to LAN Server", GUILayout.Height(40)))
{
if (!string.IsNullOrEmpty(ipInput))
{
TestModIPHook.SetCustomIP(ipInput);
TestModIPHook.CreateCustomTestRoom();
TestModIPHook.HideDialog();
Debug.Log("[IPDialogGUI] Dialog closed - ready for room join");
}
}

if (GUILayout.Button("Cancel", GUILayout.Height(40)))
{
TestModIPHook.HideDialog();
}

GUILayout.EndHorizontal();
GUILayout.EndVertical();

GUI.DragWindow(new Rect(0, 0, 10000, 20));
}
}

/// <summary>
/// Entry point for mod loader
/// </summary>
public class ModEntry
{
public static void Load()
{
string logPath = "/storage/emulated/0/CNRMods/mod_debug.log";
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] MOD ENTRY REACHED\n");
Debug.Log("[TestModIPHook] *** MOD ENTRY POINT REACHED ***");
TestModIPHook.Initialize();
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] INIT COMPLETE\n");
Debug.Log("[TestModIPHook] *** MOD INITIALIZATION COMPLETE ***");
}
catch (System.Exception ex)
{
try
{
System.IO.File.AppendAllText(logPath, "[" + System.DateTime.Now + "] EXCEPTION: " + ex.ToString() + "\n");
}
catch { }
Debug.LogError("[TestModIPHook] *** EXCEPTION IN MODENTRY.LOAD: " + ex.ToString());
}
}
}
}
