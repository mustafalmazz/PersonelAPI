using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YillikIzinController : ControllerBase
    {
        private readonly PersonelDbContext _context;
        public YillikIzinController(PersonelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<YillikIzin>>> GetYillikIzinler()
        {
            return await _context.YillikIzinler.
                Include(y=>y.KullanilanGunler).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<YillikIzin>> GetYillikIzin(int id)
        {
            var y = await _context.YillikIzinler.Include(a=>a.KullanilanGunler).FirstOrDefaultAsync(x=>x .Id == id);
            if (y == null)
            {
                return BadRequest();
            }
            return y;
        }

        [HttpPost]
        public async Task<ActionResult<YillikIzin>> PostYillikIzin(YillikIzin yillikIzin)
        {
            _context.YillikIzinler.Add(yillikIzin);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetYillikIzin), new { id = yillikIzin.Id }, yillikIzin);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutYillikIzin(int id , YillikIzin izin)
        {
            if(id != izin.Id)
            {
                return BadRequest();
            }
            _context.Entry(izin).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if (!YillikIzinExists(id))
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
        private bool YillikIzinExists(int id)
        {
            return _context.YillikIzinler.Any(x => x.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteYillikIzin(int id)
        {
            var silinecek = await _context.YillikIzinler.FindAsync(id);
            if(silinecek == null)
            {
                return NotFound();
            }
            _context.YillikIzinler.Remove(silinecek);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
