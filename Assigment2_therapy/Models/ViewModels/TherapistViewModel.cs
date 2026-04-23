using Montra.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace Assigment2_therapy.Models.ViewModels
{
    public class TherapistFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập chuyên môn.")]
        [Display(Name = "Chuyên môn")]
        public string Specialty { get; set; } = string.Empty;

        [Display(Name = "Tiểu sử")]
        public string Bio { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        // For Create only
        [Display(Name = "Email đăng nhập")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }

    public class TherapistSearchViewModel
    {
        public string? Search { get; set; }
        public string? Specialty { get; set; }
        public List<Therapist> Therapists { get; set; } = new();
        public List<string> Specialties { get; set; } = new();
    }
}
