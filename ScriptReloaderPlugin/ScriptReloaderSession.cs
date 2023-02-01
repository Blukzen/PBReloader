using System;
using System.Linq;
using Blukzen.ScriptReloadPlugin.Utility;
using EmptyKeys.UserInterface;
using NLog.LayoutRenderers;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace Blukzen.ScriptReloadPlugin
{
    [MySessionComponentDescriptor(MyUpdateOrder.Simulation)]
    public class ScriptReloaderSession : MySessionComponentBase
    {
        private ScriptReloaderStorage _storage;
        public ScriptReloaderStorage Storage
        {
            get
            {
                return _storage ??= new ScriptReloaderStorage(MySession.Static.WorldId);
            }
        }
        public static ScriptReloaderSession Instance;
        public override void LoadData()
        {
            base.LoadData();
            Instance = this;
        }

        public override void SaveData()
        {
            Storage.Save();
        }
    }
}