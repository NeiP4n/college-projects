using Microsoft.EntityFrameworkCore;

namespace TourAgents.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Тур> Туры { get; set; }
        public DbSet<Клиент> Клиенты { get; set; }
        public DbSet<Бронирование> Бронирования { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}