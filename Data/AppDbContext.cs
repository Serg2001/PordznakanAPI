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
        public DbSet<PupilStaging> PupilsStaging { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<TeacherStaging> TeachersStaging { get; set; }
        public DbSet<TeacherSubject> TeacherSubjects { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure School entity
            modelBuilder.Entity<School>()
                .HasKey(s => s.DshhSchoolId);  // Set DshhSchoolId as primary key

            modelBuilder.Entity<School>()
                .HasIndex(s => s.KtakSchoolId);
                //.IsUnique();  // Index for querying by external API ID

            modelBuilder.Entity<School>()
                .HasIndex(s => s.RegionId);  // Index for querying by region

            // Configure Classroom entity
            modelBuilder.Entity<Classroom>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Classroom>()
                .HasIndex(c => new { c.KtakSchoolId, c.KtakClassroomId });
                //.IsUnique();  // Unique index for external API ID combination

            modelBuilder.Entity<Classroom>()
                .HasOne(c => c.School)
                .WithMany(s => s.Classrooms)
                .HasForeignKey(c => c.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Pupil entity
            modelBuilder.Entity<Pupil>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Pupil>()
                .HasIndex(p => new { p.KtakSchoolId, p.ClassroomId, p.Place });
                //.IsUnique();  // Composite index for external API identification

            modelBuilder.Entity<Pupil>()
                .HasIndex(p => p.KtakPupilId);  // Index for querying by external pupil ID

            modelBuilder.Entity<Pupil>()
                .HasOne(p => p.Classroom)
                .WithMany(c => c.Pupils)
                .HasForeignKey(p => p.ClassroomInternalId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Pupil staging entity (same schema, different table)
            modelBuilder.Entity<PupilStaging>()
                .ToTable("PupilsStaging");

            modelBuilder.Entity<PupilStaging>()
                .HasKey(p => p.Id);

            // Configure Teacher entity
            modelBuilder.Entity<Teacher>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.KtakTeacherId)
                .IsUnique();

            // Configure Teacher staging entity
            modelBuilder.Entity<TeacherStaging>()
                .ToTable("TeachersStaging");

            modelBuilder.Entity<TeacherStaging>()
                .HasKey(t => t.Id);

            // Configure TeacherSubject entity
            modelBuilder.Entity<TeacherSubject>()
                .HasKey(ts => ts.Id);

            modelBuilder.Entity<TeacherSubject>()
                .HasOne(ts => ts.Teacher)
                .WithMany(t => t.Subjects)
                .HasForeignKey(ts => ts.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}