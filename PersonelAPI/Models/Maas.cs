namespace PersonelAPI.Models
{
    public class Maas
    {
        public int Id { get; set; }
        public int PersonelId { get; set; }
        public decimal NetMaas { get; set; }
        public decimal BrutMaas { get; set; }
        public DateTime MaasTarihi { get; set; }

        public Personel Personel { get; set; }
        public ICollection<EkOdeme> EkOdemeler { get; set; }
    }
}
