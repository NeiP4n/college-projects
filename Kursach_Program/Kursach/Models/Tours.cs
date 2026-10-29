using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Kursach.Models
{
    public class Tours
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Destination { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть положительным числом.")]
        public decimal Price { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Количество доступных мест должно быть положительным числом.")]
        public int AvailableSeats { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
    }

}