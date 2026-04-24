using FinalAssignment.Therapy.Application.DependencyInjection;
using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Infrastructure.DependencyInjection;
using FinalAssignment.Therapy.Infrastructure.Seed;
using FinalAssignment.Therapy.Web.Hubs;
using FinalAssignment.Therapy.Web.Options;
using FinalAssignment.Therapy.Web.Services;
using FinalAssignment.Therapy.Web.Workers;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppointmentCleanupOptions>(builder.Configuration.GetSection(AppointmentCleanupOptions.SectionName));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddTransient<IUserClaimsPrincipalFactory, CookieUserClaimsPrincipalFactory>();
builder.Services.AddSingleton<IAdminDashboardNotifier, SignalRAdminDashboardNotifier>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Cookie.Name = "final-therapy-auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddHostedService<PendingAppointmentExpirationWorker>();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<AdminDashboardHub>("/hubs/admin-dashboard");
app.MapHub<ChatHub>("/hubs/chat");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
