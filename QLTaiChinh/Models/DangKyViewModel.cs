using System.ComponentModel.DataAnnotations;

namespace QLTaiChinh.Models
{
    public class DangKyViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfimPW { get; set; }
    }
}
