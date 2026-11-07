using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RaceDay.Web.Infrastructure;

/// <summary>If the API says the token has expired, end the MVC session and send the user back to the login page.</summary>
public class SessionExpiredFilter : IExceptionFilter {
    public void OnException(ExceptionContext context) {
        if (context.Exception is not SessionExpiredException) return;
        context.HttpContext.Session.Clear();
        var tempData = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(context.HttpContext);
        tempData["Warning"] = "Your session has expired. Please log in again.";
        context.Result = new RedirectToActionResult("Login", "Account", null);
        context.ExceptionHandled = true;
    }
}
