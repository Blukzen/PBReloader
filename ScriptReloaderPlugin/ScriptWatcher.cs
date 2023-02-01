using System.Collections.Generic;
using System.IO;
using Sandbox.Game.Entities.Blocks;

namespace Blukzen.ScriptReloadPlugin
{
    public class ScriptWatcher
    {
        
        private readonly List<MyProgrammableBlock> _programmableBlocks = new();
        private readonly FileSystemWatcher _watcher;
        private string _scriptPath;
        private string _scriptName;
        private string _fullScriptPath => _scriptPath + "\\" + _scriptName;
        

        public ScriptWatcher(string scriptPath, string scriptName)
        {
            _scriptPath = scriptPath;
            _scriptName = scriptName;
            _watcher = new FileSystemWatcher(scriptPath);
            _watcher.NotifyFilter = NotifyFilters.LastWrite;

            _watcher.Changed += OnChanged;
            _watcher.Error += OnError;
            _watcher.Filter = "*.cs";
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
        }

        public void Subscribe(MyProgrammableBlock pb)
        {
            _programmableBlocks.Add(pb);
        }

        public void Unsubscribe(MyProgrammableBlock pb)
        {
            _programmableBlocks.Remove(pb);
        }

        private void UpdateSubscribers(string script)
        {
            foreach (var pb in _programmableBlocks)
            {
                pb.ReloadScript(script);
            }
        }
        
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            FileHelpers.ReadFileBackoffAsync(_fullScriptPath, callback: UpdateSubscribers);
        }
        
        private void OnError(object sender, ErrorEventArgs errorEventArgs)
        {
            foreach (var pb in _programmableBlocks)
            {
                pb.UpdateProgram(errorEventArgs.GetException().Message);
                pb.SendRecompile();
            }
        }
    }
}