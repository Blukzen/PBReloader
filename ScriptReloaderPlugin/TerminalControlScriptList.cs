using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using VRage.Utils;

namespace Blukzen.ScriptReloadPlugin
{
    public class TerminalControlScriptList : MyTerminalControlListbox<MyProgrammableBlock>
    {
        public TerminalControlScriptList(string id, MyStringId title, MyStringId tooltip, bool multiSelect = false, int visibleRowsCount = 8) : base(id, title, tooltip, multiSelect, visibleRowsCount)
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
            if (!pb.TryGetScriptPath(out var scriptPath)) return;
            
            try
            {
                var item = listBoxContent.First(
                    item => item.UserData.ToString() == scriptPath);
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
            string[] scriptPaths = Directory.GetDirectories(Constants.LocalScriptsFolder);

            foreach (var path in scriptPaths)
            {
                string scriptName = Path.GetFileName(path);
                var item = new MyGuiControlListbox.Item(text: new StringBuilder(scriptName), toolTip: scriptName,
                    userData: path, icon: MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_LOCAL.Normal);
                list.Add(item);
            }
        }
        
        private static void SelectScript(MyProgrammableBlock pb, List<MyGuiControlListbox.Item> items)
        {
            if (items.Count == 0)
                return;

            MyGuiControlListbox.Item item = items.First();

            if (pb.Storage == null)
            {
                pb.Storage = new MyModStorageComponent();
            }

            pb.Storage.Add(Constants.StorageKeyScript, item.UserData.ToString());
        }
    }
}