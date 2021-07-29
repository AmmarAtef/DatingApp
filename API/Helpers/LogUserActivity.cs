using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultAction = await next();
            if (!resultAction.HttpContext.User.Identity.IsAuthenticated)
            {
                return;
            }
            var userId = resultAction.HttpContext.User.GetUserId();
            var repo = resultAction.HttpContext.RequestServices.GetService<IUnitOfWork>();
            var user = await repo.UserRepository.GetUserByIdAsync(userId);
            user.LastActive = DateTime.UtcNow;
            await repo.Complete();
        }
    }
}
