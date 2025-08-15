using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelAPI.Models;

public class Employee
{
    public int Id { get; set; }

    [Column("Ad")]
    public string FirstName { get; set; }

    [Column("Soyad")]
    public string LastName { get; set; }

    [Column("TCKN")]
    public string NationalId { get; set; }

    [Column("DogumTarihi")]
    public DateTime BirthDate { get; set; }

    [Column("Gorev")]
    public string Position { get; set; }
    public bool IsActive { get; set; } = true;


    [Column("IseGirisTarihi")]
    public DateTime StartDate { get; set; }

    [Column("IstenAyrilisTarihi")]
    public DateTime? EndDate { get; set; }

    public BankInfo? BankInfo { get; set; }

    public ICollection<AnnualLeave> AnnualLeaves { get; set; } = new List<AnnualLeave>();

    public ICollection<UnpaidLeave> UnpaidLeaves { get; set; } = new List<UnpaidLeave>();

    public ICollection<Report> Reports { get; set; } = new List<Report>();

    public ICollection<Salary> Salaries { get; set; } = new List<Salary>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    public User? User { get; set; }
}
