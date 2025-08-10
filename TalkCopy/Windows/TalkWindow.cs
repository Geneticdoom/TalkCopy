using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using TalkCopy.Core.Handlers;
using TalkCopy.Windows.Attributes;
using TalkCopy.Windows.Interfaces;

namespace TalkCopy.Windows;

internal abstract class TalkWindow : Window, ITalkWindow
{
    public string Name { get; private set; } = "";

    public TalkWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse, bool forceMainWindow = false) : base(name, flags, forceMainWindow) 
    {
        Name = name;
        PluginHandlers.Plugin.WindowHandler.AddWindow(this);
        HandleAttributing();
    }

    void HandleAttributing()
    {
        object[] attributes = GetType().GetCustomAttributes(true);
        foreach(object attribute in attributes)
        {
            if (attribute is SettingsWindowAttribute) PluginHandlers.PluginInterface.UiBuilder.OpenConfigUi += () => IsOpen = true;
            if (attribute is MainWindowAttribute) PluginHandlers.PluginInterface.UiBuilder.OpenMainUi += () => IsOpen = true;
        }
    }
}
