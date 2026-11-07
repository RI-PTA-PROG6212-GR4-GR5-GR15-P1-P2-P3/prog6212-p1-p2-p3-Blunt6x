using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RaceDay.Web.Infrastructure;

/// <summary>
/// Session-based authorisation. Requires a signed-in user (and optionally a specific role).
/// Unauthenticated visitors are sent to the login page; users with the wrong role are sent to "access denied".
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class SessionAuthorizeAttribute(string? role = null) : Attribute, IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
        var http = context.HttpContext;
        var user = http.Session.GetUser();
        if (user is null) {
            var tempData = http.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(http);
            tempData["Warning"] = "Please log in to continue.";
            var returnUrl = HttpMethods.IsGet(http.Request.Method) ? $"{http.Request.Path}{http.Request.QueryString}" : null;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }
        if (role is not null && !string.Equals(user.Role, role, StringComparison.Ordinal))
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }
}
