namespace PersonelAPI.Models
{
    public class Rapor
    {
        public int Id { get; set; }
        public int PersonelId { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public string Aciklama { get; set; }
        public Personel? Personel { get; set; }
    }
}
