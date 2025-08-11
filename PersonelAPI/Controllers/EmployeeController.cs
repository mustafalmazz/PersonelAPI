using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

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
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _context.Employees
                .Include(p => p.BankInfo)
                .Include(p => p.AnnualLeaves)
                    .ThenInclude(y => y.UsedLeaveDays)
                .Include(p => p.UnpaidLeaves)
                .Include(p => p.Reports)
                .Include(p => p.Salaries)
                    .ThenInclude(m => m.ExtraPayments)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(p => p.BankInfo)
                .Include(p => p.AnnualLeaves)
                    .ThenInclude(y => y.UsedLeaveDays)
                .Include(p => p.UnpaidLeaves)
                .Include(p => p.Reports)
                .Include(p => p.Salaries)
                    .ThenInclude(m => m.ExtraPayments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (employee == null)
            {
                return NotFound();
            }
            return employee;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(a => a.Id == id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("check")]
        public IActionResult Check()
        {
            return Ok("API is running.");
        }
    }
}
