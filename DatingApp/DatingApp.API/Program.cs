using DatingApp.API.Extensions;
using DatingApp.API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<DatingApp.API.SignalR.PresenceHub>("/hubs/presence");
app.MapHub<DatingApp.API.SignalR.MessageHub>("/hubs/message");
app.MapFallbackToController("Index", "Fallback");

app.Run();
