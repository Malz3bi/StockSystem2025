using StockSystem2025.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(); // Add SignalR

// 🔹 تفعيل الجلسات (Sessions)
builder.Services.AddDistributedMemoryCache(); // تخزين الجلسات بالذاكرة
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // وقت انتهاء الجلسة
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// إعداد الاتصال بقاعدة البيانات
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // لتفاصيل الأخطاء في البيئة غير التطويرية
}



app.UseHttpsRedirection();

// تفعيل الملفات الثابتة مثل CSS و JS
app.UseStaticFiles();

// تفعيل التوجيه
app.UseRouting();

// 🔹 تفعيل الجلسات
app.UseSession();

// تفعيل صلاحيات الوصول (لو كنت تستخدم Authorization)
app.UseAuthorization();

// التوجيه للكنترولر والاكشن
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
