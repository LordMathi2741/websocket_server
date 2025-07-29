namespace WebSocketService.Domain.Model.Commands;

public record CreateSenderNotificationCommand(string SenderId,string Message);