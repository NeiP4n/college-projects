using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CodeLearn.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LoginRecord>          LoginRecords      { get; set; }
        public DbSet<Course>               Courses           { get; set; }
        public DbSet<LearningModule>       Modules           { get; set; }
        public DbSet<Lesson>               Lessons           { get; set; }
        public DbSet<ContentBlock>         ContentBlocks     { get; set; }
        public DbSet<UserLessonCompletion> LessonCompletions { get; set; }
        public DbSet<UserLessonProgress>   LessonProgresses  { get; set; }
        public DbSet<UserLessonError>      LessonErrors      { get; set; }
        public DbSet<LessonComment>        LessonComments    { get; set; }
        public DbSet<UserAiUsage>          AiUsages          { get; set; }
        public DbSet<MapObject>            MapObjects         { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Сохраняем прежние имена таблиц (без префикса AspNet)
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

            builder.Entity<Course>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            builder.Entity<Course>()
                .HasMany(c => c.Modules)
                .WithOne(m => m.Course)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            // SetNull: удаление модуля не удаляет уроки
            builder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            // SetNull: удаление урока не удаляет блоки контента
            builder.Entity<ContentBlock>()
                .HasOne(cb => cb.Lesson)
                .WithMany(l => l.ContentBlocks)
                .HasForeignKey(cb => cb.LessonId)
                .OnDelete(DeleteBehavior.SetNull);

            // Прохождения уроков — уникальная пара (пользователь, урок)
            builder.Entity<UserLessonCompletion>()
                .HasIndex(c => new { c.UserId, c.LessonId })
                .IsUnique();
            builder.Entity<UserLessonCompletion>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserLessonCompletion>()
                .HasOne(c => c.Lesson)
                .WithMany()
                .HasForeignKey(c => c.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLessonProgress>()
                .HasIndex(p => new { p.UserId, p.LessonId })
                .IsUnique();
            builder.Entity<UserLessonProgress>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserLessonProgress>()
                .HasOne(p => p.Lesson)
                .WithMany()
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // AI-лимит — уникальная пара (пользователь, дата)
            builder.Entity<UserAiUsage>()
                .HasIndex(u => new { u.UserId, u.Date })
                .IsUnique();
            builder.Entity<UserAiUsage>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Map Builder
            builder.Entity<MapObject>()
                .HasOne(o => o.Course)
                .WithMany()
                .HasForeignKey(o => o.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
