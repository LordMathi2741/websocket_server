using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketService.Application.Helper;
using WebSocketService.Domain.Rules;
using WebSocketService.Interfaces.WS.Handlers;
using WebSocketService.Interfaces.WS.Resources;

namespace WebSocketService.Application.UseCases;

public class OpenSocketUseCase : IOpenSocketRules
{
    public async Task SendMessageAsync(byte[] buffer, WebSocketReceiveResult result, WebSocket webSocket)
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