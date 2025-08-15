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
    public class UnpaidLeaveController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        public UnpaidLeaveController(EmployeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<UnpaidLeave>>> GetUnpaidLeaves()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                return await _context.UnpaidLeaves
                    .Include(p => p.Employee)
                    .Where(l => l.EmployeeId == userId)
                    .ToListAsync();
            }

            return await _context.UnpaidLeaves.Include(p => p.Employee).ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<UnpaidLeave>> GetUnpaidLeave(int id)
        {
            var leave = await _context.UnpaidLeaves.Include(p => p.Employee).FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (leave.EmployeeId != userId)
                    return Forbid();
            }

            return leave;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UnpaidLeave>> PostUnpaidLeave(UnpaidLeave leave)
        {
            await _context.UnpaidLeaves.AddAsync(leave);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUnpaidLeave), new { id = leave.Id }, leave);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUnpaidLeave(int id, UnpaidLeave leave)
        {
            if (id != leave.Id)
                return BadRequest();

            _context.Entry(leave).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.UnpaidLeaves.Any(f => f.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUnpaidLeave(int id)
        {
            var leave = await _context.UnpaidLeaves.FindAsync(id);
            if (leave == null)
                return NotFound();

            _context.UnpaidLeaves.Remove(leave);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
