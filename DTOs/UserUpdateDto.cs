using System.ComponentModel.DataAnnotations;

namespace KasKelasApi.DTOs
{
    public class UserUpdateDto
    {
        [Required]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int? ClassId { get; set; }
        public int? ParentId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}