using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using KasKelasApi.DTOs; // Tambahkan ini

namespace KasKelasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly KasKelasApiDbContext _context;
        private readonly ILogger<UserController> _logger;
        public UserController(KasKelasApiDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll() =>
            await _context.Users.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with id {Id} not found", id);
                return NotFound();
            }
            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for User create");
                return BadRequest(ModelState);
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                ClassId = dto.ClassId == 0 ? null : dto.ClassId, // 0 dianggap null
                Username = dto.Username,
                Password = dto.Password,
                // ParentId dan Class bisa null
            };

            if (dto.Parent != null)
            {
                user.ParentId = dto.Parent.Id;
            }

            // Jika ingin set Class dari dto.Class, bisa tambahkan di sini jika perlu

            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating User");
                return StatusCode(500, "Internal server error");
            }
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for User update");
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with id {Id} not found for update", id);
                return NotFound();
            }

            // Update only if not null (or not empty for string)
            if (dto.FullName != null)
                user.FullName = dto.FullName;
            if (dto.Email != null)
                user.Email = dto.Email;
            if (dto.Role != null)
                user.Role = dto.Role;
            if (dto.ClassId.HasValue)
                user.ClassId = dto.ClassId;
            if (dto.ParentId.HasValue)
                user.ParentId = dto.ParentId;
            if (dto.Username != null)
                user.Username = dto.Username;
            if (dto.Password != null)
                user.Password = dto.Password;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id))
                {
                    _logger.LogWarning("User with id {Id} not found for update", id);
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating User");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with id {Id} not found for delete", id);
                return NotFound();
            }

            // Cek apakah user masih dipakai di Expenses
            bool hasExpenses = await _context.Expenses.AnyAsync(e => e.CreatedBy == id);
            if (hasExpenses)
            {
                return BadRequest(new { message = "User tidak bisa dihapus karena masih digunakan di Expenses." });
            }

            _context.Users.Remove(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting User");
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Password == dto.Password);

            if (user == null)
                return Unauthorized(new { message = "Username atau password salah" });

            // Kembalikan seluruh data user (tanpa password)
            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Email,
                    user.ClassId,
                    user.Role,
                    user.ParentId,
                    user.Class, // jika ingin mengembalikan data Class
                    user.Parent // jika ingin mengembalikan data Parent 
                    // tambahkan properti lain sesuai model User, kecuali password
                }
            });
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<UserBulkDto> usersDto)
        {
            if (usersDto == null || usersDto.Count == 0)
                return BadRequest(new { message = "User list is empty." });

            var users = usersDto.Select(dto => new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                ClassId = dto.ClassId,
                Username = dto.Username,
                Password = dto.Password
                // Field lain jika ada
            }).ToList();

            _context.Users.AddRange(users);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating users in bulk");
                return StatusCode(500, "Internal server error");
            }
            return Ok(new { message = "Users created successfully", users });
        }

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Optional: verifikasi password lama
            if (!string.IsNullOrEmpty(dto.OldPassword) && user.Password != dto.OldPassword)
                return BadRequest(new { message = "Old password is incorrect" });

            user.Password = dto.NewPassword;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for User {Id}", id);
                return StatusCode(500, "Internal server error");
            }
            return Ok(new { message = "Password changed successfully" });
        }
    }
}
