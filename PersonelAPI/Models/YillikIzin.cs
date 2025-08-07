namespace PersonelAPI.Models
{
    public class YillikIzin
    {
        public int Id { get; set; }
        public int PersonelId { get; set; }
        public int HakedilenGun { get; set; }
        public int KullanimYili { get; set; }

        public Personel? Personel { get; set; }
        public ICollection<KullanilanIzinGunu> KullanilanGunler { get; set; } = new List<KullanilanIzinGunu>();
    }
}
