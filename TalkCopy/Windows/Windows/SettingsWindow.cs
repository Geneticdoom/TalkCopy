using TalkCopy.Core.Handlers;
using Dalamud.Bindings.ImGui;
using TalkCopy.Windows.Attributes;
using TalkCopy.Attributes;
using Dalamud.Game.ClientState.Keys;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        if (ImGui.CollapsingHeader("Global Settings"))
        {
            if (ImGui.Checkbox("Remove Text Between Angled Brackets? ( <Example Text> )", ref PluginHandlers.Plugin.Config.ParseOutBrackets))
                PluginHandlers.Plugin.Config.Save();

            if (ImGui.Checkbox("Copy ANY Text", ref PluginHandlers.Plugin.Config.CopyAnyText))
                PluginHandlers.Plugin.Config.Save();
        }

        if (ImGui.CollapsingHeader("Output Settings"))
        {
            bool useWebSocket = PluginHandlers.Plugin.Config.UseWebSocket;
            if (ImGui.Checkbox("Use WebSocket Server (instead of clipboard)", ref useWebSocket))
            {
                PluginHandlers.Plugin.Config.UseWebSocket = useWebSocket;
                PluginHandlers.Plugin.Config.Save();
                
                // Handle WebSocket server lifecycle
                if (useWebSocket)
                {
                    if (PluginHandlers.Plugin.WebSocketServer == null)
                    {
                        _ = Task.Run(async () => await PluginHandlers.Plugin.InitializeWebSocketServer());
                    }
                }
                else
                {
                    PluginHandlers.Plugin.WebSocketServer?.Dispose();
                    PluginHandlers.Plugin.SetWebSocketServer(null);
                }
            }

            if (useWebSocket)
            {
                ImGui.Indent();
                
                int port = PluginHandlers.Plugin.Config.WebSocketPort;
                if (ImGui.InputInt("WebSocket Port", ref port))
                {
                    if (port > 0 && port <= 65535)
                    {
                        PluginHandlers.Plugin.Config.WebSocketPort = port;
                        PluginHandlers.Plugin.Config.Save();
                        
                        // Restart WebSocket server with new port
                        PluginHandlers.Plugin.WebSocketServer?.Dispose();
                        _ = Task.Run(async () => await PluginHandlers.Plugin.InitializeWebSocketServer());
                    }
                }
                
                if (port <= 0 || port > 65535)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Port must be between 1 and 65535");
                }
                
                // Show WebSocket server status
                bool isRunning = PluginHandlers.Plugin.WebSocketServer?.IsRunning ?? false;
                string statusText = isRunning ? "Running" : "Stopped";
                var statusColor = isRunning ? new System.Numerics.Vector4(0, 1, 0, 1) : new System.Numerics.Vector4(1, 0, 0, 1);
                ImGui.TextColored(statusColor, $"WebSocket Server Status: {statusText}");
                
                if (isRunning)
                {
                    ImGui.Text($"Server URL: ws://localhost:{port}");
                    ImGui.Text("Connect to this URL with a WebSocket client to receive text data");
                }
                
                ImGui.Unindent();
            }
        }

        if (ImGui.CollapsingHeader("Normal Mode Settings"))
        {
            ImGui.BeginDisabled(!PluginHandlers.Plugin.Config.CopyAnyText);

            if (ImGui.Checkbox("Try Copy Lists?", ref PluginHandlers.Plugin.Config.TryCopyLists))
                PluginHandlers.Plugin.Config.Save();

            if (ImGui.Checkbox("Copy Text from Tooltips?", ref PluginHandlers.Plugin.Config.CopyTooltips))
                PluginHandlers.Plugin.Config.Save();

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

        if (ImGui.CollapsingHeader("Text Copy Mode Settings"))
        {
            if (ImGui.Checkbox("Show Preview Tooltip in Text Copy Mode?", ref PluginHandlers.Plugin.Config.ShowTooltip))
                PluginHandlers.Plugin.Config.Save();

            if (ImGui.Checkbox("Key Selection Toggles Text Copy Mode?", ref PluginHandlers.Plugin.Config.ToggleMode))
                PluginHandlers.Plugin.Config.Save();

            if (ImGui.Checkbox("Prevent Key Passthrough to the Game?", ref PluginHandlers.Plugin.Config.PreventPassthrough))
                PluginHandlers.Plugin.Config.Save();

            ImGui.NewLine();
            ImGui.Text("Don't want anything to do with the Text Copy Mode?\nNo Problem! Set every keybind to 'No Key'");

            VirtualKeySelect("Combo Modifier 1", ref PluginHandlers.Plugin.Config.ComboModifier);
            VirtualKeySelect("Combo Modifier 2", ref PluginHandlers.Plugin.Config.ComboModifier2);
            VirtualKeySelect("Combo Key", ref PluginHandlers.Plugin.Config.ComboKey);

            ImGui.NewLine();

            if (ImGui.Checkbox("Show Warning Outline?", ref PluginHandlers.Plugin.Config.ShowWarningOutline))
                PluginHandlers.Plugin.Config.Save();

            if (ImGui.Checkbox("Show Warning Text Box?", ref PluginHandlers.Plugin.Config.ShowWarningText))
                PluginHandlers.Plugin.Config.Save();
        }
    }


    void VirtualKeySelect(string text, ref VirtualKey chosen)
    {
        if (ImGui.BeginCombo(text, chosen.GetFancyName()))
        {
            foreach (var key in Enum.GetValues<VirtualKey>().Where(x => x != VirtualKey.LBUTTON))
            {
                if (ImGui.Selectable(key.GetFancyName(), key == chosen))
                {
                    chosen = key;
                    PluginHandlers.Plugin.Config.Save();
                }
            }

            ImGui.EndCombo();
        }
    }

}
