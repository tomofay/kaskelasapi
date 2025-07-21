using System.ComponentModel.DataAnnotations;

namespace KasKelasApi.DTOs
{
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } // Optional, bisa dikosongkan jika tidak perlu verifikasi lama
        [Required]
        public string NewPassword { get; set; }
    }
}
