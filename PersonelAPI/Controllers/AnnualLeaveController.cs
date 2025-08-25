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
                    .Include(al => al.Employee)
                    .Where(al => al.EmployeeId == userId)
                    .ToListAsync();
            }

            return await _context.AnnualLeaves
                .Include(al => al.UsedLeaveDays)
                .Include(al => al.Employee)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<AnnualLeave>> GetAnnualLeave(int id)
        {
            var annualLeave = await _context.AnnualLeaves
                .Include(al => al.UsedLeaveDays)
                .Include(al => al.Employee)
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
        public async Task<ActionResult<AnnualLeave>> PostAnnualLeave(AnnualLeaveCreateRequest request)
        {
            var employee = await _context.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
                return BadRequest("Çalışan bulunamadı.");

            var entitledDays = CalculateAnnualLeaveEntitlement(employee, request.UsageYear);

            var annualLeave = new AnnualLeave
            {
                EmployeeId = request.EmployeeId,
                EntitledDays = entitledDays,
                UsageYear = request.UsageYear
            };

            _context.AnnualLeaves.Add(annualLeave);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnnualLeave), new { id = annualLeave.Id }, annualLeave);
        }

        [HttpPost("use-leave")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UseAnnualLeave(UseAnnualLeaveRequest request)
        {
            var annualLeave = await _context.AnnualLeaves
                .Include(al => al.UsedLeaveDays)
                .Include(al => al.Employee)
                .FirstOrDefaultAsync(al => al.Id == request.AnnualLeaveId);

            if (annualLeave == null)
                return NotFound("Yıllık izin kaydı bulunamadı.");

            var leaveDays = CalculateLeaveDays(request.StartDate, request.EndDate);
            var remainingDays = annualLeave.EntitledDays - annualLeave.UsedLeaveDays.Sum(u => u.Days);

            if (leaveDays > remainingDays)
                return BadRequest($"Yetersiz yıllık izin hakkı. Kalan gün: {remainingDays}, Talep edilen: {leaveDays}");

            // İzin günlerini kaydet
            var usedLeaveDay = new UsedLeaveDay
            {
                AnnualLeaveId = request.AnnualLeaveId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Days = leaveDays,
                Description = request.Description
            };

            _context.UsedLeaveDays.Add(usedLeaveDay);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yıllık izin kaydedildi.", usedDays = leaveDays, remainingDays = remainingDays - leaveDays });
        }

        [HttpGet("calculate-entitlement/{employeeId}/{year}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> CalculateEntitlement(int employeeId, int year)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return NotFound("Çalışan bulunamadı.");

            var entitledDays = CalculateAnnualLeaveEntitlement(employee, year);
            return Ok(entitledDays);
        }

        [HttpGet("calculate-leave-days")]
        [Authorize(Roles = "Admin")]
        public ActionResult<int> CalculateLeaveDaysPreview(DateTime startDate, DateTime endDate)
        {
            var leaveDays = CalculateLeaveDays(startDate, endDate);
            return Ok(leaveDays);
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

        private int CalculateAnnualLeaveEntitlement(Employee employee, int year)
        {
            var age = CalculateAge(employee.BirthDate, new DateTime(year, 1, 1));
            var workYears = CalculateWorkYears(employee.StartDate, new DateTime(year, 12, 31));

            // 18 yaş altı ve 50 yaş üstü için minimum 20 gün
            if (age < 18 || age >= 50)
            {
                return Math.Max(20, GetBasicEntitlement(workYears));
            }

            return GetBasicEntitlement(workYears);
        }

        private int GetBasicEntitlement(double workYears)
        {
            if (workYears < 1)
                return 0; // Bir yılı doldurmamış çalışanın yıllık izin hakkı yok

            if (workYears >= 1 && workYears < 5)
                return 14;
            else if (workYears >= 5 && workYears < 10)
                return 20;
            else
                return 26; // 10 yıl ve üzeri için 26 gün
        }

        private int CalculateAge(DateTime birthDate, DateTime referenceDate)
        {
            var age = referenceDate.Year - birthDate.Year;
            if (birthDate.Date > referenceDate.AddYears(-age)) age--;
            return age;
        }

        private double CalculateWorkYears(DateTime startDate, DateTime referenceDate)
        {
            if (referenceDate < startDate)
                return 0;

            var totalDays = (referenceDate - startDate).TotalDays;
            return totalDays / 365.25; // Artık yılları da hesaba katarak
        }

        private int CalculateLeaveDays(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return 0;

            int leaveDays = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                // Pazar günleri ve resmi tatiller yıllık izinden sayılmaz
                if (currentDate.DayOfWeek != DayOfWeek.Sunday && !IsPublicHoliday(currentDate))
                {
                    leaveDays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return leaveDays;
        }

        private bool IsPublicHoliday(DateTime date)
        {
            // Türkiye resmi tatilleri - temel liste
            var publicHolidays = new List<DateTime>
            {
                // Sabit tatiller
                new DateTime(date.Year, 1, 1),   // Yılbaşı
                new DateTime(date.Year, 4, 23),  // Ulusal Egemenlik ve Çocuk Bayramı
                new DateTime(date.Year, 5, 1),   // İşçi Bayramı
                new DateTime(date.Year, 5, 19),  // Gençlik ve Spor Bayramı
                new DateTime(date.Year, 7, 15),  // Demokrasi ve Milli Birlik Günü
                new DateTime(date.Year, 8, 30),  // Zafer Bayramı
                new DateTime(date.Year, 10, 29), // Cumhuriyet Bayramı
            };

            // Dini bayramlar (değişken) - Bu kısım için ayrı bir servis veya kütüphane kullanılabilir
            // Şimdilik basit bir kontrol yapıyoruz, gerçek uygulamada Hijri takvim hesaplaması gerekir

            return publicHolidays.Any(holiday => holiday.Date == date.Date);
        }
    }

    // Request modelleri
    public class AnnualLeaveCreateRequest
    {
        public int EmployeeId { get; set; }
        public int UsageYear { get; set; }
    }

    public class UseAnnualLeaveRequest
    {
        public int AnnualLeaveId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}