using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using KasKelasApi.DTOs;
using System.IO;

namespace KasKelasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly KasKelasApiDbContext _context;
        private readonly ILogger<PaymentController> _logger;
        private readonly IWebHostEnvironment _env;

        public PaymentController(KasKelasApiDbContext context, ILogger<PaymentController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetAll() =>
            await _context.Payments.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> Get(int id)
        {
            var item = await _context.Payments.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Payment with id {Id} not found", id);
                return NotFound();
            }
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> Create([FromBody] Payment item)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Payment create");
                return BadRequest(ModelState);
            }
            _context.Payments.Add(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Payment");
                return StatusCode(500, "Internal server error");
            }
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Payment item)
        {
            if (id != item.Id)
            {
                _logger.LogWarning("Payment id mismatch: {Id} vs {ItemId}", id, item.Id);
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Payment update");
                return BadRequest(ModelState);
            }
            _context.Entry(item).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Payments.Any(e => e.Id == id))
                {
                    _logger.LogWarning("Payment with id {Id} not found for update", id);
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Payment");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Payments.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Payment with id {Id} not found for delete", id);
                return NotFound();
            }
            _context.Payments.Remove(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Payment");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPaymentProof([FromForm] PaymentUploadDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Payment upload");
                return BadRequest(ModelState);
            }

            if (dto.ProofFile == null || dto.ProofFile.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            // Perbaiki penentuan web root path
            var webRootPath = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var folderPath = Path.Combine(webRootPath, "payment-proofs");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.ProofFile.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ProofFile.CopyToAsync(stream);
            }

            // Ambil nominal kas per minggu dari setting
            var kasSetting = await _context.KasSettings.FirstOrDefaultAsync();
            if (kasSetting == null)
            {
                return StatusCode(500, new { message = "Kas setting not found." });
            }
            if (kasSetting.AmountPerWeek <= 0)
            {
                return BadRequest(new { message = "Nominal per minggu tidak valid." });
            }

            // Hitung week number otomatis
            int weekNumber = (int)(dto.Amount / kasSetting.AmountPerWeek);

            // Cari minggu terakhir yang sudah dibayar user
            int lastPaidWeek = await _context.Payments
                .Where(p => p.UserId == dto.UserId)
                .OrderByDescending(p => p.WeekNumber)
                .Select(p => p.WeekNumber)
                .FirstOrDefaultAsync();

            // Jika belum pernah bayar, mulai dari minggu ke-1
            int startWeek = lastPaidWeek + 1;

            var payment = new Payment
            {
                UserId = dto.UserId,
                WeekNumber = weekNumber,
                Amount = dto.Amount,
                ProofUrl = $"payment-proofs/{fileName}",
                Status = "Pending",
                SubmittedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "File uploaded successfully",
                filePath = payment.ProofUrl,
                paymentId = payment.Id,
                weekNumber = payment.WeekNumber,
                startWeek = startWeek // Minggu pertama yang dibayar pada transaksi ini
            });
        }

        // POST: api/Payment/{id}/acc
        [HttpPost("{id}/acc")]
        public async Task<IActionResult> AccPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            if (payment.Status == "Diterima")
                return BadRequest("Pembayaran sudah di-ACC.");

            payment.Status = "Diterima";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pembayaran berhasil di-ACC." });
        }

        // POST: api/Payment/{id}/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            if (payment.Status == "Ditolak")
                return BadRequest("Pembayaran sudah ditolak.");

            payment.Status = "Ditolak";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pembayaran berhasil ditolak." });
        }

        // Endpoint untuk mengetahui tunggakan semua user
        [HttpGet("arrears")]
        public async Task<IActionResult> GetAllUserArrears()
        {
            var kasSetting = await _context.KasSettings.FirstOrDefaultAsync();
            if (kasSetting == null)
                return StatusCode(500, new { message = "Kas setting not found." });
            if (kasSetting.AmountPerWeek <= 0)
                return BadRequest(new { message = "Nominal per minggu tidak valid." });

            var startDate = kasSetting.StartDate;
            var now = DateTime.UtcNow.Date;
            int currentWeek = (int)((now - startDate.Date).TotalDays / 7) + 1;

            // Ambil semua user kecuali admin (misal Role == "admin")
            var users = await _context.Users
                .Where(u => u.Role != "admin")
                .ToListAsync();

            var result = new List<object>();
            foreach (var user in users)
            {
                // Minggu yang sudah dibayar (status Diterima)
                int paidWeeks = await _context.Payments
                    .Where(p => p.UserId == user.Id && p.Status == "Diterima")
                    .SumAsync(p => p.WeekNumber);

                // Minggu tunggakan manual (status Tunggakan)
                int manualArrears = await _context.Payments
                    .Where(p => p.UserId == user.Id && p.Status == "Tunggakan")
                    .SumAsync(p => p.WeekNumber);

                // Arrears = minggu berjalan - minggu terbayar + tunggakan manual
                int arrears = (currentWeek - paidWeeks) + manualArrears;
                if (arrears < 0) arrears = 0;

                result.Add(new
                {
                    userId = user.Id,
                    userName = user.FullName,
                    paidWeeks,
                    currentWeek,
                    arrears
                });
            }

            return Ok(result);
        }

        [HttpPost("add-arrears")]
        public async Task<IActionResult> AddArrears([FromBody] AddArrearsDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (dto.WeekCount <= 0)
                return BadRequest(new { message = "WeekCount harus lebih dari 0." });

            var kasSetting = await _context.KasSettings.FirstOrDefaultAsync(x => x.ClassId == user.ClassId);
            if (kasSetting == null || kasSetting.AmountPerWeek <= 0)
                return BadRequest(new { message = "Kas setting tidak ditemukan atau AmountPerWeek tidak valid." });

            var payment = new Payment
            {
                UserId = dto.UserId,
                WeekNumber = dto.WeekCount,
                Amount = kasSetting.AmountPerWeek * dto.WeekCount,
                ProofUrl = "tunggakan",
                Status = "Tunggakan",
                SubmittedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tunggakan berhasil ditambahkan.", payment });
        }

    }

}