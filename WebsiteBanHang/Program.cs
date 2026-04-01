using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Web_dienmay.Models;
using Web_dienmay.Repositories;
using WebsiteBanHang.Services;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Web_dienmay.Services.InvoiceService>();
builder.Services.AddScoped<Web_dienmay.Services.PayOSService>();

// Email services
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddScoped<EmailTemplateService>();

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IShoppingCartRepository, EFShoppingCartRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();
builder.Services.AddScoped<IProductRatingRepository, EFProductRatingRepository>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection.");
}

// Accept common key variants from environment secrets to avoid startup failures.
defaultConnection = Regex.Replace(defaultConnection, @"(^|;)\s*userid\s*=", "$1User Id=", RegexOptions.IgnoreCase);
defaultConnection = Regex.Replace(defaultConnection, @"(^|;)\s*username\s*=", "$1User Id=", RegexOptions.IgnoreCase);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(
    defaultConnection,
    sqlOptions => sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null)));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Apply pending migrations on startup so the container can boot against a fresh MSSQL volume.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    const int maxAttempts = 15;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            dbContext.Database.Migrate();
            logger.LogInformation("Database migration completed.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database migration attempt {Attempt}/{MaxAttempts} failed.", attempt, maxAttempts);

            if (attempt == maxAttempts)
            {
                throw;
            }

            Thread.Sleep(TimeSpan.FromSeconds(4));
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.MapRazorPages();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    // Area route must come first (with Home as default controller)
    endpoints.MapControllerRoute(
        name: "Admin",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    // Default route
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
