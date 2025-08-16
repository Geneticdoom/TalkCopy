using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TalkCopy.Core.Handlers;

namespace TalkCopy;

internal class WebSocketServer : IDisposable
{
    private HttpListener? _httpListener;
    private readonly List<WebSocket> _connectedClients = new();
    private readonly object _clientsLock = new();
    private bool _isRunning = false;
    private CancellationTokenSource? _cancellationTokenSource;

    public bool IsRunning => _isRunning;

    public async Task StartAsync(int port)
    {
        if (_isRunning) return;

        try
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://localhost:{port}/");
            _httpListener.Start();

            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;

            PluginHandlers.PluginLog.Information($"WebSocket server started on port {port}");

            // Start listening for connections
            _ = Task.Run(() => ListenForConnectionsAsync(_cancellationTokenSource.Token));
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error($"Failed to start WebSocket server: {ex.Message}");
            _isRunning = false;
        }
    }

    public async Task StopAsync()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cancellationTokenSource?.Cancel();

        // Close all connected clients
        lock (_clientsLock)
        {
            foreach (var client in _connectedClients)
            {
                try
                {
                    if (client.State == WebSocketState.Open)
                    {
                        _ = Task.Run(async () => await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None));
                    }
                }
                catch (Exception ex)
                {
                    PluginHandlers.PluginLog.Error($"Error closing WebSocket client: {ex.Message}");
                }
            }
            _connectedClients.Clear();
        }

        _httpListener?.Stop();
        _httpListener?.Close();
        _httpListener = null;

        PluginHandlers.PluginLog.Information("WebSocket server stopped");
    }

    private async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var context = await _httpListener!.GetContextAsync();
                
                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    var webSocket = webSocketContext.WebSocket;

                    lock (_clientsLock)
                    {
                        _connectedClients.Add(webSocket);
                    }

                    PluginHandlers.PluginLog.Information($"WebSocket client connected. Total clients: {_connectedClients.Count}");

                    // Start handling this client
                    _ = Task.Run(() => HandleClientAsync(webSocket, cancellationToken));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    PluginHandlers.PluginLog.Error($"Error in WebSocket listener: {ex.Message}");
                }
            }
        }
    }

    private async Task HandleClientAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];

        try
        {
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client requested close", cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error($"Error handling WebSocket client: {ex.Message}");
        }
        finally
        {
            lock (_clientsLock)
            {
                _connectedClients.Remove(webSocket);
            }

            PluginHandlers.PluginLog.Information($"WebSocket client disconnected. Total clients: {_connectedClients.Count}");
        }
    }

    public async Task SendTextAsync(string text)
    {
        if (!_isRunning || _connectedClients.Count == 0) return;

        var buffer = Encoding.UTF8.GetBytes(text);
        var clientsToRemove = new List<WebSocket>();

        lock (_clientsLock)
        {
            foreach (var client in _connectedClients)
            {
                try
                {
                    if (client.State == WebSocketState.Open)
                    {
                        _ = Task.Run(async () => await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
                    }
                    else
                    {
                        clientsToRemove.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    PluginHandlers.PluginLog.Error($"Error sending to WebSocket client: {ex.Message}");
                    clientsToRemove.Add(client);
                }
            }

            // Remove disconnected clients
            foreach (var client in clientsToRemove)
            {
                _connectedClients.Remove(client);
            }
        }

        if (clientsToRemove.Count > 0)
        {
            PluginHandlers.PluginLog.Information($"Removed {clientsToRemove.Count} disconnected clients. Total clients: {_connectedClients.Count}");
        }
    }

    public void Dispose()
    {
        _ = Task.Run(async () => await StopAsync());
        _cancellationTokenSource?.Dispose();
    }
}
