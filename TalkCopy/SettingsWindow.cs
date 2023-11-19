using TalkCopy.Core.Handlers;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace TalkCopy;

internal class SettingsWindow : Window
{
    public SettingsWindow() : base("Talk Copy Settings", ImGuiWindowFlags.NoCollapse, false) { }

    public override void Draw()
    {
        if (ImGui.Checkbox("Enable Copy", ref PluginHandlers.Plugin.config.copyToClipboard))
            PluginHandlers.Plugin.config.Save();
    }
}
