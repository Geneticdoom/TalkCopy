using TalkCopy.Core.Handlers;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
namespace TalkCopy;

internal class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool ParseOutBrackets = false;

    public bool CopyDialogBoxText = true;
    public bool CopyErrorBoxText = true;
    public bool CopyBattleTalk = true;
    public bool CopyToastText = true;
    public bool CopyAreaText = true;
    public bool CopySubtitles = true;
    public bool CopyTooltips = true;

    public VirtualKey ComboModifier = VirtualKey.CONTROL;
    public VirtualKey ComboModifier2 = VirtualKey.SHIFT;
    public VirtualKey ComboKey = VirtualKey.SPACE;
    public bool PreventPassthrough = true;
    public bool ToggleMode = true;
    public bool ShowTooltip = true;
    public bool ShowWarningOutline = true;
    public bool ShowWarningText = true;

    public bool CopyAnyText = true;
    public bool hour24 = true;

    public void Save() => PluginHandlers.PluginInterface.SavePluginConfig(this);
}
