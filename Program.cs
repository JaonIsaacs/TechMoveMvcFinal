using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;
using TechMove.Patterns.Observer;
using TechMove.Patterns.Factory;
using TechMove.Patterns.Strategy;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Register Services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();

// Register Observer Pattern Services
builder.Services.AddSingleton<ContractObservable>();
builder.Services.AddScoped<IContractObserver, ContractStatusLogger>();

// Register Factory Pattern Services
builder.Services.AddScoped<StandardServiceRequestFactory>();
builder.Services.AddScoped<PremiumServiceRequestFactory>();
builder.Services.AddScoped<ServiceRequestFactoryProvider>();

// Register Strategy Pattern Services
builder.Services.AddScoped<IPricingStrategy, StandardPricingStrategy>();
builder.Services.AddScoped<PremiumPricingStrategy>();
builder.Services.AddScoped<RegionalPricingStrategy>();
builder.Services.AddScoped<PricingContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // IMPORTANT: Add this before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
