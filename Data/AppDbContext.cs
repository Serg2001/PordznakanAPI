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
        public DbSet<MmuhStudent> MmuhStudents { get; set; }
        public DbSet<MmuhStudentStaging> MmuhStudentsStaging { get; set; }
        public DbSet<MmuhStaff> MmuhStaff { get; set; }
        public DbSet<MmuhStaffStaging> MmuhStaffStaging { get; set; }
        public DbSet<NmuhStudent> NmuhStudents { get; set; }
        public DbSet<NmuhStudentStaging> NmuhStudentsStaging { get; set; }
        public DbSet<NmuhStaff> NmuhStaff { get; set; }
        public DbSet<NmuhStaffStaging> NmuhStaffStaging { get; set; }
        public DbSet<LogEmployee> LogEmployees { get; set; }
        public DbSet<LogStudent> LogStudents { get; set; }
        public DbSet<LogMmuhEmployee> LogMmuhEmployees { get; set; }
        public DbSet<LogMmuhStudent> LogMmuhStudents { get; set; }
        public DbSet<LogNmuhStudent> LogNmuhStudents { get; set; }
        public DbSet<LogNmuhEmployee> LogNmuhEmployees { get; set; }


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

            // Configure MmuhStudent entity
            modelBuilder.Entity<MmuhStudent>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<MmuhStudent>()
                .HasIndex(m => m.MmuhStudentId);

            modelBuilder.Entity<MmuhStudent>()
                .HasIndex(m => m.MmuhSchoolId);

            // Configure MmuhStudent staging entity (same schema, different table)
            modelBuilder.Entity<MmuhStudentStaging>()
                .ToTable("MmuhStudentsStaging");

            modelBuilder.Entity<MmuhStudentStaging>()
                .HasKey(m => m.Id);

            // Configure MmuhStaff entity
            modelBuilder.Entity<MmuhStaff>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<MmuhStaff>()
                .HasIndex(m => m.MmuhStaffId);

            modelBuilder.Entity<MmuhStaff>()
                .HasIndex(m => m.InstId);

            // Configure MmuhStaff staging entity (same schema, different table)
            modelBuilder.Entity<MmuhStaffStaging>()
                .ToTable("MmuhStaffStaging");

            modelBuilder.Entity<MmuhStaffStaging>()
                .HasKey(m => m.Id);

            // Configure NmuhStudent entity
            modelBuilder.Entity<NmuhStudent>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<NmuhStudent>()
                .HasIndex(m => m.NmuhStudentId);

            modelBuilder.Entity<NmuhStudent>()
                .HasIndex(m => m.NmuhSchoolId);

            // Configure NmuhStudent staging entity (same schema, different table)
            modelBuilder.Entity<NmuhStudentStaging>()
                .ToTable("NmuhStudentsStaging");

            modelBuilder.Entity<NmuhStudentStaging>()
                .HasKey(m => m.Id);

            // Configure NmuhStaff entity
            modelBuilder.Entity<NmuhStaff>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<NmuhStaff>()
                .HasIndex(m => m.NmuhStaffId);

            modelBuilder.Entity<NmuhStaff>()
                .HasIndex(m => m.InstId);

            // Configure NmuhStaff staging entity (same schema, different table)
            modelBuilder.Entity<NmuhStaffStaging>()
                .ToTable("NmuhStaffStaging");

            modelBuilder.Entity<NmuhStaffStaging>()
                .HasKey(m => m.Id);

            // Configure LogEmployee entity
            modelBuilder.Entity<LogEmployee>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<LogEmployee>()
                .HasIndex(l => l.LogId);

            modelBuilder.Entity<LogEmployee>()
                .HasIndex(l => l.SchoolId);

            // Configure LogStudent entity
            modelBuilder.Entity<LogStudent>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<LogStudent>()
                .HasIndex(l => l.LogId);

            modelBuilder.Entity<LogStudent>()
                .HasIndex(l => l.SchoolId);

            // Configure LogMmuhEmployee entity
            modelBuilder.Entity<LogMmuhEmployee>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<LogMmuhEmployee>()
                .HasIndex(l => l.LogId);

            modelBuilder.Entity<LogMmuhEmployee>()
                .HasIndex(l => l.SchoolId);

            // Configure LogMmuhStudent entity
            modelBuilder.Entity<LogMmuhStudent>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<LogMmuhStudent>()
                .HasIndex(l => l.LogId);

            modelBuilder.Entity<LogMmuhStudent>()
                .HasIndex(l => l.SchoolId);

            // Configure LogNmuhStudent entity
            modelBuilder.Entity<LogNmuhStudent>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<LogNmuhStudent>()
                .HasIndex(l => l.LogId);

            modelBuilder.Entity<LogNmuhStudent>()
                .HasIndex(l => l.SchoolId);

            // Configure LogNmuhEmployee entity
            modelBuilder.Entity<LogNmuhEmployee>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<LogNmuhEmployee>()
                .HasIndex(l => l.LogId);

            modelBuilder.Entity<LogNmuhEmployee>()
                .HasIndex(l => l.SchoolId);
        }
    }
}