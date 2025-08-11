namespace PersonelAPI.Models
{
    public class Kullanici
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; }
        public byte[] SifreHash { get; set; }
        public string Rol { get; set; }
        public int PersonelId { get; set; }
        public Personel? Personel { get; set; }
    }
}
