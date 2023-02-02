using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Blukzen.ScriptReloadPlugin.Extensions;
using Blukzen.ScriptReloadPlugin.TerminalControls;
using Blukzen.ScriptReloadPlugin.Utility;
using HarmonyLib;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Terminal.Controls;
using Sandbox.Graphics.GUI;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace Blukzen.ScriptReloadPlugin.Patches
{
    [HarmonyPatch(typeof(MyProgrammableBlock))]
    public class ProgrammableBlockPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MyProgrammableBlock), "Init")]
        public static void InitPostfix(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid,
            MyProgrammableBlock __instance, ref string ___m_editorData, ref string ___m_programData)
        {
            var data = __instance.GetData();
            if (data.AutoReloadScript && data.HasScript)
            {
                var recompile = __instance.GetType()
                    .GetMethod("Recompile", BindingFlags.NonPublic | BindingFlags.Instance);
                var watcher = ScriptWatcher.GetOrAddWatcher(data.SavedScriptPath);
                var script = watcher.LoadScript();

                watcher.Subscribe(__instance, false);
                ___m_editorData = ___m_programData = script;
                recompile?.Invoke(__instance, new object[] { false });
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("CreateTerminalControls")]
        public static void CreateTerminalControlsPrefix(out CreateTerminalControlsState __state)
        {
            __state = new CreateTerminalControlsState();

            if (MyTerminalControlFactory.AreControlsCreated<MyProgrammableBlock>())
            {
                __state.ControlsCreated = true;
            }
            else
            {
                __state.ControlsCreated = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("CreateTerminalControls")]
        public static void CreateTerminalControlsPostfix(CreateTerminalControlsState __state)
        {
            if (__state.ControlsCreated)
            {
                return;
            }

            var enabled = new Func<MyProgrammableBlock, bool>(pb => pb.GetData().AutoReloadScript && pb.Enabled);
            
            MyTerminalControlSeparator<MyProgrammableBlock> separator =
                new MyTerminalControlSeparator<MyProgrammableBlock>();
            
            MyTerminalControlOnOffSwitch<MyProgrammableBlock> toggle =
                new MyTerminalControlOnOffSwitch<MyProgrammableBlock>(
                    "PBAutoReloadToggle",
                    MyStringId.GetOrCompute("Auto Reload Scripts"),
                    MyStringId.GetOrCompute("Auto reloads script when file changed is detected"));
            toggle.Getter = GetAutoReloadEnabled;
            toggle.Setter = SetAutoReloadEnabled;
            toggle.Enabled = pb => pb.Enabled;

            TerminalControlScriptList scriptList = new TerminalControlScriptList(
                "PBScriptList",
                MyStringId.GetOrCompute("Local Scripts"),
                MyStringId.GetOrCompute("Select a local script to load"));
            scriptList.Enabled = enabled;

            MyTerminalControlButton<MyProgrammableBlock> openFolder = new MyTerminalControlButton<MyProgrammableBlock>(
                "PBAutoReloadOpenFolder",
                MyStringId.GetOrCompute("Open Script Folder"),
                MyStringId.GetOrCompute("Opens the folder containing the selected script"),
                OpenScriptFolder
            );
            openFolder.Enabled = enabled;


            MyTerminalControlFactory.AddControl(separator);
            MyTerminalControlFactory.AddControl(toggle);
            MyTerminalControlFactory.AddControl(scriptList);
            MyTerminalControlFactory.AddControl(openFolder);
        }

        private static void OpenScriptFolder(MyProgrammableBlock pb)
        {
            var data = pb.GetData();
            if (data.HasScript)
            {
                try
                {
                    Process.Start($@"{data.SavedScriptPath}");
                }
                catch (Exception e)
                {
                    ScriptReloader.Logger.Error(e, $"Failed to open script folder {data.SavedScriptPath}");
                }
            }
        }

        private static bool GetAutoReloadEnabled(MyProgrammableBlock pb)
        {
            return pb.GetData().AutoReloadScript;
        }

        private static void SetAutoReloadEnabled(MyProgrammableBlock pb, bool value)
        {
            var data = pb.GetData();
            data.AutoReloadScript = value;
            if (value && data.HasScript)
            {
                var watcher = ScriptWatcher.GetOrAddWatcher(data.SavedScriptPath);
                watcher.Subscribe(pb);
            }
            else if (data.HasScript)
            {
                ScriptWatcher.Unsubscribe(data.SavedScriptPath, pb);
            }
        }

        public struct CreateTerminalControlsState
        {
            public bool ControlsCreated;
        }
    }
}