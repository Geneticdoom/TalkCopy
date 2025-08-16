using System;
using System.Collections.Generic;
using TalkCopy.Core.Handlers;
using Dalamud.Bindings.ImGui;
using System.Linq;
using System.Threading.Tasks;

namespace TalkCopy;

internal static class CopyHandler
{
    internal static List<CopyData> CopyData = new();

    public static void CopyTextToClipboard(string Addon, string Text, bool Block)
    {
        if (!PluginHandlers.Plugin.Config.CopyAnyText) return;
        PluginHandlers.PluginLog.Verbose("Addon: " + Addon + " wants to copy the text: " + Text);
        CopyData.Add(new CopyData(DateTime.Now, Addon, Text, Block));
        if (CopyData.Count > 100) CopyData.RemoveAt(0);

        if (!Block)
        {
            if (PluginHandlers.Plugin.Config.UseWebSocket)
            {
                // Send via WebSocket with failsafe check
                _ = Task.Run(async () => await SendTextViaWebSocketWithFailsafe(Text));
            }
            else
            {
                // Use clipboard (original behavior)
                ImGui.SetClipboardText(Text);
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

internal struct CopyData(DateTime timeStamp, string addon, string text, bool blocked)
{
    public readonly DateTime MessageTimestamp = timeStamp;
    public readonly string Addon = addon;
    public readonly string Text = text;
    public readonly bool Blocked = blocked;
}
