using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelAPI.Models
{
    public class AnnualLeave
    {
        public int Id { get; set; }

        [Column("PersonelId")]
        public int EmployeeId { get; set; }

        [Column("HakedilenGun")]
        public int EntitledDays { get; set; }

        [Column("KullanimYili")]
        public int UsageYear { get; set; }
        [Column("OlusturmaTarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column("GuncellenmeTarihi")]
        public DateTime? UpdatedDate { get; set; }

        [Column("Aktif")]
        public bool IsActive { get; set; } = true;
        public Employee? Employee { get; set; }

        public ICollection<UsedLeaveDay> UsedLeaveDays { get; set; } = new List<UsedLeaveDay>();
        [NotMapped]
        public int UsedDays => UsedLeaveDays?.Sum(u => u.Days) ?? 0;

        [NotMapped]
        public int RemainingDays => EntitledDays - UsedDays;

        [NotMapped]
        public decimal UsagePercentage => EntitledDays > 0 ? (decimal)UsedDays / EntitledDays * 100 : 0;

        [NotMapped]
        public bool IsExpired => UsageYear < DateTime.Now.Year;

        [NotMapped]
        public bool CanUseLeave => IsActive && !IsExpired && RemainingDays > 0;
    }
}
