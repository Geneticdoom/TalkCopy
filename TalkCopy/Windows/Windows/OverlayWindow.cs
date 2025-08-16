using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Logging;
using TalkCopy.Core.Handlers;
using TalkCopy.Windows.Attributes;
using TalkCopy.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TalkCopy.Windows.Windows;

[Active]
internal class OverlayWindow : TalkWindow
{
    public OverlayWindow() : base("Talk Copy Overlay")
    {
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new System.Numerics.Vector2(250, 250)
        };
    }

    public override void Draw()
    {
        ImGui.Text("Talk Copy Overlay");
        ImGui.Text("This window shows the current status of the plugin.");
        
        // Show current mode
        var currentMode = TalkCopyPlugin.CurrentMode;
        ImGui.Text($"Current Mode: {currentMode}");
        
        // Show WebSocket status if enabled
        if (PluginHandlers.Plugin.Config.UseWebSocket)
        {
            var isRunning = PluginHandlers.Plugin.WebSocketServer?.IsRunning ?? false;
            var statusText = isRunning ? "Running" : "Stopped";
            var statusColor = isRunning ? new System.Numerics.Vector4(0, 1, 0, 1) : new System.Numerics.Vector4(1, 0, 0, 1);
            ImGui.TextColored(statusColor, $"WebSocket Server: {statusText}");
        }
    }
}
