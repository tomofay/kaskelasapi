using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;
using Microsoft.Extensions.Logging;

namespace KasKelasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly KasKelasApiDbContext _context;
        private readonly ILogger<ClassController> _logger;
        public ClassController(KasKelasApiDbContext context, ILogger<ClassController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Class>>> GetAll() =>
            await _context.Classes.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Class>> Get(int id)
        {
            var item = await _context.Classes.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Class with id {Id} not found", id);
                return NotFound();
            }
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Class>> Create([FromBody] Class item)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Class create");
                return BadRequest(ModelState);
            }
            _context.Classes.Add(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Class");
                return StatusCode(500, "Internal server error");
            }
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Class item)
        {
            if (id != item.Id)
            {
                _logger.LogWarning("Class id mismatch: {Id} vs {ItemId}", id, item.Id);
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Class update");
                return BadRequest(ModelState);
            }
            _context.Entry(item).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Classes.Any(e => e.Id == id))
                {
                    _logger.LogWarning("Class with id {Id} not found for update", id);
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Class");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Classes.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Class with id {Id} not found for delete", id);
                return NotFound();
            }
            _context.Classes.Remove(item);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Class");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }
    }
}
