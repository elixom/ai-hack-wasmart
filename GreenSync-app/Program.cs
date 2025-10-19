using GreenSync.Lib.Services;
using GreenSync.Lib.Data;
using GreenSync.Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework and Database Context
builder.Services.AddDbContext<GreenSyncDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Add ASP.NET Core Identity with custom user
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<GreenSyncDbContext>()
.AddDefaultTokenProviders();

// Configure Identity Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Register our services
builder.Services.AddScoped<IReportService, GreenSync.Lib.Services.EntityFramework.EfReportService>();
builder.Services.AddScoped<IEcoCreditService, GreenSync.Lib.Services.EntityFramework.EfEcoCreditService>();
builder.Services.AddScoped<IRouteService, GreenSync.Lib.Services.EntityFramework.EfRouteService>();
builder.Services.AddScoped<IFleetVehicleService, GreenSync.Lib.Services.EntityFramework.EfFleetVehicleService>();
builder.Services.AddScoped<IAuthService, GreenSync.Lib.Services.EntityFramework.EfAuthService>();
builder.Services.AddSingleton<IMapsService, AzureMapsService>();
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Add session middleware
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

// Add Areas routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
