using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Blukzen.ScriptReloadPlugin.Extensions;
using Blukzen.ScriptReloadPlugin.Utility;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using VRage.Game;
using VRage.Utils;

namespace Blukzen.ScriptReloadPlugin.TerminalControls
{
    public class TerminalControlScriptList : MyTerminalControlListbox<MyProgrammableBlock>
    {
        public TerminalControlScriptList(string id, MyStringId title, MyStringId tooltip, bool multiSelect = false,
            int visibleRowsCount = 8) : base(id, title, tooltip, multiSelect, visibleRowsCount)
        {
            ListContent = GetLocalScripts;
            ItemSelected = SelectScript;
        }

        private static void GetLocalScripts(MyProgrammableBlock pb,
            ICollection<MyGuiControlListbox.Item> listBoxContent,
            ICollection<MyGuiControlListbox.Item> listBoxSelectedItems,
            ICollection<MyGuiControlListbox.Item> lastFocused)
        {
            GetLocalScriptNames(ref listBoxContent);
            var data = pb.GetData();

            try
            {
                var item = listBoxContent.First(item => item.UserData.ToString() == data.SavedScriptPath);
                listBoxSelectedItems.Add(item);
            }
            catch (InvalidOperationException ex)
            {
                ScriptReloader.Logger.Error(ex, "Failed to load selected script");
            }
        }

        private static void GetLocalScriptNames(ref ICollection<MyGuiControlListbox.Item> list)
        {
            if (!Directory.Exists(Constants.LocalScriptsFolder))
                return;

            var scriptPaths = Directory.GetDirectories(Constants.LocalScriptsFolder)
                .Concat(Directory.GetDirectories(Constants.CloudScriptsFolder));
            
            foreach (var path in scriptPaths)
            {
                var isCloud = path.Contains("temp");
                var scriptName = Path.GetFileName(path);
                var item = new MyGuiControlListbox.Item(text: new StringBuilder(scriptName), toolTip: scriptName,
                    userData: path, icon: isCloud ? MyGuiConstants.TEXTURE_ICON_MODS_CLOUD.Normal : MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_LOCAL.Normal);
                list.Add(item);
            }
        }

        private static void SelectScript(MyProgrammableBlock pb, List<MyGuiControlListbox.Item> items)
        {
            if (items.Count == 0)
                return;

            var item = items.First();
            var data = pb.GetData();

            if (data.AutoReloadScript && data.HasScript)
            {
                ScriptWatcher.Unsubscribe(data.SavedScriptPath, pb);
            }

            data.SavedScriptPath = item.UserData.ToString();
            ScriptWatcher.GetOrAddWatcher(data.SavedScriptPath).Subscribe(pb);
            ScriptReloader.Logger.Debug(data.SavedScriptPath);
        }
    }
}