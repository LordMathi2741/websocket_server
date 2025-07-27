using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;

namespace WebSocketService;

public static class WebSocketHandler
{
    private static readonly ConcurrentDictionary<WebSocket, string> _sockets = new();

    public static async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var clientId = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _sockets.TryAdd(webSocket, clientId);
                Console.WriteLine($"Client connected: {clientId}");
            }

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _sockets.TryRemove(webSocket, out _);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (_sockets.TryGetValue(webSocket, out var senderId))
                    {
                        Console.WriteLine($"Received from {senderId}: {message}");
                        await BroadcastMessageAsync(message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
            _sockets.TryRemove(webSocket, out _);
        }
    }

    private static async Task BroadcastMessageAsync(string message)
    {
        var payload = Encoding.UTF8.GetBytes(message);

        foreach (var socket in _sockets.Keys)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}