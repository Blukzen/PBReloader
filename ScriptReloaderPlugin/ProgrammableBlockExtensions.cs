using Sandbox.Game.Entities.Blocks;

namespace Blukzen.ScriptReloadPlugin
{
    public static class ProgrammableBlockExtensions
    {
        public static void ReloadScript(this MyProgrammableBlock pb, string script)
        {
            pb.UpdateProgram(script);
            pb.SendRecompile();
        }
        
        public static bool TryGetScriptPath(this MyProgrammableBlock pb, out string scriptPath)
        {
            pb.Storage.TryGetValue(Constants.StorageKeyScript, out string path);
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