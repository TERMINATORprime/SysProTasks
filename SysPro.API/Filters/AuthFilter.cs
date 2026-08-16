using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SysPro.API.Filters;

public class AuthFilter : ActionFilterAttribute
{
    override public void OnActionExecuting(ActionExecutingContext context)
    {
        //request
        var request = context.HttpContext.Request;
        var bearer = request.Headers["Authorization"];

        if (!string.IsNullOrEmpty(bearer))
        {
            
        }
        else
        {
            context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized); 
        }
        
        base.OnActionExecuting(context);
    }
}