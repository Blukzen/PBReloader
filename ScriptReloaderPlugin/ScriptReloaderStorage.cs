using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using VRage.FileSystem;

namespace Blukzen.ScriptReloadPlugin
{
    public class ScriptReloaderStorage
    {
        private static string StorageDir => $"{MyFileSystem.ModsPath}\\{ScriptReloader.Name}";
        private Guid _worldId;
        private string _storageFile;
        public Dictionary<string, PBData> Data;
        
        public ScriptReloaderStorage(Guid worldId)
        {
            ScriptReloader.Logger.Info($"Storage Path {StorageDir}");

            if (!Directory.Exists(StorageDir))
            {
                Directory.CreateDirectory(StorageDir);
            }

            _storageFile = $"{StorageDir}\\{_worldId.ToString()}.json";

            if (File.Exists(_storageFile))
            {
                var json = File.ReadAllText(_storageFile);
                Data = JsonMapper.ToObject<Dictionary<string, PBData>>(json);
            }
            else
            {
                Data = new Dictionary<string, PBData>();
            }
        }

        public bool TryGetData(long entityId, out PBData data)
        {
            return Data.TryGetValue(entityId.ToString(), out data);
        }

        public PBData Add(MyProgrammableBlock pb)
        {
            var data = new PBData()
            {
                EntityId = pb.EntityId.ToString(),
                AutoReloadScript = false,
                SavedScriptPath = null
            };
            
            Data.Add(data.EntityId, data);
            return data;
        }

        public void Save()
        {
            var json = JsonMapper.ToJson(Data);
            File.WriteAllText(_storageFile, json);
        }

        public class PBData
        {
            public string EntityId;
            public bool AutoReloadScript;
            public string SavedScriptPath;

            public bool HasScript => SavedScriptPath is { Length: > 0 };
        }
    }
}