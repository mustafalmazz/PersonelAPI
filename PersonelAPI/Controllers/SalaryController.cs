using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models;
using System.Security.Claims;

namespace PersonelAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SalaryController : ControllerBase
    {
        private readonly EmployeeDbContext _context;

        public SalaryController(EmployeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<IEnumerable<Salary>>> GetSalaries()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                return await _context.Salaries
                    .Include(s => s.ExtraPayments)
                    .Include(s => s.Employee)
                    .Where(s => s.EmployeeId == userId)
                    .ToListAsync();
            }

            return await _context.Salaries
                .Include(s => s.Employee)
                .Include(s => s.ExtraPayments)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<Salary>> GetSalary(int id)
        {
            var salary = await _context.Salaries
                .Include(s => s.Employee)
                .Include(s => s.ExtraPayments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salary == null)
                return NotFound();

            if (User.IsInRole("Employee"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (salary.EmployeeId != userId)
                    return Forbid();
            }

            return salary;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Salary>> CreateSalary(SalaryCreateRequest request)
        {
            var employee = await _context.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
                return BadRequest("Çalışan bulunamadı.");

            var salary = new Salary
            {
                EmployeeId = request.EmployeeId,
                GrossSalary = request.GrossSalary,
                SalaryDate = request.SalaryDate,
                WorkedDays = request.WorkedDays,
                UnpaidLeaveDays = request.UnpaidLeaveDays,
                IsManufacturing = request.IsManufacturing,
                HasIncentive5510 = request.HasIncentive5510,
                IncentiveRate = request.IncentiveRate,
                ExtraPayments = request.ExtraPayments?.Select(ep => new ExtraPayment
                {
                    Amount = ep.Amount,
                    Description = ep.Description
                }).ToList() ?? new List<ExtraPayment>()
            };

            // Maaş hesaplama
            CalculateSalary(salary, employee);

            _context.Salaries.Add(salary);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalary), new { id = salary.Id }, salary);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSalary(int id, SalaryUpdateRequest request)
        {
            if (id != request.Id)
                return BadRequest();

            var salary = await _context.Salaries
                .Include(s => s.ExtraPayments)
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salary == null)
                return NotFound();

            // Mevcut extra ödemeleri temizle
            _context.ExtraPayments.RemoveRange(salary.ExtraPayments);

            // Güncelleme
            salary.GrossSalary = request.GrossSalary;
            salary.SalaryDate = request.SalaryDate;
            salary.WorkedDays = request.WorkedDays;
            salary.UnpaidLeaveDays = request.UnpaidLeaveDays;
            salary.IsManufacturing = request.IsManufacturing;
            salary.HasIncentive5510 = request.HasIncentive5510;
            salary.IncentiveRate = request.IncentiveRate;

            salary.ExtraPayments = request.ExtraPayments?.Select(ep => new ExtraPayment
            {
                SalaryId = salary.Id,
                Amount = ep.Amount,
                Description = ep.Description
            }).ToList() ?? new List<ExtraPayment>();

            // Yeniden hesapla
            CalculateSalary(salary, salary.Employee);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Salaries.Any(s => s.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSalary(int id)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary == null)
                return NotFound();

            _context.Salaries.Remove(salary);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("calculate")]
        [Authorize(Roles = "Admin")]
        public IActionResult CalculateSalaryPreview(SalaryCalculationRequest request)
        {
            var result = CalculateSalaryDetailed(
                request.GrossSalary,
                request.WorkedDays,
                request.UnpaidLeaveDays,
                request.IsManufacturing,
                request.HasIncentive5510,
                request.IncentiveRate,
                request.ExtraPayments,
                request.EmployeeAge
            );

            return Ok(result);
        }

        private void CalculateSalary(Salary salary, Employee employee)
        {
            var result = CalculateSalaryDetailed(
                salary.GrossSalary,
                salary.WorkedDays,
                salary.UnpaidLeaveDays,
                salary.IsManufacturing,
                salary.HasIncentive5510,
                salary.IncentiveRate,
                salary.ExtraPayments.Select(ep => ep.Amount).ToList(),
                CalculateAge(employee.BirthDate)
            );

            salary.NetSalary = result.NetSalary;
            salary.EmployeeSGKPremium = result.EmployeeSGKPremium;
            salary.EmployeeUnemploymentPremium = result.EmployeeUnemploymentPremium;
            salary.IncomeTax = result.IncomeTax;
            salary.EmployerSGKPremium = result.EmployerSGKPremium;
            salary.EmployerUnemploymentPremium = result.EmployerUnemploymentPremium;
            salary.TotalEmployerCost = result.TotalEmployerCost;
        }

        private SalaryCalculationResult CalculateSalaryDetailed(
            decimal grossSalary,
            int workedDays,
            int unpaidLeaveDays,
            bool isManufacturing,
            bool hasIncentive5510,
            decimal incentiveRate,
            ICollection<decimal> extraPayments,
            int employeeAge)
        {
            const decimal MINIMUM_WAGE_GROSS = 26005.50m;
            const decimal MINIMUM_WAGE_NET = 22104.67m;
            const decimal EMPLOYEE_SGK_RATE = 0.14m;
            const decimal EMPLOYEE_UNEMPLOYMENT_RATE = 0.01m;
            const decimal EMPLOYER_SGK_RATE = 0.2075m;
            const decimal EMPLOYER_UNEMPLOYMENT_RATE = 0.0225m;
            const decimal INCOME_TAX_RATE = 0.15m;
            const decimal DAILY_FOOD_ALLOWANCE_LIMIT = 108m;

            var result = new SalaryCalculationResult();

            // Günlük brüt ücret hesabı (30 gün üzerinden)
            decimal dailyGrossWage = grossSalary / 30;

            // Çalışılan gün sayısına göre brüt ücret hesabı
            decimal actualGrossSalary = dailyGrossWage * workedDays;

            // Ek ödemeler toplamı
            decimal totalExtraPayments = extraPayments?.Sum() ?? 0;

            // Toplam brüt ücret (maaş + ek ödemeler)
            decimal totalGrossSalary = actualGrossSalary + totalExtraPayments;

            // İşçi SGK primi
            result.EmployeeSGKPremium = totalGrossSalary * EMPLOYEE_SGK_RATE;

            // İşçi işsizlik primi
            result.EmployeeUnemploymentPremium = totalGrossSalary * EMPLOYEE_UNEMPLOYMENT_RATE;

            // Toplam işçi kesintileri
            decimal totalEmployeeDeductions = result.EmployeeSGKPremium + result.EmployeeUnemploymentPremium;

            // Gelir vergisi hesabı
            decimal taxableAmount = totalGrossSalary - totalEmployeeDeductions;

            // Asgari ücret istisnası kontrolü
            if (taxableAmount > MINIMUM_WAGE_NET)
            {
                result.IncomeTax = (taxableAmount - MINIMUM_WAGE_NET) * INCOME_TAX_RATE;
            }
            else
            {
                result.IncomeTax = 0;
            }

            // Net maaş
            result.NetSalary = totalGrossSalary - totalEmployeeDeductions - result.IncomeTax;

            // İşveren primleri
            decimal employerSGKRate = EMPLOYER_SGK_RATE;
            decimal employerUnemploymentRate = EMPLOYER_UNEMPLOYMENT_RATE;

            // 5510 teşvik kontrolü
            if (hasIncentive5510)
            {
                if (isManufacturing)
                {
                    // İmalat sektörü %5 indirim
                    employerSGKRate -= 0.05m;
                }
                else
                {
                    // Diğer sektörler %4 indirim
                    employerSGKRate -= 0.04m;
                }
            }

            // Özel teşvik oranı varsa
            if (incentiveRate > 0)
            {
                employerSGKRate = Math.Max(0, employerSGKRate - (incentiveRate / 100));
            }

            result.EmployerSGKPremium = totalGrossSalary * employerSGKRate;
            result.EmployerUnemploymentPremium = totalGrossSalary * employerUnemploymentRate;

            // Toplam işveren maliyeti
            result.TotalEmployerCost = totalGrossSalary + result.EmployerSGKPremium + result.EmployerUnemploymentPremium;

            // Detaylar
            result.GrossSalary = actualGrossSalary;
            result.ExtraPaymentsTotal = totalExtraPayments;
            result.TotalGrossSalary = totalGrossSalary;
            result.WorkedDays = workedDays;
            result.UnpaidLeaveDays = unpaidLeaveDays;
            result.DailyWage = dailyGrossWage;

            return result;
        }

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    // Request/Response modelleri
    public class SalaryCreateRequest
    {
        public int EmployeeId { get; set; }
        public decimal GrossSalary { get; set; }
        public DateTime SalaryDate { get; set; }
        public int WorkedDays { get; set; } = 30;
        public int UnpaidLeaveDays { get; set; } = 0;
        public bool IsManufacturing { get; set; } = false;
        public bool HasIncentive5510 { get; set; } = false;
        public decimal IncentiveRate { get; set; } = 0;
        public List<ExtraPaymentRequest>? ExtraPayments { get; set; }
    }

    public class SalaryUpdateRequest : SalaryCreateRequest
    {
        public int Id { get; set; }
    }

    public class ExtraPaymentRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class SalaryCalculationRequest
    {
        public decimal GrossSalary { get; set; }
        public int WorkedDays { get; set; } = 30;
        public int UnpaidLeaveDays { get; set; } = 0;
        public bool IsManufacturing { get; set; } = false;
        public bool HasIncentive5510 { get; set; } = false;
        public decimal IncentiveRate { get; set; } = 0;
        public List<decimal> ExtraPayments { get; set; } = new List<decimal>();
        public int EmployeeAge { get; set; }
    }

    public class SalaryCalculationResult
    {
        public decimal GrossSalary { get; set; }
        public decimal ExtraPaymentsTotal { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal EmployeeSGKPremium { get; set; }
        public decimal EmployeeUnemploymentPremium { get; set; }
        public decimal IncomeTax { get; set; }
        public decimal NetSalary { get; set; }
        public decimal EmployerSGKPremium { get; set; }
        public decimal EmployerUnemploymentPremium { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public int WorkedDays { get; set; }
        public int UnpaidLeaveDays { get; set; }
        public decimal DailyWage { get; set; }
    }
}