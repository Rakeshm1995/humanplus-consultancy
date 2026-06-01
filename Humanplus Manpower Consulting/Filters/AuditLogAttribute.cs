using HumanPlus.Domain.Entities.System;
using HumanPlus.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Humanplus_Manpower_Consulting.Filters
{
    public class AuditLogAttribute : ActionFilterAttribute
    {
        private readonly string _action;

        public AuditLogAttribute(string action) => _action = action;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var result = await next();

            if (result.Exception == null && context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<HumanPlusDbContext>();
                var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userId != null)
                {
                    db.AuditLogs.Add(new AuditLog
                    {
                        UserId = userId,
                        Action = _action,
                        EntityType = context.Controller.GetType().Name,
                        Details = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
                    });
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
