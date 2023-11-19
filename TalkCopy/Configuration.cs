using TalkCopy.Core.Handlers;
using Dalamud.Configuration;

namespace TalkCopy;

internal class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool copyToClipboard = true;

    public void Save() => PluginHandlers.PluginInterface.SavePluginConfig(this);
}
