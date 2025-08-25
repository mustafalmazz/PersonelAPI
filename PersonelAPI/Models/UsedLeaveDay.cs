using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelAPI.Models
{
    public class UsedLeaveDay
    {
        public int Id { get; set; }

        [Column("YillikIzinId")]
        public int AnnualLeaveId { get; set; }

        [Column("BaslangicTarihi")]
        public DateTime StartDate { get; set; }

        [Column("BitisTarihi")]
        public DateTime EndDate { get; set; }

        [Column("KullanılanGun")]
        public int Days { get; set; }

        [Column("Aciklama")]
        public string Description { get; set; } = string.Empty;

        [Column("OlusturmaTarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public AnnualLeave? AnnualLeave { get; set; }

        // Hesaplanan özellik - toplam süre
        [NotMapped]
        public int TotalCalendarDays => (EndDate - StartDate).Days + 1;
    }
}
