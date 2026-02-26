using UnityEngine;
using System;
using System.IO;

namespace CNRModLoader
{
    /// <summary>
    /// This class should be injected into the game's main scene or attached to a persistent GameObject
    /// It initializes the mod loading system early in the game's lifecycle
    /// </summary>
    public class CNRModLoaderInitializer : MonoBehaviour
    {
        private static bool initialized = false;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeModLoader()
        {
            if (initialized) return;
            initialized = true;
            
            try
            {
                Debug.Log("[CNRModLoader] Initializing mod loader system...");
                
                // Create the mod loader manager
                var modManager = CNRModManager.Instance;
                
                Debug.Log("[CNRModLoader] Mod loader system initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CNRModLoader] Failed to initialize mod loader: {ex}");
            }
        }
        
        private void Awake()
        {
            // Backup initialization method in case RuntimeInitializeOnLoadMethod doesn't work
            if (!initialized)
            {
                InitializeModLoader();
            }
        }
        
        private void Start()
        {
            // Check if we have proper permissions and directory access
            CheckModDirectoryAccess();
        }
        
        private void CheckModDirectoryAccess()
        {
            try
            {
                string modDir = "/storage/emulated/0/CNRMods";
                
                if (!Directory.Exists(modDir))
                {
                    Debug.Log($"[CNRModLoader] Creating mod directory: {modDir}");
                    Directory.CreateDirectory(modDir);
                }
                
                // Test write access
                string testFile = Path.Combine(modDir, "test_access.txt");
                File.WriteAllText(testFile, "CNR Mod Loader Access Test");
                
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                    Debug.Log("[CNRModLoader] Mod directory access confirmed");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CNRModLoader] Mod directory access failed: {ex}");
                Debug.LogError("[CNRModLoader] Make sure the app has WRITE_EXTERNAL_STORAGE permission");
            }
        }
        
        /// <summary>
        /// Manual initialization method that can be called from existing game code
        /// </summary>
        public static void ManualInitialize()
        {
            InitializeModLoader();
        }
    }
}