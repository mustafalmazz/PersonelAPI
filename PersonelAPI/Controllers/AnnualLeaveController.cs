using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AnnualLeaveController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        public AnnualLeaveController(EmployeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<AnnualLeave>>> GetAnnualLeaves()
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userRole == "Employee" && int.TryParse(userIdClaim, out int userId))
            {
                return await _context.AnnualLeaves
                    .Include(al => al.UsedLeaveDays)
                    .Where(al => al.EmployeeId == userId)
                    .ToListAsync();
            }

            // Admin tüm kayıtları görebilir
            return await _context.AnnualLeaves
                .Include(al => al.UsedLeaveDays)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<AnnualLeave>> GetAnnualLeave(int id)
        {
            var annualLeave = await _context.AnnualLeaves
                .Include(al => al.UsedLeaveDays)
                .FirstOrDefaultAsync(al => al.Id == id);

            if (annualLeave == null)
            {
                return NotFound();
            }

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userRole == "Employee" && int.TryParse(userIdClaim, out int userId))
            {
                if (annualLeave.EmployeeId != userId)
                    return Forbid();
            }

            return annualLeave;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AnnualLeave>> PostAnnualLeave(AnnualLeave annualLeave)
        {
            _context.AnnualLeaves.Add(annualLeave);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnnualLeave), new { id = annualLeave.Id }, annualLeave);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAnnualLeave(int id, AnnualLeave annualLeave)
        {
            if (id != annualLeave.Id)
            {
                return BadRequest();
            }

            _context.Entry(annualLeave).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnnualLeaveExists(id))
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAnnualLeave(int id)
        {
            var annualLeave = await _context.AnnualLeaves.FindAsync(id);

            if (annualLeave == null)
            {
                return NotFound();
            }

            _context.AnnualLeaves.Remove(annualLeave);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AnnualLeaveExists(int id)
        {
            return _context.AnnualLeaves.Any(al => al.Id == id);
        }
    }
}
