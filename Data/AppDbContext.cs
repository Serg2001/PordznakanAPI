using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
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
        public DbSet<MuhStudent> MuhStudents { get; set; }
        public DbSet<MuhStudentStaging> MuhStudentsStaging { get; set; }
        public DbSet<MmuhStaff> MmuhStaff { get; set; }
        public DbSet<MmuhStaffStaging> MmuhStaffStaging { get; set; }
        public DbSet<NmuhStaff> NmuhStaff { get; set; }
        public DbSet<NmuhStaffStaging> NmuhStaffStaging { get; set; }
        public DbSet<NmuhStaffGroup> NmuhStaffGroups { get; set; }
        public DbSet<NmuhSubject> NmuhSubjects { get; set; }
        public DbSet<MmuhStaffGroup> MmuhStaffGroups { get; set; }
        public DbSet<MmuhSubject> MmuhSubjects { get; set; }
        public DbSet<SchoolEmployee> SchoolEmployees { get; set; }
        public DbSet<MuhInstitution> MuhInstitutions { get; set; }
        public DbSet<MuhInstitutionStaging> MuhInstitutionsStaging { get; set; }


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
                .HasIndex(p => p.KtakPupilId)   // Index for querying by external pupil ID
                .IsUnique();                    // One row per EMIS pupil; apply only after duplicates are cleaned up

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

            // Configure MuhStudent entity
            modelBuilder.Entity<MuhStudent>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<MuhStudent>()
                .HasIndex(m => m.MuhStudentId);

            modelBuilder.Entity<MuhStudent>()
                .HasIndex(m => m.InstitutionId);

            modelBuilder.Entity<MuhStudent>()
                .Property(m => m.AnnualFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MuhStudentStaging>()
                .Property(m => m.AnnualFee)
                .HasPrecision(18, 2);

            // Configure MuhStudent staging entity (same schema, different table)
            modelBuilder.Entity<MuhStudentStaging>()
                .ToTable("MuhStudentsStaging");

            modelBuilder.Entity<MuhStudentStaging>()
                .HasKey(m => m.Id);

            // Configure MmuhStaff entity
            modelBuilder.Entity<MmuhStaff>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<MmuhStaff>()
                .HasIndex(m => m.MmuhStaffId);

            modelBuilder.Entity<MmuhStaff>()
                .HasIndex(m => m.InstId);

            // Store List<int> GroupIds as a JSON string column for MmuhStaff
            var mmuhGroupIdsComparer = new ValueComparer<List<int>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (acc, id) => HashCode.Combine(acc, id.GetHashCode())),
                v => v.ToList());

            modelBuilder.Entity<MmuhStaff>()
                .Property(m => m.GroupIds)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => string.IsNullOrEmpty(v)
                        ? new List<int>()
                        : JsonConvert.DeserializeObject<List<int>>(v)!)
                .Metadata.SetValueComparer(mmuhGroupIdsComparer);

            // Configure MmuhStaffGroup entity
            modelBuilder.Entity<MmuhStaffGroup>()
                .HasKey(g => g.Id);

            modelBuilder.Entity<MmuhStaffGroup>()
                .HasIndex(g => g.MmuhStaffId);

            modelBuilder.Entity<MmuhStaffGroup>()
                .HasOne(g => g.MmuhStaff)
                .WithMany(s => s.Groups)
                .HasForeignKey(g => g.MmuhStaffId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure MmuhSubject entity
            modelBuilder.Entity<MmuhSubject>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<MmuhSubject>()
                .HasOne(s => s.MmuhStaffGroup)
                .WithMany(g => g.Subjects)
                .HasForeignKey(s => s.MmuhStaffGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure MmuhStaff staging entity (same schema, different table)
            modelBuilder.Entity<MmuhStaffStaging>()
                .ToTable("MmuhStaffStaging");

            modelBuilder.Entity<MmuhStaffStaging>()
                .HasKey(m => m.Id);

            // Configure NmuhStaff entity
            modelBuilder.Entity<NmuhStaff>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<NmuhStaff>()
                .HasIndex(m => m.NmuhStaffId);

            modelBuilder.Entity<NmuhStaff>()
                .HasIndex(m => m.InstId);

            // Store List<int> GroupIds as a JSON string column
            var groupIdsComparer = new ValueComparer<List<int>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (acc, id) => HashCode.Combine(acc, id.GetHashCode())),
                v => v.ToList());

            modelBuilder.Entity<NmuhStaff>()
                .Property(m => m.GroupIds)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => string.IsNullOrEmpty(v)
                        ? new List<int>()
                        : JsonConvert.DeserializeObject<List<int>>(v)!)
                .Metadata.SetValueComparer(groupIdsComparer);

            // Configure NmuhStaffGroup entity
            modelBuilder.Entity<NmuhStaffGroup>()
                .HasKey(g => g.Id);

            modelBuilder.Entity<NmuhStaffGroup>()
                .HasIndex(g => g.NmuhStaffId);

            modelBuilder.Entity<NmuhStaffGroup>()
                .HasOne(g => g.NmuhStaff)
                .WithMany(s => s.Groups)
                .HasForeignKey(g => g.NmuhStaffId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure NmuhSubject entity
            modelBuilder.Entity<NmuhSubject>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<NmuhSubject>()
                .HasOne(s => s.NmuhStaffGroup)
                .WithMany(g => g.Subjects)
                .HasForeignKey(s => s.NmuhStaffGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure NmuhStaff staging entity (same schema, different table)
            modelBuilder.Entity<NmuhStaffStaging>()
                .ToTable("NmuhStaffStaging");

            modelBuilder.Entity<NmuhStaffStaging>()
                .HasKey(m => m.Id);

            // Configure SchoolEmployee entity
            modelBuilder.Entity<SchoolEmployee>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<SchoolEmployee>()
                .HasIndex(e => e.PersonId);

            modelBuilder.Entity<SchoolEmployee>()
                .HasIndex(e => e.SchoolId);

            modelBuilder.Entity<SchoolEmployee>()
                .HasIndex(e => e.RegionId);

            // Configure MuhInstitution entity
            modelBuilder.Entity<MuhInstitution>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<MuhInstitution>()
                .HasIndex(m => m.InstitutionId);

            modelBuilder.Entity<MuhInstitution>()
                .HasIndex(m => m.RegionId);

            // Configure MuhInstitution staging entity (same schema, different table)
            modelBuilder.Entity<MuhInstitutionStaging>()
                .ToTable("MuhInstitutionsStaging");

            modelBuilder.Entity<MuhInstitutionStaging>()
                .HasKey(m => m.Id);
        }
    }
}