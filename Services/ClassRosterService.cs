using Microsoft.EntityFrameworkCore;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Enums;
using PordznakanAPI.Models;

namespace PordznakanAPI.Services
{
    public enum RosterStatus
    {
        Ok,
        /// <summary>The identifier is neither a GUID nor a positive integer.</summary>
        InvalidId,
        NotFound
    }

    public sealed record RosterResult<T>(RosterStatus Status, T? Value, string? Message = null) where T : class
    {
        public static RosterResult<T> Ok(T value) => new(RosterStatus.Ok, value);
        public static RosterResult<T> InvalidId(string message) => new(RosterStatus.InvalidId, null, message);
        public static RosterResult<T> NotFound(string message) => new(RosterStatus.NotFound, null, message);
    }

    /// <summary>
    /// An entity identifier that is either our internal id (a GUID) or the KTAK/EMIS id (an integer).
    /// The two forms can never be confused, so one route parameter carries either.
    /// </summary>
    public readonly record struct EntityKey(Guid? InternalId, int? KtakId)
    {
        public static bool TryParse(string? raw, out EntityKey key)
        {
            key = default;
            var text = raw?.Trim();
            if (string.IsNullOrEmpty(text))
                return false;

            if (Guid.TryParse(text, out var guid) && guid != Guid.Empty)
            {
                key = new EntityKey(guid, null);
                return true;
            }

            if (int.TryParse(text, out var ktak) && ktak > 0)
            {
                key = new EntityKey(null, ktak);
                return true;
            }

            return false;
        }
    }

    public interface IClassRosterService
    {
        Task<RosterResult<TeacherClassesResponse>> GetTeacherClassesAsync(string id, CancellationToken ct = default);
        Task<RosterResult<TeacherSubjectsResponse>> GetTeacherSubjectsAsync(string id, CancellationToken ct = default);
        Task<RosterResult<ClassmatesResponse>> GetClassmatesAsync(string id, CancellationToken ct = default);
    }

    public class ClassRosterService : IClassRosterService
    {
        private readonly AppDbContext _context;

        public ClassRosterService(AppDbContext context)
        {
            _context = context;
        }

