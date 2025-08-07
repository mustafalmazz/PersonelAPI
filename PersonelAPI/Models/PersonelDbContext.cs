using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models; // Models klasöründeyse bu namespace doğru olmalı

public class PersonelDbContext : DbContext
{
    public PersonelDbContext(DbContextOptions<PersonelDbContext> options)
        : base(options)
    {
    }

    public DbSet<Personel> Personeller { get; set; }
    public DbSet<BankaBilgisi> BankaBilgileri { get; set; }
    public DbSet<YillikIzin> YillikIzinler { get; set; }
    public DbSet<KullanilanIzinGunu> KullanilanIzinGunleri { get; set; }
    public DbSet<UcretsizIzin> UcretsizIzinler { get; set; }
    public DbSet<Rapor> Raporlar { get; set; }
    public DbSet<Maas> Maaslar { get; set; }
    public DbSet<EkOdeme> EkOdemeler { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tablo isimleri ile tam eşleme
        modelBuilder.Entity<Personel>().ToTable("Personel");
        modelBuilder.Entity<BankaBilgisi>().ToTable("BankaBilgisi");
        modelBuilder.Entity<YillikIzin>().ToTable("YillikIzin");
        modelBuilder.Entity<KullanilanIzinGunu>().ToTable("KullanilanIzinGunleri");
        modelBuilder.Entity<UcretsizIzin>().ToTable("UcretsizIzin");
        modelBuilder.Entity<Rapor>().ToTable("Rapor");
        modelBuilder.Entity<Maas>().ToTable("Maas");
        modelBuilder.Entity<EkOdeme>().ToTable("EkOdeme");

        // 1'e 1 ilişki: Personel - BankaBilgisi
        modelBuilder.Entity<BankaBilgisi>()
            .HasOne(b => b.Personel)
            .WithOne(p => p.BankaBilgisi)
            .HasForeignKey<BankaBilgisi>(b => b.PersonelId);

        // 1'e çok ilişkiler

        modelBuilder.Entity<Personel>()
            .HasMany(p => p.YillikIzinler)
            .WithOne(y => y.Personel)
            .HasForeignKey(y => y.PersonelId);

        modelBuilder.Entity<YillikIzin>()
            .HasMany(y => y.KullanilanGunler)
            .WithOne(k => k.YillikIzin)
            .HasForeignKey(k => k.YillikIzinId);

        modelBuilder.Entity<Personel>()
            .HasMany(p => p.UcretsizIzinler)
            .WithOne(u => u.Personel)
            .HasForeignKey(u => u.PersonelId);

        modelBuilder.Entity<Personel>()
            .HasMany(p => p.Raporlar)
            .WithOne(r => r.Personel)
            .HasForeignKey(r => r.PersonelId);

        modelBuilder.Entity<Personel>()
            .HasMany(p => p.Maaslar)
            .WithOne(m => m.Personel)
            .HasForeignKey(m => m.PersonelId);

        modelBuilder.Entity<Maas>()
            .HasMany(m => m.EkOdemeler)
            .WithOne(e => e.Maas)
            .HasForeignKey(e => e.MaasId);
    }
}
