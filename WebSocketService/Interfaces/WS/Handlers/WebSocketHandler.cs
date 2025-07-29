using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketService.Application.Helper;
using WebSocketService.Interfaces.WS.Resources;

namespace WebSocketService.Interfaces.WS.Handlers;

public static class WebSocketHandler
{

    public static async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        try
        {
      
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var clientId = Encoding.UTF8.GetString(buffer, 0, result.Count);
                SessionSuportHelper.AddSocket(webSocket, clientId);
                Console.WriteLine($"Client connected: {clientId}");
            }

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    SessionSuportHelper.RemoveSocket(webSocket);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var request = JsonSerializer.Deserialize<SenderNotificationResource>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (request != null && !string.IsNullOrEmpty(request.Message) && SessionSuportHelper.sockets.TryGetValue(webSocket, out var senderId))
                    {
                        Console.WriteLine($"Received from {senderId}: {request.Message}");
                        await BroadcastMessageHandler.Handle(senderId, request.Message);
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
            SessionSuportHelper.RemoveSocket(webSocket);
        }
    }
}