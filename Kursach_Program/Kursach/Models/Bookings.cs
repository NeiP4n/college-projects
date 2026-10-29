using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace Kursach.Models
{
    public class Bookings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TourId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string Status { get; set; } = string.Empty;

        [IgnoreDataMember]
        public virtual Tours Tour { get; set; } = default!;

        public virtual Clients Client { get; set; } = default!;
    }

}