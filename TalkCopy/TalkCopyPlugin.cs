using Dalamud.Plugin;
using TalkCopy.Automation;
using TalkCopy.Core.Handlers;
using TalkCopy.Windows;
using System.Threading.Tasks;

namespace TalkCopy;

public sealed class TalkCopyPlugin : IDalamudPlugin
{
    internal Configuration Config { get; private set; }
    internal WindowHandler WindowHandler { get; private set; }
    internal WebSocketServer? WebSocketServer { get; private set; }

    public static PluginMode CurrentMode = PluginMode.Default;

    public TalkCopyPlugin(IDalamudPluginInterface pluginInterface)
    {
        PluginHandlers.Start(ref pluginInterface, this);
        Config = PluginHandlers.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        WindowHandler = new WindowHandler(pluginInterface);
        
        // Initialize WebSocket server if enabled
        if (Config.UseWebSocket)
        {
            _ = Task.Run(async () => await InitializeWebSocketServer());
        }
        
        new Creator().Create();
    }

    public async Task InitializeWebSocketServer()
    {
        WebSocketServer = new WebSocketServer();
        await WebSocketServer.StartAsync(Config.WebSocketPort);
    }

    internal void SetWebSocketServer(WebSocketServer? server)
    {
        WebSocketServer = server;
    }

    public void Dispose()
    {
        WebSocketServer?.Dispose();
        WindowHandler?.Dispose();
    }
}

public enum PluginMode
{
    Default,
    TextCopy
}
