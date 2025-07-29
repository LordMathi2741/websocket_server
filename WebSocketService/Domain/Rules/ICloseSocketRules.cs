using System.Net.WebSockets;

namespace WebSocketService.Domain.Rules;

public interface ICloseSocketRules
{
    Task ConnectClient(byte[] buffer, WebSocket webSocket,  WebSocketReceiveResult result);
    Task CloseSocket(WebSocket socket);
    void ConnectClientError(WebSocket socket, Exception exception);
}