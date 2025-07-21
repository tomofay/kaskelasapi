using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasKelasApi.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WeekNumber { get; set; }
        public decimal Amount { get; set; }
        public string? ProofUrl { get; set; } // Jadikan nullable
        public string Status { get; set; } // "Pending", "Accepted", "Rejected"
        public DateTime SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public int? VerifiedBy { get; set; }
        public virtual User User { get; set; }
        public virtual User Verifier { get; set; }
    }
}
