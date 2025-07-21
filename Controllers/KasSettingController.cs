using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;
using KasKelasApi.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;

namespace KasKelasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KasSettingController : ControllerBase
    {
        private readonly KasKelasApiDbContext _context;
        private readonly ILogger<KasSettingController> _logger;
        public KasSettingController(KasKelasApiDbContext context, ILogger<KasSettingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KasSetting>>> GetAll() =>
            await _context.KasSettings.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<KasSetting>> Get(int id)
        {
            var item = await _context.KasSettings.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("KasSetting with id {Id} not found", id);
                return NotFound();
            }
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<KasSetting>> Create([FromBody] KasSettingDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for KasSetting create");
                return BadRequest(ModelState);
            }

            var kasSetting = new KasSetting
            {
                ClassId = dto.ClassId,
                AmountPerWeek = dto.AmountPerWeek,
                StartDate = dto.StartDate
            };

            _context.KasSettings.Add(kasSetting);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating KasSetting");
                return StatusCode(500, "Internal server error");
            }
            return CreatedAtAction(nameof(Get), new { id = kasSetting.Id }, kasSetting);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] KasSettingDto dto)
        {
            if (id != dto.ClassId && id != dto.Id)
            {
                _logger.LogWarning("KasSetting id mismatch: {Id} vs {DtoId}", id, dto.ClassId);
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for KasSetting update");
                return BadRequest(ModelState);
            }

            var kasSetting = await _context.KasSettings.FindAsync(id);
            if (kasSetting == null)
            {
                _logger.LogWarning("KasSetting with id {Id} not found for update", id);
                return NotFound();
            }

            kasSetting.ClassId = dto.ClassId;
            kasSetting.AmountPerWeek = dto.AmountPerWeek;
            kasSetting.StartDate = dto.StartDate;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.KasSettings.Any(e => e.Id == id))
                {
                    _logger.LogWarning("KasSetting with id {Id} not found for update", id);
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating KasSetting");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.KasSettings.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("KasSetting with id {Id} not found for delete", id);
                return NotFound();
            }
            _context.KasSettings.Remove(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting KasSetting");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpPost("add-balance")]
        public async Task<IActionResult> AddBalance([FromBody] AddBalanceDto dto)
        {
            // Tambahkan pemasukan ke kas dengan membuat Payment baru (misal UserId = 0 untuk admin/manual)
            var payment = new Payment
            {
                UserId = 39, // atau id user admin, atau buat UserId khusus untuk pemasukan manual
                WeekNumber = 0,
                Amount = dto.Amount,
                ProofUrl = "Tes",
                Status = "Diterima",
                SubmittedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Saldo berhasil ditambahkan sebagai pemasukan kas.", payment });
        }
    }
}
