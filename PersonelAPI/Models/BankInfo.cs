using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelAPI.Models
{
    public class BankInfo
    {
        public int Id { get; set; }

        [Column("PersonelId")]
        public int EmployeeId { get; set; }

        [Column("BankaAdi")]
        public string BankName { get; set; }

        [Column("IBAN")]
        public string IBAN { get; set; }

        public Employee? Employee { get; set; }
    }
}
