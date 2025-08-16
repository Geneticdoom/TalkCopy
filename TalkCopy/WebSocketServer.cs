using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TalkCopy.Core.Handlers;

namespace TalkCopy;

internal class WebSocketServer : IDisposable
{
    private TcpListener? _listener;
    private readonly List<WebSocket> _connectedClients = new();
    private readonly object _clientsLock = new();
    private bool _isRunning = false;
    private CancellationTokenSource? _cancellationTokenSource;
    private string? _currentAddress;
    private int _currentPort;

    public bool IsRunning => _isRunning;

    public async Task StartAsync(string address, int preferredPort)
    {
        if (_isRunning) return;

        try
        {
            await StopAsync();

            var ipAddress = IPAddress.Parse(address);
            _listener = new TcpListener(ipAddress, preferredPort == 0 ? 0 : preferredPort);
            _listener.Start();

            _currentAddress = address;
            _currentPort = ((IPEndPoint)_listener.LocalEndpoint).Port;

            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;

            PluginHandlers.PluginLog.Information($"WebSocket server started on ws://{address}:{_currentPort}");

            _ = Task.Run(() => AcceptLoopAsync(_cancellationTokenSource.Token));
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error($"Failed to start WebSocket server: {ex.Message}");
            _isRunning = false;
            _listener?.Stop();
            _listener = null;
            throw;
        }
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener!.AcceptTcpClientAsync(cancellationToken);
                _ = Task.Run(() => HandleConnectionAsync(tcpClient, cancellationToken));
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    PluginHandlers.PluginLog.Error($"Error accepting connection: {ex.Message}");
                }
            }
        }
    }

    private async Task HandleConnectionAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        using var networkStream = tcpClient.GetStream();

        // WebSocket handshake
        var buffer = new byte[4096];
        var bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (!request.Contains("Upgrade: websocket", StringComparison.OrdinalIgnoreCase))
        {
            tcpClient.Close();
            return;
        }

        var secWebSocketKey = ExtractWebSocketKey(request);
        if (secWebSocketKey == null)
        {
            tcpClient.Close();
            return;
        }

        var acceptKey = ComputeWebSocketAcceptKey(secWebSocketKey);
        var response = "HTTP/1.1 101 Switching Protocols\r\n" +
                       "Connection: Upgrade\r\n" +
                       "Upgrade: websocket\r\n" +
                       $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";

        var responseBytes = Encoding.UTF8.GetBytes(response);
        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);

        // Wrap the network stream in a WebSocket
        using var webSocket = WebSocket.CreateFromStream(networkStream, isServer: true, subProtocol: null, TimeSpan.FromSeconds(30));

        lock (_clientsLock)
        {
            _connectedClients.Add(webSocket);
        }

        PluginHandlers.PluginLog.Information($"WebSocket client connected. Total clients: {_connectedClients.Count}");

        await HandleClientAsync(webSocket, cancellationToken);

        lock (_clientsLock)
        {
            _connectedClients.Remove(webSocket);
        }

        PluginHandlers.PluginLog.Information($"WebSocket client disconnected. Total clients: {_connectedClients.Count}");
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
    }

    public Task SendTextAsync(string text)
    {
        if (!_isRunning || _connectedClients.Count == 0) return Task.CompletedTask;

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

            foreach (var client in clientsToRemove)
            {
                _connectedClients.Remove(client);
            }
        }

        if (clientsToRemove.Count > 0)
        {
            PluginHandlers.PluginLog.Information($"Removed {clientsToRemove.Count} disconnected clients. Total clients: {_connectedClients.Count}");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (!_isRunning) return Task.CompletedTask;

        _isRunning = false;
        _cancellationTokenSource?.Cancel();

        lock (_clientsLock)
        {
            foreach (var client in _connectedClients)
            {
                try
                {
                    if (client.State == WebSocketState.Open)
                    {
                        _ = client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
                    }
                }
                catch { }
            }
            _connectedClients.Clear();
        }

        _listener?.Stop();
        _listener = null;

        PluginHandlers.PluginLog.Information($"WebSocket server stopped and port {_currentPort} released");

        return Task.CompletedTask;
    }

    public int GetCurrentPort() => _currentPort;
    public string GetCurrentAddress() => _currentAddress ?? "localhost";
    public string GetServerUrl() => $"ws://{GetCurrentAddress()}:{GetCurrentPort()}";

    // --- Compatibility methods for existing code ---
    public async Task<bool> EnsureRunningAsync()
    {
        if (!_isRunning)
        {
            try
            {
                if (_currentAddress != null)
                {
                    await StartAsync(_currentAddress, _currentPort);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                PluginHandlers.PluginLog.Error($"EnsureRunningAsync failed: {ex.Message}");
                return false;
            }
        }
        return true;
    }

    public async Task RestartAsync()
    {
        try
        {
            await StopAsync();
            if (_currentAddress != null)
            {
                await StartAsync(_currentAddress, _currentPort);
            }
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error($"RestartAsync failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _ = StopAsync();
        _cancellationTokenSource?.Dispose();
    }

    // --- Helpers for WebSocket handshake ---
    private static string? ExtractWebSocketKey(string request)
    {
        foreach (var line in request.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring("Sec-WebSocket-Key:".Length).Trim();
            }
        }
        return null;
    }

    private static string ComputeWebSocketAcceptKey(string secWebSocketKey)
    {
        const string WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        string concatenated = secWebSocketKey + WebSocketGuid;
        byte[] concatenatedAsBytes = Encoding.UTF8.GetBytes(concatenated);
        byte[] sha1Hash = System.Security.Cryptography.SHA1.HashData(concatenatedAsBytes);
        return Convert.ToBase64String(sha1Hash);
    }
}
