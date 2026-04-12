using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SieuThiAnhDuong.Data;

namespace SieuThiAnhDuong
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
           

            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
    });
            builder.Services.AddControllersWithViews()
    .AddMvcOptions(options => {
        // Việt hóa lỗi sai định dạng dữ liệu (Invalid)
        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor((x) => $"Giá trị '{x}' không hợp lệ.");
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor((x) => "Trường này không được để trống.");
    });
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
               
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseMiddleware<SessionCheckMiddleware>();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
               pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
