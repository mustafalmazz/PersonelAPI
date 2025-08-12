using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Token ile erişim için
    public class SalaryController : ControllerBase
    {
        private readonly EmployeeDbContext _context;

        public SalaryController(EmployeeDbContext context)
        {
            _context = context;
        }

        // Maaşları listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Salary>>> GetSalaries()
        {
            return await _context.Salaries
                .Include(s => s.Employee)
                .Include(s => s.ExtraPayments)
                .ToListAsync();
        }

        // Belirli maaşı getir
        [HttpGet("{id}")]
        public async Task<ActionResult<Salary>> GetSalary(int id)
        {
            var salary = await _context.Salaries
                .Include(s => s.Employee)
                .Include(s => s.ExtraPayments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salary == null)
                return NotFound();

            return salary;
        }

        // Maaş ekle
        [HttpPost]
        public async Task<ActionResult<Salary>> CreateSalary(Salary salary)
        {
            // ExtraPayments toplamı
            decimal extraTotal = salary.ExtraPayments.Sum(e => e.Amount);

            // Net maaş formülü
            salary.NetSalary = salary.GrossSalary - salary.Deduction + extraTotal;

            _context.Salaries.Add(salary);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalary), new { id = salary.Id }, salary);
        }

        // Maaş güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSalary(int id, Salary salary)
        {
            if (id != salary.Id)
                return BadRequest();

            // ExtraPayments toplamını yeniden hesapla
            decimal extraTotal = salary.ExtraPayments.Sum(e => e.Amount);
            salary.NetSalary = salary.GrossSalary - salary.Deduction + extraTotal;

            _context.Entry(salary).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Salaries.Any(s => s.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // Maaş sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalary(int id)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary == null)
                return NotFound();

            _context.Salaries.Remove(salary);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
