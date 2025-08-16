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
    private string _statusMessage = "";
    private bool _statusMessageIsError = false;
    private DateTime _statusMessageTime = DateTime.MinValue;

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
                        _ = InitializeWebSocketServerWithErrorHandling();
                    }
                }
                else
                {
                    PluginHandlers.Plugin.WebSocketServer?.Dispose();
                    PluginHandlers.Plugin.SetWebSocketServer(null);
                    ShowStatusMessage("WebSocket server disabled", false);
                }
            }

            if (useWebSocket)
            {
                ImGui.Indent();
                
                // Address input
                string address = PluginHandlers.Plugin.Config.WebSocketAddress;
                if (ImGui.InputText("WebSocket Address", ref address, 256))
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        PluginHandlers.Plugin.Config.WebSocketAddress = address;
                        PluginHandlers.Plugin.Config.Save();
                        
                        // Restart WebSocket server with new address
                        _ = RestartWebSocketServerWithErrorHandling();
                    }
                }
                
                if (string.IsNullOrWhiteSpace(address))
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Address cannot be empty");
                }
                
                // Port input
                int port = PluginHandlers.Plugin.Config.WebSocketPort;
                if (ImGui.InputInt("WebSocket Port", ref port))
                {
                    if (port > 0 && port <= 65535)
                    {
                        PluginHandlers.Plugin.Config.WebSocketPort = port;
                        PluginHandlers.Plugin.Config.Save();
                        
                        // Restart WebSocket server with new port
                        _ = RestartWebSocketServerWithErrorHandling();
                    }
                }
                
                if (port <= 0 || port > 65535)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Port must be between 1 and 65535");
                }
                
                // Show port safety information
                if (port < 1024)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0.5f, 0, 1), "⚠️ Ports below 1024 are privileged and may require elevated permissions");
                }
                else if (port < 49152)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), "⚠️ Ports 1024-49151 are registered ports and may be reserved by other applications");
                }
                else
                {
                    ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), "✅ Port is in the safe dynamic/ephemeral range (49152-65535)");
                }
                
                ImGui.Separator();
                
                // Manual control buttons
                bool isRunning = PluginHandlers.Plugin.WebSocketServer?.IsRunning ?? false;
                string statusText = isRunning ? "Running" : "Stopped";
                var statusColor = isRunning ? new System.Numerics.Vector4(0, 1, 0, 1) : new System.Numerics.Vector4(1, 0, 0, 1);
                ImGui.TextColored(statusColor, $"WebSocket Server Status: {statusText}");
                
                if (isRunning)
                {
                    // Show the actual port being used (may be different from configured port)
                    var actualPort = PluginHandlers.Plugin.WebSocketServer?.GetCurrentPort() ?? port;
                    var actualAddress = PluginHandlers.Plugin.WebSocketServer?.GetCurrentAddress() ?? address;
                    var serverUrl = PluginHandlers.Plugin.WebSocketServer?.GetServerUrl() ?? $"ws://{address}:{port}";
                    
                    ImGui.Text($"Server URL: {serverUrl}");
                    
                    // Show if the actual port differs from configured port
                    if (actualPort != port)
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), 
                            $"Note: Using port {actualPort} instead of configured port {port} (configured port was in use)");
                    }
                    
                    ImGui.Text("Connect to this URL with a WebSocket client to receive text data");
                    
                    if (ImGui.Button("Stop Server"))
                    {
                        _ = StopWebSocketServerWithErrorHandling();
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("Restart Server"))
                    {
                        _ = RestartWebSocketServerWithErrorHandling();
                    }
                }
                else
                {
                    if (ImGui.Button("Start Server"))
                    {
                        _ = InitializeWebSocketServerWithErrorHandling();
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("Test Connection"))
                    {
                        _ = TestWebSocketConnection();
                    }
                    
                    // Show helpful information about port conflicts
                    ImGui.Separator();
                    ImGui.Text("Port Conflict Help:");
                    ImGui.BulletText("The server will automatically find an available port if the configured port is in use");
                    ImGui.BulletText("OS-assigned ports (port 0) are used when possible for maximum compatibility");
                    ImGui.BulletText("Safe port range: 49152-65535 (dynamic/ephemeral ports)");
                    ImGui.BulletText("Common causes: Other applications, previous server instances, firewall settings");
                    ImGui.BulletText("Check Windows Firewall or antivirus software if connection issues persist");
                    ImGui.BulletText("Ports below 1024 are privileged and may show as SYSTEM process");
                }
                
                // Show status message if any
                if (!string.IsNullOrEmpty(_statusMessage) && (DateTime.Now - _statusMessageTime).TotalSeconds < 5)
                {
                    var messageColor = _statusMessageIsError ? 
                        new System.Numerics.Vector4(1, 0, 0, 1) : 
                        new System.Numerics.Vector4(0, 1, 0, 1);
                    ImGui.TextColored(messageColor, _statusMessage);
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

    private void ShowStatusMessage(string message, bool isError)
    {
        _statusMessage = message;
        _statusMessageIsError = isError;
        _statusMessageTime = DateTime.Now;
    }

    private Task InitializeWebSocketServerWithErrorHandling()
    {
        try
        {
            return PluginHandlers.Plugin.InitializeWebSocketServer().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    ShowStatusMessage("WebSocket server started successfully", false);
                }
                else if (task.Exception != null)
                {
                    ShowStatusMessage($"Failed to start WebSocket server: {task.Exception.InnerException?.Message}", true);
                }
            });
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to start WebSocket server: {ex.Message}", true);
            return Task.CompletedTask;
        }
    }

    private Task StopWebSocketServerWithErrorHandling()
    {
        try
        {
            PluginHandlers.Plugin.WebSocketServer?.Dispose();
            PluginHandlers.Plugin.SetWebSocketServer(null);
            ShowStatusMessage("WebSocket server stopped", false);
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to stop WebSocket server: {ex.Message}", true);
        }
        return Task.CompletedTask;
    }

    private async Task RestartWebSocketServerWithErrorHandling()
    {
        try
        {
            if (PluginHandlers.Plugin.WebSocketServer != null)
            {
                await PluginHandlers.Plugin.WebSocketServer.RestartAsync();
                ShowStatusMessage("WebSocket server restarted successfully", false);
            }
            else
            {
                await InitializeWebSocketServerWithErrorHandling();
            }
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to restart WebSocket server: {ex.Message}", true);
        }
    }

    private async Task TestWebSocketConnection()
    {
        try
        {
            var webSocketServer = PluginHandlers.Plugin.WebSocketServer;
            if (webSocketServer != null)
            {
                var success = await webSocketServer.EnsureRunningAsync();
                if (success)
                {
                    ShowStatusMessage("WebSocket server connection test successful", false);
                }
                else
                {
                    ShowStatusMessage("WebSocket server connection test failed", true);
                }
            }
            else
            {
                ShowStatusMessage("WebSocket server is not initialized", true);
            }
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"WebSocket connection test error: {ex.Message}", true);
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
