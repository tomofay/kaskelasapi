using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasKelasApi.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FullName { get; set; } // Pastikan penulisan FullName benar
        public string Email { get; set; }
        public string Role { get; set; } // "Admin", "Murid", "OrangTua"
        public int? ClassId { get; set; } = null;
        public virtual Class Class { get; set; } = null; // Relasi ke kelas, nullable untuk admin
        public int? ParentId { get; set; } = null; // Untuk murid, relasi ke orang tua
        public virtual User Parent { get; set; } = null; // Untuk murid, relasi ke orang tua
        public virtual ICollection<User> Children { get; set; } = new List<User>(); // Untuk orang tua, relasi ke anak

        // Tambahkan untuk login:
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
