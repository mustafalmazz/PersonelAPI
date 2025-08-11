using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

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
        public async Task<ActionResult<IEnumerable<UsedLeaveDay>>> GetUsedLeaveDays()
        {
            return await _context.UsedLeaveDays.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsedLeaveDay>> GetUsedLeaveDay(int id)
        {
            var leaveDay = await _context.UsedLeaveDays.FindAsync(id);
            if (leaveDay == null)
            {
                return NotFound();
            }
            return leaveDay;
        }

        [HttpPost]
        public async Task<ActionResult<UsedLeaveDay>> PostUsedLeaveDay(UsedLeaveDay leaveDay)
        {
            await _context.UsedLeaveDays.AddAsync(leaveDay);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsedLeaveDay), new { id = leaveDay.Id }, leaveDay);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsedLeaveDay(int id, UsedLeaveDay leaveDay)
        {
            if (id != leaveDay.Id)
            {
                return BadRequest();
            }
            _context.Entry(leaveDay).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.UsedLeaveDays.Any(x => x.Id == id))
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
        public async Task<IActionResult> DeleteUsedLeaveDay(int id)
        {
            var toDelete = await _context.UsedLeaveDays.FindAsync(id);
            if (toDelete == null)
            {
                return NotFound();
            }
            _context.Remove(toDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
