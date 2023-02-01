using System.Collections.Generic;
using System.IO;
using Blukzen.ScriptReloadPlugin.Extensions;
using Sandbox.Game.Entities.Blocks;

namespace Blukzen.ScriptReloadPlugin.Utility
{
    public class ScriptWatcher
    {
        public static Dictionary<string, ScriptWatcher> GetWatchers { get; } = new();

        private readonly HashSet<MyProgrammableBlock> _programmableBlocks = new();
        private readonly FileSystemWatcher _watcher;
        private string _scriptPath;
        private string _scriptName;
        private string _currentScript;
        private string _fullScriptPath => _scriptPath + "\\" + _scriptName;

        public bool ScriptLoaded => _currentScript is { Length: > 1 }; 

        public static ScriptWatcher GetOrAddWatcher(string path)
        {
            if (GetWatchers.TryGetValue(path, out var watcher))
            {
                return watcher;
            }
            
            watcher = new ScriptWatcher(path, Constants.DefaultScriptName);
            GetWatchers.Add(path, watcher);

            return watcher;
        }

        public static void Unsubscribe(string path, MyProgrammableBlock pb)
        {
            if (!GetWatchers.TryGetValue(path, out var watcher))
            {
                return;
            }
            
            watcher.Unsubscribe(pb);

            if (watcher._programmableBlocks.Count == 0)
            {
                watcher.Dispose();
            }
        }

        private ScriptWatcher(string scriptPath, string scriptName)
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

        public string LoadScript()
        {
            _currentScript = File.ReadAllText(_fullScriptPath);
            return _currentScript;
        }

        public void Subscribe(MyProgrammableBlock pb, bool recompile = true)
        {
            if (_currentScript == null)
            {
                LoadScript();
            }
            
            _programmableBlocks.Add(pb);
            if (recompile)
            {
                pb.ReloadScript(_currentScript);
            }
        }

        private void Unsubscribe(MyProgrammableBlock pb)
        {
            _programmableBlocks.Remove(pb);
        }

        private void UpdateSubscribers(string script)
        {
            _currentScript = script;
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

        private void Dispose()
        {
            _watcher.Dispose();
            GetWatchers.Remove(_scriptPath);
        }
    }
}