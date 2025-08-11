using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

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
        public async Task<ActionResult<IEnumerable<UnpaidLeave>>> GetUnpaidLeaves()
        {
            var list = await _context.UnpaidLeaves.Include(p => p.Employee).ToListAsync();
            return list;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UnpaidLeave>> GetUnpaidLeave(int id)
        {
            var item = await _context.UnpaidLeaves.Include(b => b.Employee).FirstOrDefaultAsync(a => a.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return item;
        }

        [HttpPost]
        public async Task<ActionResult> PostUnpaidLeave(UnpaidLeave leave)
        {
            await _context.UnpaidLeaves.AddAsync(leave);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUnpaidLeave), new { id = leave.Id }, leave);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUnpaidLeave(int id, UnpaidLeave leave)
        {
            if (id != leave.Id)
            {
                return BadRequest();
            }
            _context.Entry(leave).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.UnpaidLeaves.Any(f => f.Id == id))
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
        public async Task<IActionResult> DeleteUnpaidLeave(int id)
        {
            var leave = await _context.UnpaidLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            _context.UnpaidLeaves.Remove(leave);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
