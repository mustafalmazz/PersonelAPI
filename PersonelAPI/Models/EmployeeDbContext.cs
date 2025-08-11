using Microsoft.EntityFrameworkCore;
using PersonelAPI.Models; // Models klasöründeyse bu namespace doğru olmalı

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<BankInfo> BankInfos { get; set; }
    public DbSet<AnnualLeave> AnnualLeaves { get; set; }
    public DbSet<UsedLeaveDay> UsedLeaveDays { get; set; }
    public DbSet<UnpaidLeave> UnpaidLeaves { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Salary> Salaries { get; set; }
    public DbSet<ExtraPayment> ExtraPayments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tablo isimleri ile tam eşleme
        modelBuilder.Entity<Employee>().ToTable("Personel");
        modelBuilder.Entity<BankInfo>().ToTable("BankaBilgisi");
        modelBuilder.Entity<AnnualLeave>().ToTable("YillikIzin");
        modelBuilder.Entity<UsedLeaveDay>().ToTable("KullanilanIzinGunleri");
        modelBuilder.Entity<UnpaidLeave>().ToTable("UcretsizIzin");
        modelBuilder.Entity<Report>().ToTable("Rapor");
        modelBuilder.Entity<Salary>().ToTable("Maas");
        modelBuilder.Entity<ExtraPayment>().ToTable("EkOdeme");
        modelBuilder.Entity<User>().ToTable("Kullanici");
        modelBuilder.Entity<Address>().ToTable("Address");

        // 1'e 1 ilişki: Employee - BankInfo
        modelBuilder.Entity<BankInfo>()
            .HasOne(b => b.Employee)
            .WithOne(e => e.BankInfo)
            .HasForeignKey<BankInfo>(b => b.EmployeeId);

        // 1'e çok ilişkiler

        modelBuilder.Entity<Employee>()
            .HasMany(e => e.AnnualLeaves)
            .WithOne(a => a.Employee)
            .HasForeignKey(a => a.EmployeeId);

        modelBuilder.Entity<AnnualLeave>()
            .HasMany(a => a.UsedLeaveDays)
            .WithOne(u => u.AnnualLeave)
            .HasForeignKey(u => u.AnnualLeaveId);

        modelBuilder.Entity<Employee>()
            .HasMany(e => e.UnpaidLeaves)
            .WithOne(u => u.Employee)
            .HasForeignKey(u => u.EmployeeId);

        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Reports)
            .WithOne(r => r.Employee)
            .HasForeignKey(r => r.EmployeeId);

        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Salaries)
            .WithOne(s => s.Employee)
            .HasForeignKey(s => s.EmployeeId);

        modelBuilder.Entity<Salary>()
            .HasMany(s => s.ExtraPayments)
            .WithOne(e => e.Salary)
            .HasForeignKey(e => e.SalaryId);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<User>(u => u.EmployeeId);

        modelBuilder.Entity<Address>()
            .HasOne(a => a.Employee)
            .WithMany(p => p.Addresses)
            .HasForeignKey(a => a.EmployeeId);
    }
}
