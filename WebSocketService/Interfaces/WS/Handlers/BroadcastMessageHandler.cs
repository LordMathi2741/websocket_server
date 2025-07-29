using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketService.Application.Helper;
using WebSocketService.Domain.Model.Commands;

namespace WebSocketService.Interfaces.WS.Handlers;

public class BroadcastMessageHandler
{
    public static async Task Handle(string senderId, string message)
    {
        var resource = new CreateSenderNotificationCommand(senderId, message);
        var payload = JsonSerializer.Serialize(resource);
        var bytes = Encoding.UTF8.GetBytes(payload);

        foreach (var socket in SessionSuportHelper.sockets.Keys)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}