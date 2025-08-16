using TalkCopy.Core.Handlers;
using Dalamud.Bindings.ImGui;
using TalkCopy.Windows.Attributes;
using TalkCopy.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TalkCopy.Windows.Windows;

[Active]
internal class CopyLogWindow : TalkWindow
{
    public CopyLogWindow() : base("Talk Copy Log")
    {
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new System.Numerics.Vector2(250, 250)
        };
    }

    public override void Draw()
    {
        if (ImGui.Button("Clear Log"))
        {
            CopyHandler.CopyData.Clear();
        }

        ImGui.SameLine();

        if (ImGui.Button("Copy All"))
        {
            var allText = string.Join("\n", CopyHandler.CopyData.Select(x => x.Text));
            if (PluginHandlers.Plugin.Config.UseWebSocket)
            {
                // Send via WebSocket with failsafe
                _ = Task.Run(async () => await SendTextViaWebSocketWithFailsafe(allText));
            }
            else
            {
                ImGui.SetClipboardText(allText);
            }
        }

        ImGui.Separator();

        for (int i = CopyHandler.CopyData.Count - 1; i >= 0; i--)
        {
            var data = CopyHandler.CopyData[i];
            var timeString = data.MessageTimestamp.ToString(PluginHandlers.Plugin.Config.hour24 ? "HH:mm:ss" : "hh:mm:ss tt");
            
            ImGui.Text($"[{timeString}] {data.Addon}: {data.Text}");
            
            ImGui.SameLine();
            
            if (ImGui.Button($"Copy Again##{i}"))
            {
                if (PluginHandlers.Plugin.Config.UseWebSocket)
                {
                    // Send via WebSocket with failsafe
                    _ = Task.Run(async () => await SendTextViaWebSocketWithFailsafe(data.Text));
                }
                else
                {
                    ImGui.SetClipboardText(data.Text);
                }
            }
        }
    }

    private static async Task SendTextViaWebSocketWithFailsafe(string text)
    {
        try
        {
            var webSocketServer = PluginHandlers.Plugin.WebSocketServer;
            if (webSocketServer == null)
            {
                PluginHandlers.PluginLog.Warning("WebSocket server is null, attempting to initialize...");
                await PluginHandlers.Plugin.InitializeWebSocketServer();
                webSocketServer = PluginHandlers.Plugin.WebSocketServer;
            }

            if (webSocketServer != null)
            {
                // Ensure the server is actually running
                if (!webSocketServer.IsRunning)
                {
                    PluginHandlers.PluginLog.Warning("WebSocket server is not running, attempting to restart...");
                    var success = await webSocketServer.EnsureRunningAsync();
                    if (!success)
                    {
                        PluginHandlers.PluginLog.Error("Failed to restart WebSocket server, falling back to clipboard");
                        ImGui.SetClipboardText(text);
                        return;
                    }
                }

                await webSocketServer.SendTextAsync(text);
            }
            else
            {
                PluginHandlers.PluginLog.Error("Failed to initialize WebSocket server, falling back to clipboard");
                ImGui.SetClipboardText(text);
            }
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error($"Error sending text via WebSocket: {ex.Message}, falling back to clipboard");
            ImGui.SetClipboardText(text);
        }
    }
}
