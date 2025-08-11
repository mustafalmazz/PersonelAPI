using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelAPI.Models
{
    public class ExtraPayment
    {
        public int Id { get; set; }

        [Column("MaasId")]
        public int SalaryId { get; set; }

        [Column("Tutar")]
        public decimal Amount { get; set; }

        [Column("Aciklama")]
        public string Description { get; set; }

        public Salary? Salary { get; set; }
    }
}
