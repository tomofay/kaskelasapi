using Microsoft.AspNetCore.Http;

namespace KasKelasApi.DTOs
{
    public class PaymentUploadDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public IFormFile ProofFile { get; set; }
    }
}

