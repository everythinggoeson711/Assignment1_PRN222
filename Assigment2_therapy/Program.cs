using Montra.BLL.Services;
using Montra.DAL.Context;
using Montra.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

var databasePassword = builder.Configuration["Database:Password"]
    ?? builder.Configuration["MONTRA_DB_PASSWORD"];

builder.Services.Configure<CenterBookingSettings>(options =>
{
    options.SecretKey = builder.Configuration["Stripe:SecretKey"] ?? builder.Configuration["STRIPE_SECRET_KEY"] ?? string.Empty;
    options.PublishableKey = builder.Configuration["Stripe:PublishableKey"] ?? builder.Configuration["STRIPE_PUBLISHABLE_KEY"] ?? string.Empty;
    options.WebhookSecret = builder.Configuration["Stripe:WebhookSecret"] ?? builder.Configuration["STRIPE_WEBHOOK_SECRET"] ?? string.Empty;
    options.PendingPaymentExpiryMinutes = builder.Configuration.GetValue<int?>("Stripe:PendingPaymentExpiryMinutes") ?? 15;
});

if (connectionString.Contains("{DB_PASSWORD}", StringComparison.Ordinal))
{
    if (string.IsNullOrWhiteSpace(databasePassword))
    {
        throw new InvalidOperationException("Database password was not configured. Set Database:Password or MONTRA_DB_PASSWORD.");
    }

    connectionString = connectionString.Replace("{DB_PASSWORD}", databasePassword, StringComparison.Ordinal);
}

// ── Database ──────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ── Session ───────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── DAL Repositories ──────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITherapistRepository, TherapistRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// ── BLL Services ──────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITherapistService, TherapistService>();
builder.Services.AddScoped<ITherapyServiceBLL, TherapyServiceBLL>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IReferralService, ReferralService>();
builder.Services.AddScoped<ICenterBookingService, CenterBookingService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Auto-migrate and seed database ───────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await TrackingCodeMaintenanceService.BackfillMissingCodesAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
