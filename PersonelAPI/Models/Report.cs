using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelAPI.Models
{
    public class Report
    {
        public int Id { get; set; }

        [Column("PersonelId")]
        public int EmployeeId { get; set; }

        [Column("BaslangicTarihi")]
        public DateTime StartDate { get; set; }

        [Column("BitisTarihi")]
        public DateTime EndDate { get; set; }

        [Column("Aciklama")]
        public string Description { get; set; }

        public Employee? Employee { get; set; }
    }
}
