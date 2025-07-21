using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;
using Microsoft.Extensions.Logging;

namespace KasKelasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityLogController : ControllerBase
    {
        private readonly KasKelasApiDbContext _context;
        private readonly ILogger<ActivityLogController> _logger;
        public ActivityLogController(KasKelasApiDbContext context, ILogger<ActivityLogController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> GetAll() =>
            await _context.ActivityLogs.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityLog>> Get(int id)
        {
            var item = await _context.ActivityLogs.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("ActivityLog with id {Id} not found", id);
                return NotFound();
            }
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<ActivityLog>> Create([FromBody] ActivityLog item)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for ActivityLog create");
                return BadRequest(ModelState);
            }
            _context.ActivityLogs.Add(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ActivityLog");
                return StatusCode(500, "Internal server error");
            }
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActivityLog item)
        {
            if (id != item.Id)
            {
                _logger.LogWarning("ActivityLog id mismatch: {Id} vs {ItemId}", id, item.Id);
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for ActivityLog update");
                return BadRequest(ModelState);
            }
            _context.Entry(item).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ActivityLogs.Any(e => e.Id == id))
                {
                    _logger.LogWarning("ActivityLog with id {Id} not found for update", id);
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ActivityLog");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ActivityLogs.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("ActivityLog with id {Id} not found for delete", id);
                return NotFound();
            }
            _context.ActivityLogs.Remove(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ActivityLog");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }
    }
}
