using BookingSportsField.Hubs;
using BookingSportsField.Models;
using BookingSportsField.Repository;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EFRepository<>));
builder.Services.AddScoped<IFacilityRepository, EFFacilityRepository>();
builder.Services.AddScoped<IFieldRepository, EFFieldRepository>();
builder.Services.AddScoped<IBookingRepository, EFBookingRepository>();
builder.Services.AddScoped<IImageRepository, EFImageRepository>();
builder.Services.AddScoped<IReviewRepository, EFReviewRepository>();
builder.Services.AddScoped<INotificationRepository, EFNotificationRepository>();
builder.Services.AddScoped<IFavoriteRepository, EFFavoriteRepository>();
// Use NameIdentifierUserIdProvider for consistent UserId mapping
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

builder.Services.AddRazorPages();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();



builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        options.Events = new OAuthEvents
        {
            OnRemoteFailure = context =>
            {
                // Xử lý khi đăng nhập thất bại hoặc người dùng từ chối
                context.Response.Redirect("/Identity/Account/Login?error=external_auth_failed");
                context.HandleResponse();  // Ngăn middleware tiếp tục xử lý lỗi
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddSession();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Tạo vai trò nếu chưa tồn tại
    var roleNames = new[] { SD.Role_Admin, SD.Role_Customer, SD.Role_FieldOwner };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Tạo tài khoản Admin mặc định nếu chưa có
    var adminUser = await userManager.FindByEmailAsync("admin@example.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "nguyenthanhvane@gmail.com",
            Email = "nguyenthanhvane@gmail.com",
            FullName = "Nguyễn Thành Văn",
            Address = "Bình Thạnh",
            DateOfBirth = DateTime.Parse("2004-04-29"), // Cập nhật thông tin theo nhu cầu
            PhoneNumber = "0913559460"
        };
        var result = await userManager.CreateAsync(adminUser, "Admin@1234"); // Cập nhật mật khẩu mạnh cho tài khoản admin

        if (result.Succeeded)
        {
            // Gán role Admin cho tài khoản này
            await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
        }
    }
}
app.UseRouting();

app.MapHub<BookingHub>("/bookingHub");

app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "Admin",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapControllerRoute(
        name: "FieldOwner",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
});
app.MapRazorPages();
app.UseStaticFiles();

app.Run();
