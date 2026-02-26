using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CNRModLoader
{
    /// <summary>
    /// Simple method hooking system for CNR mods
    /// This provides a lightweight alternative to Harmony patching
    /// </summary>
    public static class CNRHooking
    {
        private static Dictionary<MethodInfo, List<MethodHook>> hooks = new Dictionary<MethodInfo, List<MethodHook>>();
        
        /// <summary>
        /// Represents a method hook
        /// </summary>
        public class MethodHook
        {
            public string ModName { get; set; }
            public Action<object[]> PreHook { get; set; }
            public Func<object[], object> ReplaceHook { get; set; }
            public Action<object[], object> PostHook { get; set; }
        }
        
        /// <summary>
        /// Hook a method by name and type
        /// </summary>
        public static void HookMethod(string typeName, string methodName, MethodHook hook)
        {
            try
            {
                Type targetType = Type.GetType(typeName);
                if (targetType == null)
                {
                    // Try to find the type in loaded assemblies
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        targetType = assembly.GetType(typeName);
                        if (targetType != null) break;
                    }
                }
                
                if (targetType == null)
                {
                    Debug.LogError($"[CNRHooking] Could not find type: {typeName}");
                    return;
                }
                
                MethodInfo method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method == null)
                {
                    Debug.LogError($"[CNRHooking] Could not find method: {methodName} in type {typeName}");
                    return;
                }
                
                HookMethod(method, hook);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CNRHooking] Failed to hook method {typeName}.{methodName}: {ex}");
            }
        }
        
        /// <summary>
        /// Hook a specific MethodInfo
        /// </summary>
        public static void HookMethod(MethodInfo method, MethodHook hook)
        {
            if (!hooks.ContainsKey(method))
            {
                hooks[method] = new List<MethodHook>();
            }
            
            hooks[method].Add(hook);
            Debug.Log($"[CNRHooking] Hooked method: {method.DeclaringType?.Name}.{method.Name} by mod: {hook.ModName}");
        }
        
        /// <summary>
        /// Execute pre-hooks for a method
        /// </summary>
        internal static void ExecutePreHooks(MethodInfo method, object[] args)
        {
            if (hooks.ContainsKey(method))
            {
                foreach (var hook in hooks[method])
                {
                    try
                    {
                        hook.PreHook?.Invoke(args);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[CNRHooking] Error in pre-hook for {method.Name} by {hook.ModName}: {ex}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Execute replacement hooks for a method
        /// Returns true if method should be replaced
        /// </summary>
        internal static bool ExecuteReplaceHooks(MethodInfo method, object[] args, out object result)
        {
            result = null;
            
            if (hooks.ContainsKey(method))
            {
                foreach (var hook in hooks[method])
                {
                    if (hook.ReplaceHook != null)
                    {
                        try
                        {
                            result = hook.ReplaceHook(args);
                            Debug.Log($"[CNRHooking] Method {method.Name} replaced by mod: {hook.ModName}");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[CNRHooking] Error in replace-hook for {method.Name} by {hook.ModName}: {ex}");
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Execute post-hooks for a method
        /// </summary>
        internal static void ExecutePostHooks(MethodInfo method, object[] args, object result)
        {
            if (hooks.ContainsKey(method))
            {
                foreach (var hook in hooks[method])
                {
                    try
                    {
                        hook.PostHook?.Invoke(args, result);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[CNRHooking] Error in post-hook for {method.Name} by {hook.ModName}: {ex}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Remove all hooks from a specific mod
        /// </summary>
        public static void RemoveModHooks(string modName)
        {
            foreach (var methodHooks in hooks.Values)
            {
                methodHooks.RemoveAll(h => h.ModName == modName);
            }
            
            Debug.Log($"[CNRHooking] Removed all hooks for mod: {modName}");
        }
        
        /// <summary>
        /// Get all hooked methods
        /// </summary>
        public static List<MethodInfo> GetHookedMethods()
        {
            return new List<MethodInfo>(hooks.Keys);
        }
        
        /// <summary>
        /// Helper method to find and hook Unity MonoBehaviour methods
        /// </summary>
        public static void HookUnityMethod(string gameObjectName, string componentType, string methodName, MethodHook hook)
        {
            try
            {
                GameObject target = GameObject.Find(gameObjectName);
                if (target == null)
                {
                    Debug.LogError($"[CNRHooking] Could not find GameObject: {gameObjectName}");
                    return;
                }
                
                Type compType = Type.GetType(componentType);
                if (compType == null)
                {
                    Debug.LogError($"[CNRHooking] Could not find component type: {componentType}");
                    return;
                }
                
                Component component = target.GetComponent(compType);
                if (component == null)
                {
                    Debug.LogError($"[CNRHooking] GameObject {gameObjectName} does not have component: {componentType}");
                    return;
                }
                
                MethodInfo method = compType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null)
                {
                    Debug.LogError($"[CNRHooking] Could not find method {methodName} in component {componentType}");
                    return;
                }
                
                HookMethod(method, hook);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CNRHooking] Failed to hook Unity method {gameObjectName}.{componentType}.{methodName}: {ex}");
            }
        }
    }
    
    /// <summary>
    /// Utility class for common game hooking operations
    /// </summary>
    public static class CNRGameHooks
    {
        /// <summary>
        /// Hook into CNRConnectMenu methods
        /// </summary>
        public static void HookConnectMenu(string methodName, CNRHooking.MethodHook hook)
        {
            CNRHooking.HookMethod("CNRConnectMenu", methodName, hook);
        }
        
        /// <summary>
        /// Hook into Photon networking methods
        /// </summary>
        public static void HookPhotonMethod(string methodName, CNRHooking.MethodHook hook)
        {
            CNRHooking.HookMethod("PhotonNetwork", methodName, hook);
        }
        
        /// <summary>
        /// Create a simple logging hook that logs method calls
        /// </summary>
        public static CNRHooking.MethodHook CreateLoggingHook(string modName, string description = "")
        {
            return new CNRHooking.MethodHook
            {
                ModName = modName,
                PreHook = (args) =>
                {
                    string argStr = args != null ? string.Join(", ", args) : "no args";
                    Debug.Log($"[{modName}] {description} called with: {argStr}");
                }
            };
        }
    }
}