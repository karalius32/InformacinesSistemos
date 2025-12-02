using Coinbase.Commerce;
using InformacinesSistemos.Data;
using InformacinesSistemos.Models.Enums;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DB and Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptionsAction =>
    {
        npgsqlOptionsAction.MapEnum<UserRole>("user_role", "public");
        npgsqlOptionsAction.MapEnum<SubscriptionLevel>("subscription_level", "public");
        npgsqlOptionsAction.MapEnum<AuthorRole>("author_role", "public");
    }));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Disable password complexity requirements
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<LibraryContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();

// register commerce client
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var apiKey = cfg["CoinbaseCommerce:ApiKey"];
    return new CommerceApi(apiKey);
});

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    Console.WriteLine("UNHANDLED EXCEPTION: " + e.ExceptionObject);
};

TaskScheduler.UnobservedTaskException += (s, e) =>
{
    Console.WriteLine("UNOBSERVED TASK EXCEPTION: " + e.Exception);
    e.SetObserved();
};

AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    Console.WriteLine("PROCESS EXIT");
};

app.Use(async (ctx, next) =>
{
    Console.WriteLine($"--> {ctx.Request.Method} {ctx.Request.Path} at {DateTime.UtcNow:o}");
    await next();
    Console.WriteLine($"<-- {ctx.Response.StatusCode} {ctx.Request.Path}");
});

var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};

// If you want to be strict you can add known proxies,
// but with ngrok in dev it's easiest to clear these:
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedOptions);

app.Run();
