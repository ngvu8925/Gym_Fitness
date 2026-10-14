using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Api.Models
{
    [Table("Accounts")]
    public class Account
    {
        [Column("AccountId")]
        public int AccountId { get; set; }

        [Column("BranchId")]
        public int? BranchId { get; set; }

        [Column("Username")]
        public string Username { get; set; } = string.Empty;

        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Column("Role")]
        public string Role { get; set; } = string.Empty;

        [Column("PhoneNumber")]
        public string? Phone { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("Status")]
        public bool Status { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
