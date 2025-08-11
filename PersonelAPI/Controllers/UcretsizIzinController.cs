using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UcretsizIzinController : ControllerBase
    {
        private readonly PersonelDbContext _context;
        public UcretsizIzinController(PersonelDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UcretsizIzin>>> GetUcretsizIzinler()
        {
            var liste = await _context.UcretsizIzinler.Include(p=>p.Personel).ToListAsync();
            return liste;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<UcretsizIzin>> GetUcretsizIzin(int id)
        {
            var bul = await _context.UcretsizIzinler.Include(b => b.Personel).FirstOrDefaultAsync(a=>a.Id == id);
            if (bul == null)
            {
                return NotFound();
            }
            return bul;
        }
        [HttpPost]
        public async Task<ActionResult> PostUcretsizIzin(UcretsizIzin izin)
        {
            await _context.UcretsizIzinler.AddAsync(izin);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetUcretsizIzin", new {id=izin.Id},izin);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUcretsizIzin(UcretsizIzin izin,int id)
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
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.UcretsizIzinler.Any(f => f.Id == id))
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
        public async Task<IActionResult> DeleteUcretsizIzin(int id)
        {
            var izin = await _context.UcretsizIzinler.FindAsync(id);
            if (izin == null)
            {
                return NotFound();
            }

            _context.UcretsizIzinler.Remove(izin);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
