using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaporController : ControllerBase
    {
        private readonly PersonelDbContext _context;
        public RaporController(PersonelDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rapor>>> GetRaporlar()
        {
            return await _context.Raporlar.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Rapor>> GetRapor(int id)
        {
            var rapor = _context.Raporlar.Find(id);
            if (rapor == null)
            {
                return NotFound();
            }
            return rapor;
        }
        [HttpPost]
        public async Task<ActionResult<Rapor>> PostRapor(Rapor rapor)
        {
            await _context.Raporlar.AddAsync(rapor);
            _context.SaveChanges();
            return CreatedAtAction("GetRapor", new { id = rapor.Id }, rapor);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRapor(int id,Rapor rapor)
        {
            if(id != rapor.Id)
            {
                return BadRequest();
            }
            _context.Entry(rapor).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!_context.Raporlar.Any(e=>e.Id == id))
                {
                    return  NotFound();
                }
                else throw;
            }
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRapor(int id)
        {
            var silinecek = _context.Raporlar.Find(id);
            if (silinecek == null)
            {
                return NotFound();
            }
            _context.Remove(silinecek);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
