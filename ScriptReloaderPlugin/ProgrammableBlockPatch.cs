using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using VRage.FileSystem;
using VRage.Utils;

namespace Blukzen.ScriptReloadPlugin
{
    [HarmonyPatch(typeof(MyProgrammableBlock))]
    public class ProgrammableBlockPatch
    {
        private static readonly Dictionary<string, ScriptWatcher> Watchers = new();

        [HarmonyPatch(MethodType.Constructor)]
        public static void MyProgrammableBlock()
        {
            if (!Directory.Exists(Constants.LocalScriptsFolder))
            {
                Directory.CreateDirectory(Constants.LocalScriptsFolder);
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
            
            MyTerminalControlSeparator<MyProgrammableBlock> separator =
                new MyTerminalControlSeparator<MyProgrammableBlock>();
                
            MyTerminalControlOnOffSwitch<MyProgrammableBlock> toggle =
                new MyTerminalControlOnOffSwitch<MyProgrammableBlock>(
                    "PBAutoReloadToggle",
                    MyStringId.GetOrCompute("Auto Reload Scripts"),
                    MyStringId.GetOrCompute("Auto reloads script when file changed is detected"));
            toggle.Getter = GetAutoReloadEnabled;
            toggle.Setter = SetAutoReloadEnabled;

            TerminalControlScriptList scriptList = new TerminalControlScriptList(
                "PBScriptList",
                MyStringId.GetOrCompute("Local Scripts"),
                MyStringId.GetOrCompute("Select a local script to load"));
                
            MyTerminalControlFactory.AddControl(separator);
            MyTerminalControlFactory.AddControl(toggle);
            MyTerminalControlFactory.AddControl(scriptList);
        }

        private static bool GetAutoReloadEnabled(MyProgrammableBlock block)
        {
            return false;
        }

        private static void SetAutoReloadEnabled(MyProgrammableBlock pb, bool value)
        {
            if (value && pb.TryGetScriptPath(out var scriptPath))
            {
                if (Watchers == null)
                {
                    return;
                }

                if (Watchers.ContainsKey(scriptPath))
                {
                    ScriptWatcher watcher;
                    Watchers.TryGetValue(scriptPath, out watcher);
                    watcher?.Subscribe(pb);
                }
                else
                {
                    ScriptWatcher watcher = new ScriptWatcher(scriptPath, Constants.DefaultScriptName);
                    watcher.Subscribe(pb);
                    Watchers.Add(scriptPath, watcher);
                }
            }
        }

        public struct CreateTerminalControlsState
        {
            public bool ControlsCreated;
        }
    }
}