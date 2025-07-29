using System.Net.WebSockets;

namespace WebSocketService.Domain.Rules;

public interface IOpenSocketRules
{
    Task SendMessageAsync(byte[] buffer,  WebSocketReceiveResult result, WebSocket webSocket);
}