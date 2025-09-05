using DatingApp.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Extensions;

public static class IdentityServicesExtension
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentityCore<Entities.Member>(options =>
        {
            // Here we can configure a lot of options (e.g. tokens, user, passwords etc) using "Microsoft.AspNetCore.Identity"
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = false;
        })
            .AddRoles<Entities.Role>()
            .AddRoleManager<RoleManager<Entities.Role>>()
            .AddEntityFrameworkStores<DataContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var token = config["TokenKey"] ?? throw new InvalidOperationException("Cannot access TokenKey from appsettings.json");
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(token)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // For any normal client HTTP request (that goes to a controller), the JWT is sent in the Authorization header
                    // For the client "handshake" SignalR HTTP request, the client app shall fill the query parameter "access_token"
                    var accessToken = context.Request.Query["access_token"];
                    // The initial SignalR connection is established via an HTTP request (called the "handshake")
                    // During the "handshake", we have access to the full HTTP context (e.g. request path "/hubs/presence" or query param "access_token")
                    var httpRequestPath = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && httpRequestPath.StartsWithSegments("/hubs/presence"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
            .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));

        return services;
    }
}
