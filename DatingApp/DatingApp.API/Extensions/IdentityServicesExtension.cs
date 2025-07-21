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
        });

        return services;
    }
}
