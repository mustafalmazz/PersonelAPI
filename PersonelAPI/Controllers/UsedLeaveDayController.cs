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
    public class UsedLeaveDayController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        public UsedLeaveDayController(EmployeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<UsedLeaveDay>>> GetUsedLeaveDays()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                return await _context.UsedLeaveDays
                    .Include(u => u.AnnualLeave)
                        .ThenInclude(a => a.Employee)
                    .Where(u => u.AnnualLeave.EmployeeId == userId)
                    .ToListAsync();
            }

            // Admin tüm kayıtları görebilir
            return await _context.UsedLeaveDays
                .Include(u => u.AnnualLeave)
                    .ThenInclude(a => a.Employee)
                .ToListAsync();
        }

        // GET: api/usedleaveday/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<UsedLeaveDay>> GetUsedLeaveDay(int id)
        {
            var leaveDay = await _context.UsedLeaveDays
                .Include(u => u.AnnualLeave)
                    .ThenInclude(a => a.Employee)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (leaveDay == null)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (leaveDay.AnnualLeave.EmployeeId != userId)
                    return Forbid();
            }

            return leaveDay;
        }

        // POST: api/usedleaveday
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UsedLeaveDay>> PostUsedLeaveDay(UsedLeaveDay leaveDay)
        {
            await _context.UsedLeaveDays.AddAsync(leaveDay);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsedLeaveDay), new { id = leaveDay.Id }, leaveDay);
        }

        // PUT: api/usedleaveday/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUsedLeaveDay(int id, UsedLeaveDay leaveDay)
        {
            if (id != leaveDay.Id)
                return BadRequest();

            _context.Entry(leaveDay).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.UsedLeaveDays.Any(x => x.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/usedleaveday/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUsedLeaveDay(int id)
        {
            var toDelete = await _context.UsedLeaveDays.FindAsync(id);
            if (toDelete == null)
                return NotFound();

            _context.UsedLeaveDays.Remove(toDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
