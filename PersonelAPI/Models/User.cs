using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Column("KullaniciAdi")]
        public string Username { get; set; }

        [Column("SifreHash")]
        public byte[] PasswordHash { get; set; }

        [Column("Rol")]
        public string Role { get; set; }

        [Column("PersonelId")]
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }
    }
}
