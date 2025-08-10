namespace PersonelAPI.Models
{
    public class KullanilanIzinGunu
    {
        public int Id { get; set; }
        public int YillikIzinId { get; set; }
        public DateTime Tarih { get; set; }

        public YillikIzin? YillikIzin { get; set; }
    }
}
