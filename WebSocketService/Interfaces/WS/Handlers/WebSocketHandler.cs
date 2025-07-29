using System.Net.WebSockets;
using WebSocketService.Application.Helper;
using WebSocketService.Domain.Rules;

namespace WebSocketService.Interfaces.WS.Handlers;

public static class WebSocketHandler
{
    public static async Task HandleWebSocketAsync(WebSocket webSocket, IOpenSocketRules openSocketRules, ICloseSocketRules closeSocketRules)
    {
        var buffer = new byte[1024 * 4];
        try
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
               await closeSocketRules.ConnectClient(buffer,webSocket, result);
            }
            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Close: 
                        await closeSocketRules.CloseSocket(webSocket);
                        break;
                    case WebSocketMessageType.Text:
                        await openSocketRules.SendMessageAsync(buffer, result, webSocket);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            closeSocketRules.ConnectClientError(webSocket,ex);
        }
    }
}