using System.ComponentModel.DataAnnotations;

namespace KasKelasApi.DTOs
{
    public class UserBulkDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public int ClassId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        // Tambahkan field lain jika perlu, tanpa navigasi Class/Parent
    }
}
