using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using naif_katalog.Services.Abstract;
using naif_katalog.Services.Concrete;
using System;
using System.Reflection;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new System.Globalization.CultureInfo("en-US");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor(); // ApiService requires this
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var apiAddress = builder.Configuration["ApiAdress"] ?? "https://localhost:3434/";
    if (!apiAddress.EndsWith("/")) apiAddress += "/";
    client.BaseAddress = new Uri(apiAddress);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
    UseProxy = false
});

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<IOrderEmailService, OrderEmailService>();
builder.Services.AddSingleton<naif_katalog.Services.Concrete.HomeContentStore>();
builder.Services.AddSingleton<naif_katalog.Services.Abstract.IOrderStore, naif_katalog.Services.Concrete.OrderStore>();
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 30 * 1024 * 1024);

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireAssertion(context =>
        context.User.Identity?.IsAuthenticated == true &&
        context.User.Claims.Any(claim =>
            (claim.Type == System.Security.Claims.ClaimTypes.Role
             || claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase)
             || claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase)
             || claim.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))
            && (claim.Value == "1" || claim.Value == "2"))));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
