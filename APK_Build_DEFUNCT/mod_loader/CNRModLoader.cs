using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRModLoader
{
    /// <summary>
    /// Interface that all CNR mods must implement
    /// </summary>
    public interface ICNRMod
    {
        /// <summary>
        /// Mod information
        /// </summary>
        string ModName { get; }
        string ModVersion { get; }
        string ModAuthor { get; }
        string ModDescription { get; }
        
        /// <summary>
        /// Called when the mod is loaded
        /// </summary>
        void OnModLoad();
        
        /// <summary>
        /// Called when the mod is unloaded
        /// </summary>
        void OnModUnload();
        
        /// <summary>
        /// Called every frame (optional)
        /// </summary>
        void OnUpdate() { }
        
        /// <summary>
        /// Called when GUI is rendered (optional)
        /// </summary>
        void OnGUI() { }
    }
    
    /// <summary>
    /// Attribute to mark classes as CNR mods
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CNRModAttribute : Attribute
    {
        public string Name { get; }
        public string Version { get; }
        public string Author { get; }
        public string Description { get; }
        
        public CNRModAttribute(string name, string version, string author, string description = "")
        {
            Name = name;
            Version = version;
            Author = author;
            Description = description;
        }
    }
    
    /// <summary>
    /// Core mod loading and management system
    /// </summary>
    public class CNRModManager : MonoBehaviour
    {
        private static CNRModManager _instance;
        public static CNRModManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CNRModManager");
                    _instance = go.AddComponent<CNRModManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private List<ICNRMod> loadedMods = new List<ICNRMod>();
        private Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
        
        // Path where mods are stored on Android
        private const string MOD_DIRECTORY = "/storage/emulated/0/CNRMods";
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[CNRModLoader] Mod Manager initialized");
            LoadAllMods();
        }
        
        private void Update()
        {
            // Call Update on all loaded mods
            foreach (var mod in loadedMods)
            {
                try
                {
                    mod.OnUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CNRModLoader] Error in mod {mod.ModName} Update: {ex}");
                }
            }
        }
        
        private void OnGUI()
        {
            // Call OnGUI on all loaded mods
            foreach (var mod in loadedMods)
            {
                try
                {
                    mod.OnGUI();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CNRModLoader] Error in mod {mod.ModName} OnGUI: {ex}");
                }
            }
        }
        
        /// <summary>
        /// Load all mods from the mod directory
        /// </summary>
        public void LoadAllMods()
        {
            Debug.Log($"[CNRModLoader] Looking for mods in: {MOD_DIRECTORY}");
            
            if (!Directory.Exists(MOD_DIRECTORY))
            {
                Debug.Log("[CNRModLoader] Mod directory doesn't exist, creating it...");
                try
                {
                    Directory.CreateDirectory(MOD_DIRECTORY);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CNRModLoader] Failed to create mod directory: {ex}");
                    return;
                }
            }
            
            string[] dllFiles = Directory.GetFiles(MOD_DIRECTORY, "*.dll", SearchOption.AllDirectories);
            Debug.Log($"[CNRModLoader] Found {dllFiles.Length} DLL files");
            
            foreach (string dllPath in dllFiles)
            {
                LoadMod(dllPath);
            }
            
            Debug.Log($"[CNRModLoader] Loaded {loadedMods.Count} mods total");
        }
        
        /// <summary>
        /// Load a single mod from a DLL file
        /// </summary>
        public void LoadMod(string dllPath)
        {
            try
            {
                Debug.Log($"[CNRModLoader] Loading mod: {dllPath}");
                
                byte[] assemblyData = File.ReadAllBytes(dllPath);
                Assembly assembly = Assembly.Load(assemblyData);
                
                string assemblyName = Path.GetFileName(dllPath);
                loadedAssemblies[assemblyName] = assembly;
                
                // Find all classes that implement ICNRMod
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(ICNRMod).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        try
                        {
                            ICNRMod modInstance = (ICNRMod)Activator.CreateInstance(type);
                            loadedMods.Add(modInstance);
                            
                            Debug.Log($"[CNRModLoader] Loaded mod: {modInstance.ModName} v{modInstance.ModVersion} by {modInstance.ModAuthor}");
                            
                            // Call OnModLoad
                            modInstance.OnModLoad();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[CNRModLoader] Failed to instantiate mod {type.Name}: {ex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CNRModLoader] Failed to load mod {dllPath}: {ex}");
            }
        }
        
        /// <summary>
        /// Unload all mods
        /// </summary>
        public void UnloadAllMods()
        {
            foreach (var mod in loadedMods)
            {
                try
                {
                    mod.OnModUnload();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CNRModLoader] Error unloading mod {mod.ModName}: {ex}");
                }
            }
            
            loadedMods.Clear();
            loadedAssemblies.Clear();
            
            Debug.Log("[CNRModLoader] All mods unloaded");
        }
        
        /// <summary>
        /// Get information about all loaded mods
        /// </summary>
        public List<ICNRMod> GetLoadedMods()
        {
            return new List<ICNRMod>(loadedMods);
        }
    }
}