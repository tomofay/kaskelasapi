using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;
using Microsoft.Extensions.Logging;

namespace KasKelasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly KasKelasApiDbContext _context;
        private readonly ILogger<NotificationController> _logger;
        public NotificationController(KasKelasApiDbContext context, ILogger<NotificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetAll() =>
            await _context.Notifications.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> Get(int id)
        {
            var item = await _context.Notifications.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Notification with id {Id} not found", id);
                return NotFound();
            }
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Notification>> Create([FromBody] Notification item)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Notification create");
                return BadRequest(ModelState);
            }
            _context.Notifications.Add(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Notification");
                return StatusCode(500, "Internal server error");
            }
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Notification item)
        {
            if (id != item.Id)
            {
                _logger.LogWarning("Notification id mismatch: {Id} vs {ItemId}", id, item.Id);
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Notification update");
                return BadRequest(ModelState);
            }
            _context.Entry(item).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Notifications.Any(e => e.Id == id))
                {
                    _logger.LogWarning("Notification with id {Id} not found for update", id);
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Notification");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Notifications.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Notification with id {Id} not found for delete", id);
                return NotFound();
            }
            _context.Notifications.Remove(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Notification");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }
    }
}
   