using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;
using KasKelasApi.DTOs;

namespace KasKelasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly KasKelasApiDbContext _context;
        public ExpenseController(KasKelasApiDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ExpenseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var expense = new Expense
            {
                ClassId = dto.ClassId,
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                CreatedBy = dto.CreatedBy
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetById(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.Class)
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
                return NotFound();

            return expense;
        }

        // List semua expense
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetAll()
        {
            var expenses = await _context.Expenses
                .Include(e => e.Class)
                .Include(e => e.Creator)
                .ToListAsync();
            return Ok(expenses);
        }

        // Update expense
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExpenseCreateDto dto)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            expense.ClassId = dto.ClassId;
            expense.Amount = dto.Amount;
            expense.Description = dto.Description;
            expense.Date = dto.Date;
            expense.CreatedBy = dto.CreatedBy;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Delete expense
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
