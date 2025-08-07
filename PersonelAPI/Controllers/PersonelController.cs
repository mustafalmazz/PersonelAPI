using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonelController : ControllerBase
    {
        private readonly PersonelDbContext _context;
        public PersonelController(PersonelDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Personel>>> GetPersoneller()
        {
            return await _context.Personeller
         .Include(p => p.BankaBilgisi)
         .Include(p => p.YillikIzinler)
         .ThenInclude(y => y.KullanilanGunler)
         .Include(p => p.UcretsizIzinler)
         .Include(p => p.Raporlar)
         .Include(p => p.Maaslar)
         .ThenInclude(m => m.EkOdemeler)
         .ToListAsync();    
        }

        [HttpGet("{id}")] //idye göre 1 personel bulmayı sağlar 
        public async Task<ActionResult<Personel>> GetPersonel(int id)
        {
            var personel = await _context.Personeller
            .Include(p => p.BankaBilgisi)
            .Include(p => p.YillikIzinler)
            .ThenInclude(y => y.KullanilanGunler)
            .Include(p => p.UcretsizIzinler)
            .Include(p => p.Raporlar)
            .Include(p => p.Maaslar)
            .ThenInclude(m => m.EkOdemeler)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (personel == null)
            {
                return NotFound();
            }
            return personel;
        }

        [HttpPost]//personel ekleme 
        public async Task<ActionResult<Personel>> PostPersonel(Personel personel)
        {
            _context.Personeller.Add(personel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPersonel), new { id = personel.Id }, personel);
        }
        private bool PersonelExists(int id)
        {
            return _context.Personeller.Any(a => a.Id == id);
        }
        [HttpPut("{id}")] //personel bilgisi güncelleme 
        public async Task<IActionResult> PutPersonel(int id, Personel personel)
        {
            if (id != personel.Id)
            {
                return BadRequest();
            }
            _context.Entry(personel).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonelExists(id))
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
        public async Task<IActionResult> DeletePersonel(int id)
        {
            var personel = await _context.Personeller.FindAsync(id);
            if (personel == null)
            {
                return NotFound();
            }

            _context.Personeller.Remove(personel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("kontrol")]
        public IActionResult Kontrol()
        {
            return Ok("API çalışıyor.");
        }


    }
}
