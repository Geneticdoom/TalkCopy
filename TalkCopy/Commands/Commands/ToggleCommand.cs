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

        string argsLower = args?.ToLower() ?? string.Empty;

        if (argsLower == string.Empty) Toggle = !Toggle;
        else if (argsLower == "logs") { PluginHandlers.Plugin.WindowHandler.ToggleWindow<CopyLogWindow>(); return; }
        else if (argsLower == "settings") { PluginHandlers.Plugin.WindowHandler.ToggleWindow<SettingsWindow>(); return; }
        else if (argsLower == "true") Toggle = true;
        else if (argsLower == "false") Toggle = false;
        else if (argsLower == "help")
        {
            PluginHandlers.ChatGui.Print($"DialogCopy Help Section");
            PluginHandlers.ChatGui.Print(string.Empty);
            PluginHandlers.ChatGui.Print($"You can use the following arguments:");
            PluginHandlers.ChatGui.Print($"    [logs]       (this opens the logs window)");
            PluginHandlers.ChatGui.Print($"    [settings]   (this opens the settings window)");
            PluginHandlers.ChatGui.Print($"    [false]      (this disables automatic text copy)");
            PluginHandlers.ChatGui.Print($"    [true]       (this enables automatic text copy)");
            return;
        }
        else
        {
            PluginHandlers.ChatGui.Print($"The argument: '{args}' is not recognised by Dialog Copy.");
            return;
        }

        PluginHandlers.Plugin.Config.Save();

        if (Toggle) PluginHandlers.ChatGui.Print("Text messages will now get copied!");
        else PluginHandlers.ChatGui.Print("Text messages will no longer get copied!");
    }
}
