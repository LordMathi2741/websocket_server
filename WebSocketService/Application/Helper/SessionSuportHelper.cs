using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebSocketService.Application.Helper;

public class SessionSuportHelper
{
    
    public static readonly ConcurrentDictionary<WebSocket, string> sockets = new();
    public static void AddSocket(WebSocket webSocket, string clientId)
    {
        sockets.TryAdd(webSocket, clientId);
    }
    
    public static void RemoveSocket(WebSocket webSocket)
    {
        sockets.TryRemove(webSocket, out _);
    }
}