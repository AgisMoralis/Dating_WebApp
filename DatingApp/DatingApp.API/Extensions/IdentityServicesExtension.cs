using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DatingApp.API.Extensions;

public static class IdentityServicesExtension
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
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
        });

        return services;
    }
}
