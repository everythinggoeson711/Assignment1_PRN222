using System.ComponentModel.DataAnnotations;

namespace Montra.DAL.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // "Admin" | "Therapist"
        [Required, MaxLength(20)]
        public string Role { get; set; } = "Therapist";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Therapist? Therapist { get; set; }
    }
}
