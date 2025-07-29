
using WebSocketService.Application.UseCases;
using WebSocketService.Domain.Rules;
using WebSocketService.Interfaces.WS.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddTransient<ICloseSocketRules, CloseSocketUseCase>();
builder.Services.AddTransient<IOpenSocketRules, OpenSocketUseCase>();

builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(8080));

var app = builder.Build();

app.UseWebSockets();

app.MapGet("/ws",  async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await WebSocketHandler.HandleWebSocketAsync(webSocket,
            app.Services.GetRequiredService<IOpenSocketRules>(),
            app.Services.GetRequiredService<ICloseSocketRules>());
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.UseCors("AllowAllOrigins");

app.Run();
