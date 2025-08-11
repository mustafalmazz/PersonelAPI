using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class BankInfoController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        public BankInfoController(EmployeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankInfo>>> GetBankInfos()
        {
            return await _context.BankInfos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BankInfo>> GetBankInfo(int id)
        {
            var bankInfo = await _context.BankInfos.FindAsync(id);
            if (bankInfo == null)
            {
                return NotFound();
            }
            return bankInfo;
        }

        [HttpPost]
        public async Task<ActionResult<BankInfo>> PostBankInfo(BankInfo bankInfo)
        {
            _context.BankInfos.Add(bankInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBankInfo), new { id = bankInfo.Id }, bankInfo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBankInfo(int id, BankInfo bankInfo)
        {
            if (id != bankInfo.Id)
            {
                return BadRequest();
            }

            _context.Entry(bankInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BankInfoExists(id))
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
        public async Task<IActionResult> DeleteBankInfo(int id)
        {
            var bankInfo = await _context.BankInfos.FindAsync(id);
            if (bankInfo == null)
            {
                return NotFound();
            }

            _context.BankInfos.Remove(bankInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BankInfoExists(int id)
        {
            return _context.BankInfos.Any(b => b.Id == id);
        }
    }
}
