using System.Collections.Generic;

namespace SieuThiAnhDuong.Models // Nhớ đổi tên Namespace nếu project của bạn tên khác
{
    public static class SessionControl
    {
        // Danh sách lưu: Tên đăng nhập -> Mã phiên duy nhất
        public static Dictionary<string, string> UserSessions = new Dictionary<string, string>();
    }
}