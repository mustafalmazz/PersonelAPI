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

        public Employee? Employee { get; set; }

        public ICollection<ExtraPayment> ExtraPayments { get; set; } = new List<ExtraPayment>();
    }
}
