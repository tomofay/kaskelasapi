using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;

namespace KasKelasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaldoController : ControllerBase
    {
        private readonly KasKelasApiDbContext _context;
        public SaldoController(KasKelasApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Saldo
        [HttpGet]
        public async Task<IActionResult> GetTotalSaldo()
        {
            // Total pemasukan (Payment yang statusnya diterima)
            var totalPemasukan = await _context.Payments
                .Where(p => p.Status == "Diterima")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Total pengeluaran
            var totalPengeluaran = await _context.Expenses
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var saldo = totalPemasukan - totalPengeluaran;

            return Ok(new
            {
                totalPemasukan,
                totalPengeluaran,
                saldo
            });
        }

       
    }
}
