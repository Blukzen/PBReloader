using Blukzen.Shared.Config;
using Blukzen.Shared.Logging;

namespace Blukzen.Shared.Plugin
{
    public interface ICommonPlugin
    {
        IPluginLogger Log { get; }
        IPluginConfig Config { get; }
        long Tick { get; }
    }
}