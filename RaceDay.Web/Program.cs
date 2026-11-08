using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options => {
    options.Filters.Add<SessionExpiredFilter>();
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());   // CSRF protection on every POST
});

// Session: holds the signed-in user's token, name and role across pages.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddHttpContextAccessor();

// Typed HttpClient for the Part 2 API. The MVC app never touches the database or Azure directly.
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl)) throw new InvalidOperationException("ApiSettings:BaseUrl must be configured.");
var allowUntrusted = builder.Configuration.GetValue<bool>("ApiSettings:AllowUntrustedCertificate");
builder.Services.AddHttpClient<IRaceDayApiClient, RaceDayApiClient>(client => {
    client.BaseAddress = new Uri(apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => {
    var handler = new HttpClientHandler();
    if (allowUntrusted) handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;   // local dev certificates only
    return handler;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
