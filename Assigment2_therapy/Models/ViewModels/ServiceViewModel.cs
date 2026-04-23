using System.ComponentModel.DataAnnotations;

namespace Assigment2_therapy.Models.ViewModels
{
    public class ServiceFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ.")]
        [Display(Name = "Tên dịch vụ")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 99999999, ErrorMessage = "Giá không hợp lệ.")]
        [Display(Name = "Giá (VNĐ)")]
        public decimal Price { get; set; }

        [Required]
        [Range(30, 480, ErrorMessage = "Thời lượng từ 30 đến 480 phút.")]
        [Display(Name = "Thời lượng (phút)")]
        public int DurationMinutes { get; set; } = 60;

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}
