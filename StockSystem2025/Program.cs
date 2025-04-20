using StockSystem2025.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // لتفاصيل الأخطاء في البيئة غير التطويرية
}

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
