using Sandbox.Game.Entities.Blocks;

namespace Blukzen.ScriptReloadPlugin.Extensions
{
    public static class ProgrammableBlockExtensions
    {
        public static void ReloadScript(this MyProgrammableBlock pb, string script, bool recompile = true)
        {
            pb.UpdateProgram(script);

            if (recompile)
            {
                pb.SendRecompile();
            }
        }

        public static ScriptReloaderStorage.PBData GetData(this MyProgrammableBlock pb)
        {
            if (!ScriptReloaderSession.Instance.Storage.TryGetData(pb.EntityId, out var data))
            {
                data = ScriptReloaderSession.Instance.Storage.Add(pb);
            };

            return data;
        }

        public static bool TryGetScriptPath(this MyProgrammableBlock pb, out string scriptPath)
        {
            pb.Storage.TryGetValue(Constants.StorageKeyScript, out var path);
            if (pb.Storage == null || path == null || path.Length == 0)
            {
                scriptPath = null;
                return false;
            }

            scriptPath = path;
            return true;
        }
    }
}