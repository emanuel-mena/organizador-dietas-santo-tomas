using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using organizador_dietas_santo_tomas.Data;
using organizador_dietas_santo_tomas.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(ResolveConnectionString(builder.Configuration)));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Railway termina TLS en su proxy; confiar en X-Forwarded-* de la red interna.
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<SpanishIdentityErrorDescriber>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddScoped<UserCreationService>();

var app = builder.Build();
app.UseForwardedHeaders();
// Antes de la redireccion HTTPS: el healthcheck de Railway llega por HTTP directo.
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path == "/healthz") { await ctx.Response.WriteAsync("ok"); return; }
    await next();
});
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

using (var scope = app.Services.CreateScope())
    await IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration, app.Logger);

app.Run();

static string? ResolveConnectionString(IConfiguration config)
{
    var configured = config.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(configured)) return configured;

    // Railway PostgreSQL expone DATABASE_URL (postgresql://usuario:clave@host:puerto/db).
    var url = config["DATABASE_URL"];
    if (string.IsNullOrWhiteSpace(url)) return null;
    var uri = new Uri(url);
    var credentials = uri.UserInfo.Split(':', 2);
    return new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null,
    }.ConnectionString;
}

public partial class Program { }
