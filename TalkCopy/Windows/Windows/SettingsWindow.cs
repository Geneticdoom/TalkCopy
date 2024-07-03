using TalkCopy.Core.Handlers;
using ImGuiNET;
using TalkCopy.Windows.Attributes;
using TalkCopy.Attributes;

namespace TalkCopy.Windows.Windows;

[Active]
[SettingsWindow]
internal class SettingsWindow : TalkWindow
{
    public SettingsWindow() : base("Talk Copy Settings")
    {
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new System.Numerics.Vector2(250, 250)
        };
    }

    public override void Draw()
    {
        if (ImGui.Checkbox("Copy ANY Text", ref PluginHandlers.Plugin.Config.CopyAnyText))
            PluginHandlers.Plugin.Config.Save();

        if (!PluginHandlers.Plugin.Config.CopyAnyText) ImGui.BeginDisabled();

        if (ImGui.Checkbox("Copy Text from the Dialog Box?", ref PluginHandlers.Plugin.Config.CopyDialogBoxText))
            PluginHandlers.Plugin.Config.Save();

        if (ImGui.Checkbox("Copy Text from Subtitles?", ref PluginHandlers.Plugin.Config.CopySubtitles))
            PluginHandlers.Plugin.Config.Save();

        if (ImGui.Checkbox("Copy Text from the Battle Toast?", ref PluginHandlers.Plugin.Config.CopyBattleTalk))
            PluginHandlers.Plugin.Config.Save();

        if (ImGui.Checkbox("Copy Text from Toasts?", ref PluginHandlers.Plugin.Config.CopyToastText))
            PluginHandlers.Plugin.Config.Save();

        if (ImGui.Checkbox("Copy Text from Area Toasts?", ref PluginHandlers.Plugin.Config.CopyAreaText))
            PluginHandlers.Plugin.Config.Save();

        if (ImGui.Checkbox("Copy Text from Error Toasts?", ref PluginHandlers.Plugin.Config.CopyErrorBoxText))
            PluginHandlers.Plugin.Config.Save();

        ImGui.EndDisabled();
    }
}
