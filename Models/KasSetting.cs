using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KasKelasApi.Models
{
    public class KasSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; }

        [JsonIgnore] // Agar tidak perlu dikirim saat POST/PUT
        [Required]
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [Required]
        public decimal AmountPerWeek { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

    }
}
