using System.Net.WebSockets;
using System.Text;
using WebSocketService.Application.Helper;
using WebSocketService.Domain.Rules;

namespace WebSocketService.Application.UseCases;

public class CloseSocketUseCase : ICloseSocketRules
{
    public async Task ConnectClient(byte[] buffer, WebSocket webSocket, WebSocketReceiveResult result)
    {
        var clientId = Encoding.UTF8.GetString(buffer, 0, result.Count);
        SessionSuportHelper.AddSocket(webSocket, clientId);
        Console.WriteLine($"Client connected: {clientId}");
    }

    public async Task CloseSocket(WebSocket webSocket)
    {
        SessionSuportHelper.RemoveSocket(webSocket);
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }

    public void ConnectClientError(WebSocket webSocket, Exception ex)
    {
        Console.WriteLine($"WS error: {ex.Message}");
        SessionSuportHelper.RemoveSocket(webSocket);
    }
}