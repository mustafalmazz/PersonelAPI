using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    public class PersonelController : Controller
    {
        private readonly PersonelDbContext _context;
        public PersonelController(PersonelDbContext context)
        {
            _context = context;
        }
        public async Task<ActionResult<IEnumerable<Personel>>> GetPersoneller()
        {
            return await _context.Personeller.ToListAsync();
        }

        [HttpGet("{id}")] //idye göre 1 personel bulmayı sağlar 
        public async Task<ActionResult<Personel>> GetPersonel(int id)
        {
            var personel = await _context.Personeller.FindAsync(id);
            if (personel == null)
            {
                return NotFound();
            }
            return personel;
        }

        [HttpPost]
        public async Task<ActionResult<Personel>> PostPersonel(Personel personel)
        {
            _context.Personeller.Add(personel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPersonel), new { id = personel.Id }, personel);
        }
        private bool PersonelExists(int id)
        {
            return _context.Personeller.Any(a=>a.Id == id);
        }
        [HttpPut("{id}")] //personel bilgisi güncelleme 
        public async Task<IActionResult> PutPersonel(int id, Personel personel)
        {
            if(id != personel.Id)
            {
                return BadRequest();
            }
            _context.Entry(personel).State=EntityState.Modified;
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

    }
}
