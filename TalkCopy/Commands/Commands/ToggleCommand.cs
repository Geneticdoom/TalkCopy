using TalkCopy.Attributes;
using TalkCopy.Commands.Base;
using TalkCopy.Core.Handlers;
using TalkCopy.Windows.Windows;

namespace TalkCopy.Commands.Commands;

[Active]
internal class ToggleCommand : CommandBase
{
    public override string Command { get; } = "/talkcopy";
    public override string HelpMessage { get; } = "Toggles the plugin on or off.";
    public override bool ShowInHelp { get; } = true;

    public override void OnCommand(string command, string args)
    {
        ref bool Toggle = ref PluginHandlers.Plugin.Config.CopyAnyText;

        if (args == string.Empty || args == null) Toggle = !Toggle;
        else if (args.ToLower() == "logs") { PluginHandlers.Plugin.WindowHandler.ToggleWindow<CopyLogWindow>(); return; }
        else if (args.ToLower() == "settings") { PluginHandlers.Plugin.WindowHandler.ToggleWindow<SettingsWindow>(); return; }
        else if (args.ToLower() == "true") Toggle = true;
        else if (args.ToLower() == "false") Toggle = false;

        PluginHandlers.Plugin.Config.Save();

        if (Toggle) PluginHandlers.ChatGui.Print("Text messages will now get copied!");
        else PluginHandlers.ChatGui.Print("Text messages will no longer get copied!");
    }
}
