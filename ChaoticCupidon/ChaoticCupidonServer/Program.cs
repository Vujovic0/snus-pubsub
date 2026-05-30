using ChaoticCupidonServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddSignalR();

builder.Services.AddHostedService<LetterService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.MapHub<LetterHub>("/letterHub");

app.Run();
