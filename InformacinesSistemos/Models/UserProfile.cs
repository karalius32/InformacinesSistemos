using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InformacinesSistemos.Data
{
    [Table("user_account")]
    public class UserProfile
    {
        // FK to AspNetUsers.Id (string)
        [Key]
        [Column("id")]
        public string UserId { get; set; } = null!;

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}