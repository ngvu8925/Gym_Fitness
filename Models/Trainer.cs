using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Api.Models
{
    [Table("Trainers")]
    public class Trainer
    {
        [Column("TrainerId")]
        public int TrainerId { get; set; }

        [Column("BranchId")]
        public int? BranchId { get; set; }

        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Column("Specialty")]
        public string? Specialty { get; set; }

        [Column("PhoneNumber")]
        public string? Phone { get; set; }

        [Column("HireDate")]
        public DateTime? HireDate { get; set; }

        [Column("Status")]
        public bool Status { get; set; }
    }
}
