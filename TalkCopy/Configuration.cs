using TalkCopy.Core.Handlers;
using Dalamud.Configuration;

namespace TalkCopy;

internal class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool CopyDialogBoxText = true;
    public bool CopyErrorBoxText = true;
    public bool CopyBattleTalk = true;
    public bool CopyToastText = true;
    public bool CopyAreaText = true;
    public bool CopySubtitles = true;
    public bool CopyTooltips = true;

    public bool CopyAnyText = true;
    public bool hour24 = true;

    public void Save() => PluginHandlers.PluginInterface.SavePluginConfig(this);
}
