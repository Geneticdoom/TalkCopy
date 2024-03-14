using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using TalkCopy.Core.Handlers;
using TalkCopy.Core.Hooking;
using ImGuiNET;
using Dalamud.Interface.Windowing;

namespace TalkCopy;

public sealed class TalkCopyPlugin : IDalamudPlugin
{
    internal Configuration config;
    WindowSystem windowSystem;

    public TalkCopyPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        PluginHandlers.Start(ref pluginInterface, this);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, "Talk", OnTalk);
        windowSystem = new WindowSystem("Dialog Copy");
        config = PluginHandlers.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        pluginInterface.UiBuilder.Draw += windowSystem.Draw;
        SettingsWindow window = new SettingsWindow();
        windowSystem.AddWindow(window);
        pluginInterface.UiBuilder.OpenConfigUi += () => window.IsOpen = true;
        PluginHandlers.CommandManager.AddHandler("/talkcopy", new Dalamud.Game.Command.CommandInfo((command, arguments) =>
        {
            if (arguments == string.Empty || arguments == null) PluginHandlers.Plugin.config.copyToClipboard ^= true;
            else
            {
                if (arguments.ToLower() == "true") PluginHandlers.Plugin.config.copyToClipboard = true;
                else if (arguments.ToLower() == "false") PluginHandlers.Plugin.config.copyToClipboard = false;
            }
            PluginHandlers.Plugin.config.Save();
            if (PluginHandlers.Plugin.config.copyToClipboard) PluginHandlers.ChatGui.Print("Text messages will now get copied!");
            else PluginHandlers.ChatGui.Print("Text messages will no longer get copied!");
        })
        {
            HelpMessage = "Toggles the plugin on or off.",
            ShowInHelp = true
        });
    }

    string lastText = string.Empty;
    unsafe void OnTalk(AddonEvent type, AddonArgs args)
    {
        try
        {
            BaseNode bNode = new BaseNode((AtkUnitBase*)args.Addon);
            if (bNode == null) return;
            AtkTextNode* textNode = bNode.GetNode<AtkTextNode>(3);
            if (textNode == null) return;
            string text = textNode->NodeText.ToString();
            if (lastText != text)
            {
                lastText = text;
                if (text != string.Empty && text != null && PluginHandlers.Plugin.config.copyToClipboard) ImGui.SetClipboardText(text);
            }
        }
        catch { }
    }

    public void Dispose()
    {
        PluginHandlers.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        windowSystem.RemoveAllWindows();
    }
}
