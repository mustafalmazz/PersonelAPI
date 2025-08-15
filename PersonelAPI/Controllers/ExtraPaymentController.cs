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
    public class ExtraPaymentController : ControllerBase
    {
        private readonly EmployeeDbContext _context;

        public ExtraPaymentController(EmployeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<ExtraPayment>>> GetExtraPayments()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var payments = await _context.ExtraPayments
                    .Include(ep => ep.Salary)
                    .Where(ep => ep.Salary.EmployeeId == userId)
                    .ToListAsync();
                return payments;
            }
            return await _context.ExtraPayments
                .Include(ep => ep.Salary)
                .ToListAsync();
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<ExtraPayment>> GetExtraPayment(int id)
        {
            var payment = await _context.ExtraPayments
                .Include(ep => ep.Salary)
                .FirstOrDefaultAsync(ep => ep.Id == id);

            if (payment == null)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (payment.Salary.EmployeeId != userId)
                    return Forbid();
            }

            return payment;
        }

     
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExtraPayment>> PostExtraPayment(ExtraPayment extraPayment)
        {
            _context.ExtraPayments.Add(extraPayment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetExtraPayment), new { id = extraPayment.Id }, extraPayment);
        }

     
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutExtraPayment(int id, ExtraPayment extraPayment)
        {
            if (id != extraPayment.Id)
                return BadRequest();

            _context.Entry(extraPayment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ExtraPayments.Any(ep => ep.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteExtraPayment(int id)
        {
            var payment = await _context.ExtraPayments.FindAsync(id);
            if (payment == null)
                return NotFound();

            _context.ExtraPayments.Remove(payment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
