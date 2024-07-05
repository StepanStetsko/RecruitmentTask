using Microsoft.EntityFrameworkCore;
using Recruitment_task.Data;
using Recruitment_task.Models;
using Recruitment_task.Services;

var builder = WebApplication.CreateBuilder(args);

string connectionToInstruments = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AssetContext>(option => option.UseSqlServer(connectionToInstruments, op =>
{
    op.EnableRetryOnFailure();
}));

builder.Services.AddHttpClient<FetchAssetService>();
builder.Services.AddTransient<FetchAssetService>();
builder.Services.AddTransient<AccessTokenService>();
builder.Services.AddTransient<FetchBarsServise>();
builder.Services.AddTransient<WebSocketService>();

builder.Services.AddSingleton<WebSocketService>();

var app = builder.Build();

app.UseWebSockets();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var socketService = app.Services.GetRequiredService<WebSocketService>();

    socketService.StartWebSocketServer();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();