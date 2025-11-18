using Microsoft.EntityFrameworkCore;
using PordznakanAPI.Models;

namespace PordznakanAPI.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<School> Schools { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }

        public DbSet<Pupil> Pupils { get; set; }
        public DbSet<Employee> Employees { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<School>()
                .HasIndex(s => s.KtakId)
                .IsUnique();

            modelBuilder.Entity<Classroom>()
                .HasIndex(c => c.KtakId)
                .IsUnique();
        }
    }
}
