using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Api.Models
{
    [Table("Classes")]
    public class GymClass
    {
        [Column("ClassId")]
        public int ClassId { get; set; }

        [Column("BranchId")]
        public int? BranchId { get; set; }

        [Column("ClassName")]
        public string Name { get; set; } = string.Empty;

        [Column("TrainerId")]
        public int? TrainerId { get; set; }

        [Column("Capacity")]
        public int Capacity { get; set; }

        [Column("Status")]
        public bool Status { get; set; }
    }
}
