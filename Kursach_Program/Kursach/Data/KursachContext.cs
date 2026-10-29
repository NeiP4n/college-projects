using Microsoft.EntityFrameworkCore;

namespace Kursach.Data
{
    public class KursachContext : DbContext
    {
        public KursachContext(DbContextOptions<KursachContext> options)
            : base(options)
        {
        }

        public DbSet<Kursach.Models.Tours> Tours { get; set; } = default!;
        public DbSet<Kursach.Models.Clients> Clients { get; set; } = default!;
        public DbSet<Kursach.Models.Bookings> Bookings { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kursach.Models.Tours>()
                .Property(t => t.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Kursach.Models.Bookings>()
                .HasOne(b => b.Tour) // Навигация к Tour
                .WithMany(t => t.Bookings) // С Tour может быть несколько Booking
                .HasForeignKey(b => b.TourId)
                .OnDelete(DeleteBehavior.Restrict); // Поведение при удалении

            modelBuilder.Entity<Kursach.Models.Bookings>()
                .HasOne(b => b.Client) // Навигация к Client
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // Поведение при удалении
        }


    }
}
