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
    public BankaBilgisi? BankaBilgisi { get; set; }
    public ICollection<YillikIzin> YillikIzinler { get; set; } = new List<YillikIzin>();
    public ICollection<UcretsizIzin> UcretsizIzinler { get; set; } = new List<UcretsizIzin>();
    public ICollection<Rapor> Raporlar { get; set; } = new List<Rapor>();
    public ICollection<Maas> Maaslar { get; set; } = new List<Maas>();
}
