namespace PersonelAPI.Models;

public class Personel
{
    public int Id { get; set; }
    public string Ad { get; set; }
    public string Soyad { get; set; }
    public string TCKN { get; set; }
    public DateTime DogumTarihi { get; set; }
    public string Gorev { get; set; }
    public DateTime IseGirisTarihi { get; set; }
    public DateTime? IstenAyrilisTarihi { get; set; }

    // Navigation
    public BankaBilgisi BankaBilgisi { get; set; }
    public ICollection<YillikIzin> YillikIzinler { get; set; }
    public ICollection<UcretsizIzin> UcretsizIzinler { get; set; }
    public ICollection<Rapor> Raporlar { get; set; }
    public ICollection<Maas> Maaslar { get; set; }
}
