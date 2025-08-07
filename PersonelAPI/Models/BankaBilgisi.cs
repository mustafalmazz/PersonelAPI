namespace PersonelAPI.Models
{
    public class BankaBilgisi
    {
        public int Id { get; set; }
        public int PersonelId { get; set; }
        public string BankaAdi { get; set; }
        public string IBAN { get; set; }

        // Navigation
        public Personel Personel { get; set; }
    }
}
