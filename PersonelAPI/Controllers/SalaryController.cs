using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;
using System.Security.Claims;

namespace PersonelAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class SalaryController : ControllerBase
    {
        private readonly EmployeeDbContext _context;

        public SalaryController(EmployeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<Salary>>> GetSalaries()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                return await _context.Salaries
                    .Include(s => s.ExtraPayments)
                    .Where(s => s.EmployeeId == userId)
                    .ToListAsync();
            }

            return await _context.Salaries
                .Include(s => s.Employee)
                .Include(s => s.ExtraPayments)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<Salary>> GetSalary(int id)
        {
            var salary = await _context.Salaries
                .Include(s => s.Employee)
                .Include(s => s.ExtraPayments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salary == null)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (salary.EmployeeId != userId)
                    return Forbid();
            }

            return salary;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Salary>> CreateSalary(Salary salary)
        {
            decimal extraTotal = salary.ExtraPayments.Sum(e => e.Amount);
            salary.NetSalary = salary.GrossSalary - salary.Deduction + extraTotal;

            _context.Salaries.Add(salary);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalary), new { id = salary.Id }, salary);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSalary(int id, Salary salary)
        {
            if (id != salary.Id)
                return BadRequest();

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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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
