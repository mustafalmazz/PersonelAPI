using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelAPI.Models
{
    public class Salary
    {
        public int Id { get; set; }

        [Column("PersonelId")]
        public int EmployeeId { get; set; }

        [Column("NetMaas")]
        public decimal NetSalary { get; set; }

        [Column("BrutMaas")]
        public decimal GrossSalary { get; set; }
        [Column("Kesinti")]
        public decimal Deduction { get; set; }

        [Column("MaasTarihi")]
        public DateTime SalaryDate { get; set; }
        [Column("CalisilanGun")]
        public int WorkedDays { get; set; } = 0;
        [Column("UcretsizIzinGun")]
        public int UnpaidLeaveDays { get; set; } = 0;
        [Column("IsciBagkurPrimi")]
        public decimal EmployeeSGKPremium { get; set; }
        [Column("IsciIssizlikPrimi")]
        public decimal EmployeeUnemploymentPremium { get; set; }
        [Column("GelirVergisi")]
        public decimal IncomeTax { get; set; }
        [Column("IsverenBagkurPrimi")]
        public decimal EmployerSGKPremium { get; set; }

        [Column("IsverenIssizlikPrimi")]
        public decimal EmployerUnemploymentPremium { get; set; }

        [Column("ToplamIsverenMaliyet")]
        public decimal TotalEmployerCost { get; set; }

        // Teşvik bilgileri
        [Column("ImalatSektoru")]
        public bool IsManufacturing { get; set; } = false;

        [Column("Tesvik5510")]
        public bool HasIncentive5510 { get; set; } = false;

        [Column("TesvikOrani")]
        public decimal IncentiveRate { get; set; } = 0;
        public Employee? Employee { get; set; }

        public ICollection<ExtraPayment> ExtraPayments { get; set; } = new List<ExtraPayment>();
        [NotMapped]
        public decimal TotalExtraPayments => ExtraPayments?.Sum(ep => ep.Amount) ?? 0;
        [NotMapped]
        public decimal TotalGrossSalary => GrossSalary + TotalExtraPayments;

        [NotMapped]
        public decimal DailyWage => GrossSalary / 30;

        [NotMapped]
        public decimal TotalEmployeeDeductions => EmployeeSGKPremium + EmployeeUnemploymentPremium + IncomeTax;
    }
}
