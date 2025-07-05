using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Design;

namespace DatingApp.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUsername(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            throw new OperationException("Cannot locate the username from the provided token");

        return username;
    }
}
