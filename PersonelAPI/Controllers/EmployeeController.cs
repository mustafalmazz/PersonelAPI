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
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        public EmployeeController(EmployeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var employee = await _context.Employees
                    .Include(p => p.BankInfo)
                    .Include(p => p.AnnualLeaves).ThenInclude(y => y.UsedLeaveDays)
                    .Include(p => p.UnpaidLeaves)
                    .Include(p => p.Reports)
                    .Include(p => p.Salaries).ThenInclude(m => m.ExtraPayments)
                    .Where(p => p.Id == userId && p.IsActive) 
                    .ToListAsync();
                return employee;
            }
            return await _context.Employees
                .Include(p => p.BankInfo)
                .Include(p => p.AnnualLeaves).ThenInclude(y => y.UsedLeaveDays)
                .Include(p => p.UnpaidLeaves)
                .Include(p => p.Reports)
                .Include(p => p.Salaries).ThenInclude(m => m.ExtraPayments)
                .Where(p => p.IsActive) // filtre eklendi
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(p => p.BankInfo)
                .Include(p => p.AnnualLeaves).ThenInclude(y => y.UsedLeaveDays)
                .Include(p => p.UnpaidLeaves)
                .Include(p => p.Reports)
                .Include(p => p.Salaries).ThenInclude(m => m.ExtraPayments)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (employee == null)
                return NotFound();
            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (employee.Id != userId)
                    return Forbid();
            }

            return employee;
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
                return BadRequest();

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Employees.Any(a => a.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("check")]
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Check()
        {
            return Ok("API is running.");
        }
    }
}
