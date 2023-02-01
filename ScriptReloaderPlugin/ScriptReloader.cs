using System;
using System.IO;
using System.Reflection;
using Blukzen.ScriptReloadPlugin.GUI;
using Blukzen.ScriptReloadPlugin.Utility;
using Blukzen.Shared.Config;
using Blukzen.Shared.Logging;
using Blukzen.Shared.Patches;
using Blukzen.Shared.Plugin;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using SpaceEngineers.Game;
using VRage.FileSystem;
using VRage.Plugins;
using VRage.Scripting;

namespace Blukzen.ScriptReloadPlugin
{
    static class Constants
    {
        public static Guid StorageKeyScript = new Guid("d6685778-f72a-4fdc-be7a-4485687563ee");
        public static readonly string LocalScriptsFolder = Path.Combine(MyFileSystem.UserDataPath, DefaultScriptsDirectory, "local");
        public const string DefaultScriptsDirectory = "IngameScripts";
        public const string DefaultScriptName = "Script.cs";
    }
    
    public class ScriptReloader : IPlugin, ICommonPlugin
    {
        public const string Name = "ScriptReloader";
        public static readonly Guid _id = new("d6685778-f72a-4fdc-be7a-4485687563ee");
        public IPluginLogger Log => Logger;
        public static readonly IPluginLogger Logger = new PluginLogger(Name);
        public static ScriptReloader Instance { get; private set; }


        public long Tick { get; private set; }
        
        public IPluginConfig Config => config?.Data;
        private PersistentConfig<PluginConfig> config;
        private static readonly string ConfigFileName = $"{Name}.cfg";
        private SpaceEngineersGame game;
        
        private static bool initialized;
        private static bool failed;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Log.Info("Loading");
            Instance = this;

            var configPath = Path.Combine(MyFileSystem.UserDataPath, ConfigFileName);
            config = PersistentConfig<PluginConfig>.Load(Log, configPath);

            Common.SetPlugin(this);

            if (!PatchHelpers.HarmonyPatchAll(Log, new Harmony(Name)))
            {
                failed = true;
                return;
            }

            Log.Debug("Successfully loaded");
        }

        public void Dispose()
        {
            try
            {
                ScriptWatcher.DisposeWatchers();
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Dispose failed");
            }

            Instance = null;
        }

        public void Update()
        {
            EnsureInitialized();
            try
            {
                if (!failed)
                {
                    CustomUpdate();
                    Tick++;
                }
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Update failed");
                failed = true;
            }
        }

        private void EnsureInitialized()
        {
            if (initialized || failed)
                return;

            Log.Info("Initializing");
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Failed to initialize plugin");
                failed = true;
                return;
            }

            Log.Debug("Successfully initialized");
            initialized = true;
        }

        private void Initialize()
        {
            if (!Directory.Exists(Constants.LocalScriptsFolder))
            {
                Directory.CreateDirectory(Constants.LocalScriptsFolder);
            }
        }

        private void CustomUpdate()
        {
        }


        // ReSharper disable once UnusedMember.Global
        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyPluginConfigDialog());
        }
    }
}