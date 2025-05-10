using Microsoft.EntityFrameworkCore;
using StockSystem2025.Hubs;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using StockSystem2025.Models;
using StockSystem2025.Services;
using StockSystem2025.SFLServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSignalR();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure database
var connectionString = builder.Configuration.GetConnectionString("SQLConn");
builder.Services.AddDbContext<StockdbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure file upload limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100 MB
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Add services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICriteriaService, CriteriaService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<RecommendationsStockService>();
builder.Services.AddScoped<SFLIFollowListService, SFLFollowListService>();
builder.Services.AddScoped<SFLIStockService, SFLStockService>();
builder.Services.AddScoped<SFLISettingsService, SFLSettingsService>();
builder.Services.AddScoped<INewEconomicLinksService, NewEconomicLinksService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ProgressHub>("/progressHub");

app.Run();