        // Lookups use the unique index on Teachers.KtakTeacherId / Pupils.KtakPupilId or the primary key.
        private Task<Teacher?> FindTeacherAsync(EntityKey key, CancellationToken ct) =>
            key.InternalId is { } guid
                ? _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == guid, ct)
                : _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.KtakTeacherId == key.KtakId, ct);

        private Task<Pupil?> FindPupilAsync(EntityKey key, CancellationToken ct) =>
            key.InternalId is { } guid
                ? _context.Pupils.AsNoTracking().FirstOrDefaultAsync(p => p.Id == guid, ct)
                : _context.Pupils.AsNoTracking().FirstOrDefaultAsync(p => p.KtakPupilId == key.KtakId, ct);

        public async Task<RosterResult<TeacherClassesResponse>> GetTeacherClassesAsync(string id, CancellationToken ct = default)
        {
            if (!EntityKey.TryParse(id, out var key))
                return RosterResult<TeacherClassesResponse>.InvalidId(InvalidIdMessage("teacher", id));

            var teacher = await FindTeacherAsync(key, ct);
            if (teacher is null)
                return RosterResult<TeacherClassesResponse>.NotFound($"Teacher '{id}' not found.");

            var subjects = await _context.TeacherSubjects.AsNoTracking()
                .Where(s => s.TeacherId == teacher.Id)
                .ToListAsync(ct);

            // A teacher's subject row names a grade and a letter (5ա), not a classroom. Classes are
            // therefore the classrooms of the teacher's school that carry that grade and letter. The
            // letter mapping is C# code, so the school's classrooms (a few dozen) are filtered in memory.
            var subjectsByClass = subjects
                .Where(s => s.Grade != 0 && s.SubGrade != ESubGrade.Unknown)
                .GroupBy(s => (s.Grade, s.SubGrade))
                .ToDictionary(g => g.Key, g => g.Select(s => s.Name).Where(n => n.Length > 0).Distinct().ToList());

            var classes = new List<TeacherClassDto>();

            if (subjectsByClass.Count > 0)
            {
                var schoolClassrooms = await _context.Classrooms.AsNoTracking()
                    .Where(c => c.KtakSchoolId == teacher.KtakSchoolId)
                    .ToListAsync(ct);

                var matched = schoolClassrooms
                    .Select(c => (Classroom: c, Key: (GradeMapper.MapGrade(c.Grade), GradeMapper.MapSubGrade(c.Classifier))))
                    .Where(x => subjectsByClass.ContainsKey(x.Key))
                    .OrderBy(x => x.Key.Item1).ThenBy(x => x.Key.Item2).ThenBy(x => x.Classroom.ClassName)
                    .ToList();

                var classIds = matched.Select(x => x.Classroom.Id).ToList();

                // One query for every pupil of every matched class (uses the ClassroomInternalId foreign key index).
                var pupils = classIds.Count == 0
                    ? new List<Pupil>()
                    : await _context.Pupils.AsNoTracking()
                        .Where(p => p.ClassroomInternalId != null && classIds.Contains(p.ClassroomInternalId.Value))
                        .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                        .ToListAsync(ct);

                var pupilsByClass = pupils.ToLookup(p => p.ClassroomInternalId!.Value);

                foreach (var (classroom, classKey) in matched)
                {
                    var dto = ToClassDto<TeacherClassDto>(classroom);
                    dto.Subjects = subjectsByClass[classKey];
                    dto.Pupils = pupilsByClass[classroom.Id].Select(ToPupilBrief).ToList();
                    dto.PupilCount = dto.Pupils.Count;
                    classes.Add(dto);
                }
            }

            return RosterResult<TeacherClassesResponse>.Ok(new TeacherClassesResponse
            {
                Teacher = ToTeacherBrief(teacher),
                Classes = classes
            });
        }

        public async Task<RosterResult<TeacherSubjectsResponse>> GetTeacherSubjectsAsync(string id, CancellationToken ct = default)
        {
            if (!EntityKey.TryParse(id, out var key))
                return RosterResult<TeacherSubjectsResponse>.InvalidId(InvalidIdMessage("teacher", id));

            var teacher = await FindTeacherAsync(key, ct);
            if (teacher is null)
                return RosterResult<TeacherSubjectsResponse>.NotFound($"Teacher '{id}' not found.");

            var rows = await _context.TeacherSubjects.AsNoTracking()
                .Where(s => s.TeacherId == teacher.Id)
                .ToListAsync(ct);

            var subjects = rows
                .GroupBy(s => s.SubjectId)
                .OrderBy(g => g.First().Name)
                .Select(g => new TeacherSubjectItemDto
                {
                    SubjectId = g.Key,
                    Name = g.Select(s => s.Name).FirstOrDefault(n => n.Length > 0) ?? string.Empty,
                    Grades = g.Select(s => (s.Grade, s.SubGrade))
                        .Distinct()
                        .OrderBy(x => x.Grade).ThenBy(x => x.SubGrade)
                        .Select(x => new SubjectGradeDto { Grade = x.Grade, SubGrade = x.SubGrade })
                        .ToList()
                })
                .ToList();

            return RosterResult<TeacherSubjectsResponse>.Ok(new TeacherSubjectsResponse
            {
                Teacher = ToTeacherBrief(teacher),
                Subjects = subjects
            });
        }

        public async Task<RosterResult<ClassmatesResponse>> GetClassmatesAsync(string id, CancellationToken ct = default)
        {
            if (!EntityKey.TryParse(id, out var key))
                return RosterResult<ClassmatesResponse>.InvalidId(InvalidIdMessage("pupil", id));

            var pupil = await FindPupilAsync(key, ct);
            if (pupil is null)
                return RosterResult<ClassmatesResponse>.NotFound($"Pupil '{id}' not found.");

            // The class is the pupil's classroom link; when the link is not set yet, fall back to the
            // EMIS classroom id inside the pupil's school (same pair the Pupils index covers).
            var classroom = pupil.ClassroomInternalId is { } classroomId
                ? await _context.Classrooms.AsNoTracking().FirstOrDefaultAsync(c => c.Id == classroomId, ct)
                : await _context.Classrooms.AsNoTracking().FirstOrDefaultAsync(
                    c => c.KtakSchoolId == pupil.KtakSchoolId && c.KtakClassroomId == pupil.ClassroomId, ct);

            var peersQuery = _context.Pupils.AsNoTracking().Where(p => p.Id != pupil.Id);
            peersQuery = pupil.ClassroomInternalId is { } internalClassId
                ? peersQuery.Where(p => p.ClassroomInternalId == internalClassId)
                : peersQuery.Where(p => p.KtakSchoolId == pupil.KtakSchoolId && p.ClassroomId == pupil.ClassroomId);

            var peers = await peersQuery
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .ToListAsync(ct);

            return RosterResult<ClassmatesResponse>.Ok(new ClassmatesResponse
            {
                Pupil = ToPupilBrief(pupil),
                Class = classroom is null ? null : ToClassDto<ClassBriefDto>(classroom),
                ClassmateCount = peers.Count,
                Classmates = peers.Select(ToPupilBrief).ToList()
            });
        }

        private static string InvalidIdMessage(string what, string? id) =>
            $"'{id}' is not a valid {what} identifier. Use the internal id (GUID) or the KTAK id (positive integer).";

        private static TeacherBriefDto ToTeacherBrief(Teacher t) => new()
        {
            Id = t.Id,
            KtakTeacherId = t.KtakTeacherId,
            KtakSchoolId = t.KtakSchoolId,
            RegionId = t.RegionId,
            FirstName = t.FirstName,
            LastName = t.LastName,
            FatherName = t.FatherName,
            MainSubject = t.MainSubject
        };

        private static PupilBriefDto ToPupilBrief(Pupil p) => new()
        {
            Id = p.Id,
            KtakPupilId = p.KtakPupilId,
            KtakSchoolId = p.KtakSchoolId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            FatherName = p.FatherName,
            Grade = p.Grade,
            SubGrade = p.SubGrade,
            Status = p.Status
        };

        private static T ToClassDto<T>(Classroom c) where T : ClassBriefDto, new() => new()
        {
            Id = c.Id,
            KtakClassroomId = c.KtakClassroomId,
            KtakSchoolId = c.KtakSchoolId,
            Grade = c.Grade,
            Classifier = c.Classifier,
            ClassName = c.ClassName,
            Stream = c.Stream
        };
    }
}
