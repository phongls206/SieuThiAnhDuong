using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SieuThiAnhDuong.Models; // Sửa lại Namespace nếu cần

public class SessionCheckMiddleware
{
    private readonly RequestDelegate _next;
    public SessionCheckMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
            var userName = context.User.Identity.Name;
            var currentCookieSession = context.User.FindFirst("UserSessionGuid")?.Value;

            // Kiểm tra trong sổ xem user này có bị ai khác đăng nhập đè lên chưa
            if (!string.IsNullOrEmpty(userName) && SessionControl.UserSessions.ContainsKey(userName))
            {
                if (SessionControl.UserSessions[userName] != currentCookieSession)
                {
                    // Mã không khớp -> Có người khác vào -> Đuổi ra trang Login
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Kèm theo thông báo lỗi trên URL để qua trang Login hiện thông báo
                    // Sửa dòng Redirect cũ thành dòng này
                    context.Response.Redirect("/Account/Login?announcement=session_stolen");
                    return;
                }
            }
        }
        await _next(context);
    }
}