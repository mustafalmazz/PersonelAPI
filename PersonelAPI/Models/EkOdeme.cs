namespace PersonelAPI.Models
{
    public class EkOdeme
    {
        public int Id { get; set; }
        public int MaasId { get; set; }
        public decimal Tutar { get; set; }
        public string Aciklama { get; set; }

        public Maas Maas { get; set; }
    }
}
