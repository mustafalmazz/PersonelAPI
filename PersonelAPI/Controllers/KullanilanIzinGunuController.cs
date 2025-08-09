using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;

namespace PersonelAPI.Controllers
{
    public class KullanilanIzinGunuController : ControllerBase
    {
        private readonly PersonelDbContext _context;
        public KullanilanIzinGunuController(PersonelDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KullanilanIzinGunu>>> GetKullanilanIzinGunleri()
        {
            return await _context.KullanilanIzinGunleri.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<KullanilanIzinGunu>> GetKullanilanIzinGunu(int id)
        {
            var izin = await _context.KullanilanIzinGunleri.FindAsync(id);
            if (izin == null)
            {
                return NotFound();
            }
            return izin;
        }
    }
}
