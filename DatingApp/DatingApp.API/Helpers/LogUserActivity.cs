using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatingApp.API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Here first the action of the API controller is executed
        // and then the user activity is logged
        var resultContext = await next();

        // If the user is not authenticated, we do not log activity
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;
        
        var userId = context.HttpContext.User.GetUserId();
        var unitOfWork = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var user = await unitOfWork.UserRepository.GetMemberByIdAsync(userId);
        if (user == null) return;
        
        user.LastActive = DateTime.UtcNow;
        await unitOfWork.CompleteAsync();
    }
}
