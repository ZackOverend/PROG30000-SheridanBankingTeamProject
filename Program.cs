using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using SheridanBankingTeamProject.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.sqlite"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";  // Make sure this matches your actual login route
        // options.ReturnUrlParameter = "returnUrl";
        // options.AccessDeniedPath = "/Home/AccessDenied";
        // options.ReturnUrlParameter = string.Empty;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                var tempDataWrapper = context.HttpContext.RequestServices
                    .GetRequiredService<ITempDataDictionaryFactory>()
                    .GetTempData(context.HttpContext);
                
                tempDataWrapper["ReturnUrl"] = context.RedirectUri;
                context.Response.Redirect("/Home/Login");
                return Task.CompletedTask;
            }
        };
    }
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
