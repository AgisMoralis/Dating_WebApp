using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Design;

namespace DatingApp.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            throw new OperationException("Cannot extract the user id from the provided token"));

        return userId;
    }

    public static string GetUsername(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.Name) ??
            throw new OperationException("Cannot extract the username from the provided token");

        return username;
    }
}
