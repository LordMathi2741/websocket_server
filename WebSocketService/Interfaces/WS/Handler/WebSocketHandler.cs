using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketService.Domain.Model.Commands;
using WebSocketService.Interfaces.WS.Resources;

namespace WebSocketService.Interfaces.WS.Handler;

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
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var request = JsonSerializer.Deserialize<SenderNotificationResource>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (request != null && !string.IsNullOrEmpty(request.Message) && _sockets.TryGetValue(webSocket, out var senderId))
                    {
                        Console.WriteLine($"Received from {senderId}: {request.Message}");
                        await BroadcastMessageAsync(senderId, request.Message);
                    }
                    else
                    {
                        Console.WriteLine("Invalid or missing 'Message' in the received JSON.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WS error: {ex.Message}");
            _sockets.TryRemove(webSocket, out _);
        }
    }

    private static async Task BroadcastMessageAsync(string senderId, string message)
    {
        var resource = new CreateSenderNotificationCommand(senderId, message);
        var payload = JsonSerializer.Serialize(resource);
        var bytes = Encoding.UTF8.GetBytes(payload);

        foreach (var socket in _sockets.Keys)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}