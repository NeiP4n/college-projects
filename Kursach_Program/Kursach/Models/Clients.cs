using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;


namespace Kursach.Models
{
    public class Clients
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [IgnoreDataMember]
        public virtual ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
    }
}
