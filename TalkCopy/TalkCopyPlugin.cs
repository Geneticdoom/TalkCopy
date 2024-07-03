using Dalamud.Plugin;
using TalkCopy.Automation;
using TalkCopy.Core.Handlers;
using TalkCopy.Windows;

namespace TalkCopy;

public sealed class TalkCopyPlugin : IDalamudPlugin
{
    internal Configuration Config { get; private set; }
    internal WindowHandler WindowHandler { get; private set; }

    public TalkCopyPlugin(IDalamudPluginInterface pluginInterface)
    {
        PluginHandlers.Start(ref pluginInterface, this);
        Config = PluginHandlers.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        WindowHandler = new WindowHandler(pluginInterface);
        new Creator().Create();
    }

    public void Dispose()
    {
        WindowHandler?.Dispose();
    }
}
