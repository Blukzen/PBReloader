using System;
using System.IO;
using System.Threading.Tasks;

namespace Blukzen.ScriptReloadPlugin.Utility
{
    public static class FileHelpers
    {
        public static async void ReadFileBackoffAsync(string path, int maxRetries = 3, int backoffIntervalMilliseconds = 1000, Action<string> callback = null)
        {
            var retryCount = 0;

            while (true)
            {
                try
                {
                    var readTask = Task.Run(() => File.ReadAllText(path));
                    var fileContents = await readTask;

                    callback?.Invoke(fileContents);
                    break;
                }
                catch (Exception ex)
                {
                    if (++retryCount == maxRetries)
                    {
                        ScriptReloader.Logger.Error(ex, "Exception reading file " + path);
                        break;
                    }
                }

                await Task.Delay(backoffIntervalMilliseconds * retryCount);
            }
        }
    }
}