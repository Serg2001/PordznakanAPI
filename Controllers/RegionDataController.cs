using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Enums;
using PordznakanAPI.Models;
using System.Net.Http.Json;

namespace PordznakanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionDataController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RegionDataController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// 1.1 – Returns each school's DshhSchoolId, KtakSchoolId, and Name for the given region. avelacnel hasce
        /// </summary>
        [HttpGet("schools/summary/by-region/{regionId}")]
        public async Task<IActionResult> GetSchoolSummaryByRegion([FromRoute] int regionId)
        {
            var schools = await _context.Schools
                .Where(s => s.RegionId == regionId)
                .Select(s => new SchoolSummaryDto
                {
                    DshhSchoolId = s.DshhSchoolId,
                    KtakSchoolId = s.KtakSchoolId,
                    Name = s.Name,
                    Email = s.Email,
                    Place = KtakPlace.School
                })
                .ToListAsync();

            return Ok(schools);
        }

        /// <summary>
        /// 1.2 – Returns all fields of every school that belongs to the given region.
        /// </summary>
        [HttpGet("schools/by-region/{regionId}")]
        public async Task<IActionResult> GetSchoolsByRegion([FromRoute] int regionId)
        {
            var schools = await _context.Schools
                .Where(s => s.RegionId == regionId)
                .Select(s => new SchoolDto
                {
                    DshhSchoolId = s.DshhSchoolId,
                    KtakSchoolId = s.KtakSchoolId,
                    RegionId = s.RegionId,
                    Name = s.Name,
                    Marz = s.Marz,
                    Region = s.Region,
                    Community = s.Community,
                    Email = s.Email,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return Ok(schools);
        }

        /// <summary>
        /// 1.3 – Returns all pupils that belong to the given region.
        /// </summary>
        [HttpGet("pupils/by-region/{regionId}")]
        public async Task<IActionResult> GetPupilsByRegion([FromRoute] int regionId)
        {
            var pupils = await _context.Pupils
                .Where(p => p.RegionId == regionId)
                .Select(p => new PupilDto
                {
                    Id = p.Id,
                    KtakPupilId = p.KtakPupilId,
                    KtakSchoolId = p.KtakSchoolId,
                    RegionId = p.RegionId,
                    ClassroomId = p.ClassroomId,
                    ClassroomInternalId = p.ClassroomInternalId,
                    Place = p.Place,
                    Grade = p.Grade,
                    SubGrade = p.SubGrade,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    FatherName = p.FatherName,
                    CertificateType = p.CertificateType,
                    Certificate = p.Certificate,
                    SocNumber = p.SocNumber,
                    Birthday = p.Birthday,
                    Gender = p.Gender,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(pupils);
        }

        /// <summary>
        /// 1.4 – Returns all pupils that belong to the given school (by KtakSchoolId).
        /// </summary>
        [HttpGet("pupils/by-school/{schoolId}")]
        public async Task<IActionResult> GetPupilsBySchool([FromRoute] int schoolId)
        {
            var pupils = await _context.Pupils
                .Where(p => p.KtakSchoolId == schoolId && p.Status == EPupilStatus.New)
                .Select(p => new PupilDto
                {
                    Id = p.Id,
                    KtakPupilId = p.KtakPupilId,
                    KtakSchoolId = p.KtakSchoolId,
                    RegionId = p.RegionId,
                    ClassroomId = p.ClassroomId,
                    ClassroomInternalId = p.ClassroomInternalId,
                    Place = p.Place,
                    Grade = p.Grade,
                    SubGrade = p.SubGrade,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    FatherName = p.FatherName,
                    CertificateType = p.CertificateType,
                    Certificate = p.Certificate,
                    SocNumber = p.SocNumber,
                    Birthday = p.Birthday,
                    Gender = p.Gender,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(pupils);
        }

        /// <summary>
        /// 1.5 – Returns only the internal Guid IDs of pupils for the given school (by KtakSchoolId).
        /// </summary>
        [HttpGet("pupils/ids/by-school/{schoolId}")]
        public async Task<IActionResult> GetPupilIdsBySchool([FromRoute] int schoolId)
        {
            var ids = await _context.Pupils
                .Where(p => p.KtakSchoolId == schoolId)
                .Select(p => p.Id)
                .ToListAsync();

            return Ok(ids);
        }

        // ── Teacher endpoints ────────────────────────────────────────────────

        /// <summary>
        /// T.1 – Returns all fields (including subjects) of every teacher that belongs to the given region.
        /// </summary>
        [HttpGet("teachers/by-region/{regionId}")]
        public async Task<IActionResult> GetTeachersByRegion([FromRoute] int regionId)
        {
            var teachers = await _context.Teachers
                .Include(t => t.Subjects)
                .Where(t => t.RegionId == regionId)
                .Select(t => new
                {
                    t.Id,
                    t.KtakTeacherId,
                    t.KtakSchoolId,
                    t.RegionId,
                    t.Place,
                    t.FirstName,
                    t.LastName,
                    t.FatherName,
                    t.Gender,
                    t.Birthday,
                    t.Phone,
                    t.Address,
                    t.Email,
                    t.SocNumber,
                    t.Experience,
                    t.AcademicRank,
                    t.Education,
                    t.CommandDate,
                    t.DigitLevel,
                    t.Activated,
                    t.WorkType,
                    t.CreatedAt,
                    t.UpdatedAt,
                    SchoolName = _context.Schools
                        .Where(sch => sch.KtakSchoolId == t.KtakSchoolId)
                        .Select(sch => sch.Name)
                        .FirstOrDefault(),
                    SubjectNames = t.Subjects.Select(s => s.Name).Distinct().ToList(),
                    Subjects = t.Subjects.Select(s => new
                    {
                        s.Id,
                        s.SubjectId,
                        s.Grade,
                        s.SubGrade,
                        s.Name
                    })
                })
                .ToListAsync();

            return Ok(teachers);
        }

        /// <summary>
        /// T.2 – Returns all fields (including subjects) of every teacher that belongs to the given school (by KtakSchoolId).
        /// </summary>
        [HttpGet("teachers/by-school/{schoolId}")]
        public async Task<IActionResult> GetTeachersBySchool([FromRoute] int schoolId)
        {
            var teachers = await _context.Teachers
                .Include(t => t.Subjects)
                .Where(t => t.KtakSchoolId == schoolId)
                .Select(t => new
                {
                    t.Id,
                    t.KtakTeacherId,
                    t.KtakSchoolId,
                    t.RegionId,
                    t.Place,
                    t.FirstName,
                    t.LastName,
                    t.FatherName,
                    t.Gender,
                    t.Birthday,
                    t.Phone,
                    t.Address,
                    t.Email,
                    t.SocNumber,
                    t.Experience,
                    t.AcademicRank,
                    t.Education,
                    t.CommandDate,
                    t.DigitLevel,
                    t.Activated,
                    t.WorkType,
                    t.CreatedAt,
                    t.UpdatedAt,
                    SchoolName = _context.Schools
                        .Where(sch => sch.KtakSchoolId == t.KtakSchoolId)
                        .Select(sch => sch.Name)
                        .FirstOrDefault(),
                    SubjectNames = t.Subjects.Select(s => s.Name).Distinct().ToList(),
                    Subjects = t.Subjects.Select(s => new
                    {
                        s.Id,
                        s.SubjectId,
                        s.Grade,
                        s.SubGrade,
                        s.Name
                    })
                })
                .ToListAsync();

            return Ok(teachers);
        }

        /// <summary>
        /// Returns school teachers with a flat list of subject names for the given school.
        /// </summary>
        [HttpGet("teachers/with-subjects/by-school/{schoolId}")]
        public async Task<IActionResult> GetTeachersWithSubjectsBySchool([FromRoute] int schoolId)
        {
            var schoolName = await _context.Schools
                .Where(s => s.KtakSchoolId == schoolId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();

            var teachers = await _context.Teachers
                .Include(t => t.Subjects)
                .Where(t => t.KtakSchoolId == schoolId)
                .ToListAsync();

            var result = teachers.Select(t => new
            {
                t.Id,
                t.KtakTeacherId,
                t.KtakSchoolId,
                t.FirstName,
                t.LastName,
                t.SocNumber,
                t.Phone,
                t.Address,
                t.DigitLevel,
                t.Place,
                SchoolName = schoolName,
                SubjectNames = t.Subjects.Select(s => s.Name).Distinct().ToList()
            });

            return Ok(result);
        }

        /// <summary>
        /// T.3 – Returns FirstName, LastName, SocNumber, and DigitLevel for every teacher in the given school.
        /// </summary>
        [HttpGet("teachers/summary/by-region/{regionId}")]
        public async Task<IActionResult> GetTeacherSummaryByRegion([FromRoute] int regionId)
        {
            var teachers = await _context.Teachers
                .Where(t => t.RegionId == regionId)
                .Select(t => new
                {
                    t.FirstName,
                    t.LastName,
                    t.SocNumber,
                    t.Phone,
                    t.Address,
                    t.DigitLevel,
                    SchoolName = _context.Schools
                        .Where(s => s.KtakSchoolId == t.KtakSchoolId)
                        .Select(s => s.Name)
                        .FirstOrDefault(),
                    SubjectNames = t.Subjects.Select(s => s.Name).Distinct().ToList()
                })
                .ToListAsync();

            return Ok(teachers);
        }

        /// <summary>
        /// T.4 – Returns FirstName, LastName, SocNumber, and DigitLevel for every teacher in the given school (by KtakSchoolId).
        /// </summary>
        [HttpGet("teachers/summary/by-school/{schoolId}")]
        public async Task<IActionResult> GetTeacherSummaryBySchool([FromRoute] int schoolId)
        {
            var schoolName = await _context.Schools
                .Where(s => s.KtakSchoolId == schoolId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();

            var teachers = await _context.Teachers
                .Where(t => t.KtakSchoolId == schoolId)
                .Select(t => new
                {
                    t.FirstName,
                    t.LastName,
                    t.SocNumber,
                    t.Phone,
                    t.Address,
                    t.DigitLevel,
                    SchoolName = schoolName,
                    SubjectNames = t.Subjects.Select(s => s.Name).Distinct().ToList()
                })
                .ToListAsync();

            return Ok(teachers);
        }

        /// <summary>
        /// T.5 – Returns FirstName, LastName, DigitLevel, Place and Subjects for a teacher
        /// identified by KtakSchoolId and SocNumber.
        /// </summary>
        [HttpGet("teachers/by-school/{schoolId}/{socNumber}")]
        public async Task<IActionResult> GetTeacherBySchoolAndSoc(
            [FromRoute] int schoolId,
            [FromRoute] string socNumber)
        {
            var teacherEntity = await _context.Teachers
                .Include(t => t.Subjects)
                .Where(t => t.KtakSchoolId == schoolId && t.SocNumber == socNumber)
                .FirstOrDefaultAsync();

            if (teacherEntity == null)
                return NotFound(new { message = $"No teacher found for schoolId={schoolId} and socNumber={socNumber}" });

            var schoolName = await _context.Schools
                .Where(s => s.KtakSchoolId == schoolId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();

            var teacher = new
            {
                teacherEntity.Id,
                teacherEntity.FirstName,
                teacherEntity.LastName,
                teacherEntity.KtakTeacherId,
                teacherEntity.DigitLevel,
                Phone = teacherEntity.Phone,
                Address = teacherEntity.Address,
                SchoolName = schoolName,
                Place = KtakPlace.School,
                SubjectNames = teacherEntity.Subjects.Select(s => s.Name).Distinct().ToList()
            };

            return Ok(teacher);
        }

        /// <summary>
        /// P.6 – Returns FirstName, LastName, Grade, SubGrade and Place for a pupil
        /// identified by KtakSchoolId and Certificate number (ident_document_number).
        /// </summary>
        [HttpGet("pupils/by-school/{schoolId}/{socNumber}")]
        public async Task<IActionResult> GetPupilBySchoolAndSoc(
            [FromRoute] int schoolId,
            [FromRoute] string socNumber)
        {
            var pupil = await _context.Pupils
                .Where(p => p.KtakSchoolId == schoolId && p.SocNumber == socNumber)
                .Select(p => new
                {
                    p.Id,
                    p.FirstName,
                    p.KtakPupilId,
                    p.LastName,
                    p.Grade,
                    p.SubGrade,
                    Place = KtakPlace.School
                })
                .FirstOrDefaultAsync();

            if (pupil == null)
                return NotFound(new { message = $"No pupil found for schoolId={schoolId} and socNumber={socNumber}" });

            return Ok(pupil);
        }

        // ── Classroom endpoints ──────────────────────────────────────────────

        /// <summary>
        /// Returns all classrooms for the given region.
        /// </summary>
        [HttpGet("classrooms/by-region/{regionId}")]
        public async Task<IActionResult> GetClassroomsByRegion([FromRoute] int regionId)
        {
            var classrooms = await _context.Classrooms
                .Where(c => c.RegionId == regionId)
                .Select(c => new
                {
                    c.Id,
                    c.KtakSchoolId,
                    c.KtakClassroomId,
                    c.RegionId,
                    c.Grade,
                    c.Classifier,
                    c.ClassName,
                    c.Stream,
                    c.SchoolId,
                    c.CreatedAt,
                    c.UpdatedAt
                })
                .ToListAsync();

            return Ok(classrooms);
        }

        // ── MmuhInstitution endpoints ────────────────────────────────────────

        /// <summary>
        /// Returns all MMUH institutions for the given region.
        /// </summary>
        [HttpGet("mmuh-institutions/by-region/{regionId}")]
        public async Task<IActionResult> GetMmuhInstitutionsByRegion([FromRoute] int regionId)
        {
            var institutions = await _context.MmuhInstitutions
                .Where(i => i.RegionId == regionId)
                .Select(i => new
                {
                    i.Id,
                    i.InstId,
                    i.RegionId,
                    i.Name,
                    i.LegalMarzId,
                    i.LegalAddress,
                    i.BusinessMarzId,
                    i.BusinessAddress,
                    i.Email,
                    i.CreatedAt,
                    i.UpdatedAt
                })
                .ToListAsync();

            return Ok(institutions);
        }

        /// <summary>
        /// Returns a summary (DshhSchoolId, KtakSchoolId, Name) of all MMUH institutions for the given region.
        /// </summary>
        [HttpGet("mmuh-institutions/summary/by-region/{regionId}")]
        public async Task<IActionResult> GetMmuhInstitutionsSummaryByRegion([FromRoute] int regionId)
        {
            var institutions = await _context.MmuhInstitutions
                .Where(i => i.RegionId == regionId)
                .Select(i => new MmuhInstitutionSummaryDto
                {
                    DshhSchoolId = i.Id,
                    KtakSchoolId = i.InstId,
                    Name = i.Name,
                    Email = i.Email,
                    Place = KtakPlace.Mmuh
                })
                .ToListAsync();

            return Ok(institutions);
        }

        // ── MmuhStudent endpoints ────────────────────────────────────────────

        /// <summary>
        /// Returns all MMUH students for the given region.
        /// </summary>
        [HttpGet("mmuh-students/by-region/{regionId}")]
        public async Task<IActionResult> GetMmuhStudentsByRegion([FromRoute] int regionId)
        {
            var students = await _context.MmuhStudents
                .Where(s => s.RegionId == regionId)
                .Select(s => new
                {
                    s.Id,
                    s.MmuhStudentId,
                    s.MmuhSchoolId,
                    s.RegionId,
                    s.SchoolName,
                    s.Marz,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Graduated,
                    s.GroupId,
                    s.ClassroomGrade,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(students);
        }

        /// <summary>
        /// Returns all MMUH students that belong to the given institution (by InstId).
        /// </summary>
        [HttpGet("mmuh-students/by-institution/{institutionId}")]
        public async Task<IActionResult> GetMmuhStudentsByInstitution([FromRoute] int institutionId)
        {
            var students = await _context.MmuhStudents
                .Where(s => s.MmuhSchoolId == institutionId)
                .Select(s => new
                {
                    s.Id,
                    s.MmuhStudentId,
                    s.MmuhSchoolId,
                    s.InternalSchoolId,
                    s.RegionId,
                    s.SchoolName,
                    s.Marz,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Graduated,
                    s.GroupId,
                    s.ClassroomGrade,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(students);
        }

        /// <summary>
        /// Returns a summary (Id, KtakPupilId, KtakSchoolId, FirstName, LastName, SocNumber, Grade)
        /// of all MMUH students that belong to the given institution (by InstId).
        /// </summary>
        [HttpGet("mmuh-students/summary/by-institution/{institutionId}")]
        public async Task<IActionResult> GetMmuhStudentsSummaryByInstitution([FromRoute] int institutionId)
        {
            var students = await _context.MmuhStudents
                .Where(s => s.MmuhSchoolId == institutionId)
                .Select(s => new MmuhStudentSummaryDto
                {
                    Id = s.Id,
                    KtakPupilId = s.MmuhStudentId,
                    KtakSchoolId = s.MmuhSchoolId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    SocNumber = s.SocNumber,
                    Grade = s.ClassroomGrade,
                    Place = KtakPlace.Mmuh
                })
                .ToListAsync();

            return Ok(students);
        }

        /// <summary>
        /// Returns FirstName, LastName, Grade and KtakSchoolId for an MMUH student
        /// identified by MmuhSchoolId and SocNumber.
        /// </summary>
        [HttpGet("mmuh-students/by-institution/{institutionId}/{socNumber}")]
        public async Task<IActionResult> GetMmuhStudentByInstitutionAndSoc(
            [FromRoute] int institutionId,
            [FromRoute] string socNumber)
        {
            var student = await _context.MmuhStudents
                .Where(s => s.MmuhSchoolId == institutionId && s.SocNumber == socNumber)
                .Select(s => new
                {
                    Id = s.Id,
                    KtakPupilId = s.MmuhStudentId,
                    KtakSchoolId = s.MmuhSchoolId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Grade = s.ClassroomGrade,
                    Place = KtakPlace.Mmuh
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound(new { message = $"No MMUH student found for institutionId={institutionId} and socNumber={socNumber}" });

            return Ok(student);
        }

        // ── MmuhStaff endpoints ──────────────────────────────────────────────

        /// <summary>
        /// Returns all MMUH staff for the given region.
        /// </summary>
        [HttpGet("mmuh-staff/by-region/{regionId}")]
        public async Task<IActionResult> GetMmuhStaffByRegion([FromRoute] int regionId)
        {
            var staff = await _context.MmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.RegionId == regionId)
                .Select(s => new
                {
                    s.Id,
                    s.MmuhStaffId,
                    s.InstId,
                    s.InternalSchoolId,
                    s.RegionId,
                    s.InstName,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Address,
                    s.Phone,
                    s.Citizenship,
                    s.Nationality,
                    s.IdentDocument,
                    s.IdentDocumentNumber,
                    s.FromCountry,
                    s.InFiz,
                    s.Druyq,
                    s.PartlyIds,
                    s.PartlyInstNames,
                    s.PositionName,
                    s.PositionId,
                    s.PositionDetailId,
                    s.PositionDetailName,
                    s.GroupIds,
                    Groups = s.Groups.Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        Subjects = g.Subjects.Select(sub => new
                        {
                            sub.SubjectId,
                            sub.SubjectName,
                            sub.SubjectType,
                            sub.SubjectTypeId
                        })
                    }),
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(staff);
        }

        /// <summary>
        /// Returns all MMUH staff that belong to the given institution (by InstId).
        /// </summary>
        [HttpGet("mmuh-staff/by-institution/{institutionId}")]
        public async Task<IActionResult> GetMmuhStaffByInstitution([FromRoute] int institutionId)
        {
            var staff = await _context.MmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.InstId == institutionId)
                .Select(s => new
                {
                    s.Id,
                    s.MmuhStaffId,
                    s.InstId,
                    s.InternalSchoolId,
                    s.RegionId,
                    s.InstName,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Address,
                    s.Phone,
                    s.Citizenship,
                    s.Nationality,
                    s.IdentDocument,
                    s.IdentDocumentNumber,
                    s.FromCountry,
                    s.InFiz,
                    s.Druyq,
                    s.PartlyIds,
                    s.PartlyInstNames,
                    s.PositionName,
                    s.PositionId,
                    s.PositionDetailId,
                    s.PositionDetailName,
                    s.GroupIds,
                    Groups = s.Groups.Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        Subjects = g.Subjects.Select(sub => new
                        {
                            sub.SubjectId,
                            sub.SubjectName,
                            sub.SubjectType,
                            sub.SubjectTypeId
                        })
                    }),
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(staff);
        }

        /// <summary>
        /// Returns MMUH teachers (PositionName == "Դասախոս") for the given institution (by InstId).
        /// </summary>
        [HttpGet("mmuh-teachers/by-institution/{institutionId}")]
        public async Task<IActionResult> GetMmuhTeachersByInstitution([FromRoute] int institutionId)
        {
            var teachers = await _context.MmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.InstId == institutionId && s.PositionName == "Դասախոս")
                .ToListAsync();

            var result = teachers.Select(s => new
            {
                Id = s.Id,
                KtakTeacherId = s.MmuhStaffId,
                KtakSchoolId = s.InstId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                SocNumber = s.SocNumber,
                Phone = s.Phone,
                Address = s.Address,
                SchoolName = s.InstName,
                SubjectNames = s.Groups.SelectMany(g => g.Subjects.Select(sub => sub.SubjectName)).Distinct().ToList(),
                Place = KtakPlace.Mmuh
            });

            return Ok(result);
        }

        /// <summary>
        /// Returns MMUH teachers (PositionName == "Դասախոս") with their groups and subjects for the given institution.
        /// </summary>
        [HttpGet("mmuh-teachers/with-subjects/by-institution/{institutionId}")]
        public async Task<IActionResult> GetMmuhTeachersWithSubjectsByInstitution([FromRoute] int institutionId)
        {
            var teachers = await _context.MmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.InstId == institutionId && s.PositionName == "Դասախոս")
                .ToListAsync();

            var result = teachers.Select(s => new
            {
                Id = s.Id,
                KtakTeacherId = s.MmuhStaffId,
                KtakSchoolId = s.InstId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                SocNumber = s.SocNumber,
                Phone = s.Phone,
                Address = s.Address,
                SchoolName = s.InstName,
                Place = KtakPlace.Mmuh,
                SubjectNames = s.Groups
                    .SelectMany(g => g.Subjects.Select(sub => sub.SubjectName))
                    .Distinct()
                    .ToList()
            });

            return Ok(result);
        }

        /// <summary>
        /// Returns FirstName, LastName, KtakTeacherId and KtakSchoolId for an MMUH teacher
        /// identified by InstId and SocNumber.
        /// </summary>
        [HttpGet("mmuh-teachers/by-institution/{institutionId}/{socNumber}")]
        public async Task<IActionResult> GetMmuhTeacherByInstitutionAndSoc(
            [FromRoute] int institutionId,
            [FromRoute] string socNumber)
        {
            var mmuhEntity = await _context.MmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.InstId == institutionId && s.SocNumber == socNumber && s.PositionName == "Դասախոս")
                .FirstOrDefaultAsync();

            if (mmuhEntity == null)
                return NotFound(new { message = $"No MMUH teacher found for institutionId={institutionId} and socNumber={socNumber}" });

            var teacher = new
            {
                Id = mmuhEntity.Id,
                KtakTeacherId = mmuhEntity.MmuhStaffId,
                KtakSchoolId = mmuhEntity.InstId,
                FirstName = mmuhEntity.FirstName,
                LastName = mmuhEntity.LastName,
                Phone = mmuhEntity.Phone,
                Address = mmuhEntity.Address,
                SchoolName = mmuhEntity.InstName,
                Place = KtakPlace.Mmuh,
                SubjectNames = mmuhEntity.Groups
                    .SelectMany(g => g.Subjects.Select(s => s.SubjectName))
                    .Distinct()
                    .ToList()
            };

            return Ok(teacher);
        }

        // ── NmuhInstitution endpoints ────────────────────────────────────────

        /// <summary>
        /// Returns all NMUH institutions for the given region.
        /// </summary>
        [HttpGet("nmuh-institutions/by-region/{regionId}")]
        public async Task<IActionResult> GetNmuhInstitutionsByRegion([FromRoute] int regionId)
        {
            var institutions = await _context.NmuhInstitutions
                .Where(i => i.RegionId == regionId)
                .Select(i => new
                {
                    i.Id,
                    i.InstId,
                    i.RegionId,
                    i.Name,
                    i.LegalMarzId,
                    i.LegalAddress,
                    i.BusinessMarzId,
                    i.BusinessAddress,
                    i.Email,
                    i.CreatedAt,
                    i.UpdatedAt
                })
                .ToListAsync();

            return Ok(institutions);
        }

        /// <summary>
        /// Returns a summary (DshhSchoolId, KtakSchoolId, Name) of all NMUH institutions for the given region.
        /// </summary>
        [HttpGet("nmuh-institutions/summary/by-region/{regionId}")]
        public async Task<IActionResult> GetNmuhInstitutionsSummaryByRegion([FromRoute] int regionId)
        {
            var institutions = await _context.NmuhInstitutions
                .Where(i => i.RegionId == regionId)
                .Select(i => new NmuhInstitutionSummaryDto
                {
                    DshhSchoolId = i.Id,
                    KtakSchoolId = i.InstId,
                    Name = i.Name,
                    Email = i.Email,
                    Place = KtakPlace.Nmuh
                })
                .ToListAsync();

            return Ok(institutions);
        }

        // ── NmuhStudent endpoints ────────────────────────────────────────────

        /// <summary>
        /// Returns all NMUH students for the given region.
        /// </summary>
        [HttpGet("nmuh-students/by-region/{regionId}")]
        public async Task<IActionResult> GetNmuhStudentsByRegion([FromRoute] int regionId)
        {
            var students = await _context.NmuhStudents
                .Where(s => s.RegionId == regionId)
                .Select(s => new
                {
                    s.Id,
                    s.NmuhStudentId,
                    s.NmuhSchoolId,
                    s.RegionId,
                    s.SchoolName,
                    s.Marz,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Graduated,
                    s.EduYear,
                    s.GroupId,
                    s.ClassroomGrade,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(students);
        }

        /// <summary>
        /// Returns all NMUH students that belong to the given institution (by InstId).
        /// </summary>
        [HttpGet("nmuh-students/by-institution/{institutionId}")]
        public async Task<IActionResult> GetNmuhStudentsByInstitution([FromRoute] int institutionId)
        {
            var students = await _context.NmuhStudents
                .Where(s => s.NmuhSchoolId == institutionId)
                .Select(s => new
                {
                    s.Id,
                    s.NmuhStudentId,
                    s.NmuhSchoolId,
                    s.InternalSchoolId,
                    s.RegionId,
                    s.SchoolName,
                    s.Marz,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Graduated,
                    s.EduYear,
                    s.GroupId,
                    s.ClassroomGrade,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(students);
        }

        /// <summary>
        /// Returns a summary (Id, KtakPupilId, KtakSchoolId, FirstName, LastName, SocNumber, Grade)
        /// of all NMUH students that belong to the given institution (by InstId).
        /// </summary>
        [HttpGet("nmuh-students/summary/by-institution/{institutionId}")]
        public async Task<IActionResult> GetNmuhStudentsSummaryByInstitution([FromRoute] int institutionId)
        {
            var students = await _context.NmuhStudents
                .Where(s => s.NmuhSchoolId == institutionId)
                .Select(s => new NmuhStudentSummaryDto
                {
                    Id = s.Id,
                    KtakPupilId = s.NmuhStudentId,
                    KtakSchoolId = s.NmuhSchoolId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    SocNumber = s.SocNumber,
                    Grade = s.ClassroomGrade,
                    Place = KtakPlace.Nmuh
                })
                .ToListAsync();

            return Ok(students);
        }

        /// <summary>
        /// Returns FirstName, LastName, Grade and KtakSchoolId for an NMUH student
        /// identified by NmuhSchoolId and SocNumber.
        /// </summary>
        [HttpGet("nmuh-students/by-institution/{institutionId}/{socNumber}")]
        public async Task<IActionResult> GetNmuhStudentByInstitutionAndSoc(
            [FromRoute] int institutionId,
            [FromRoute] string socNumber)
        {
            var student = await _context.NmuhStudents
                .Where(s => s.NmuhSchoolId == institutionId && s.SocNumber == socNumber)
                .Select(s => new
                {
                    Id = s.Id,
                    KtakPupilId = s.NmuhStudentId,
                    KtakSchoolId = s.NmuhSchoolId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Grade = s.ClassroomGrade,
                    Place = KtakPlace.Nmuh
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound(new { message = $"No NMUH student found for institutionId={institutionId} and socNumber={socNumber}" });

            return Ok(student);
        }

        // ── NmuhStaff endpoints ──────────────────────────────────────────────

        /// <summary>
        /// Returns all NMUH staff for the given region.
        /// </summary>
        [HttpGet("nmuh-staff/by-region/{regionId}")]
        public async Task<IActionResult> GetNmuhStaffByRegion([FromRoute] int regionId)
        {
            var staff = await _context.NmuhStaff
                .Where(s => s.RegionId == regionId)
                .Select(s => new
                {
                    s.Id,
                    s.NmuhStaffId,
                    s.InstId,
                    s.InternalSchoolId,
                    s.RegionId,
                    s.InstName,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Address,
                    s.Phone,
                    s.Citizenship,
                    s.Nationality,
                    s.IdentDocument,
                    s.IdentDocumentNumber,
                    s.FromCountry,
                    s.InFiz,
                    s.Druyq,
                    s.PartlyIds,
                    s.PartlyInstNames,
                    s.PositionName,
                    s.PositionId,
                    s.PositionDetailId,
                    s.PositionDetailName,
                    s.GroupIds,
                    Groups = s.Groups.Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        Subjects = g.Subjects.Select(sub => new
                        {
                            sub.SubjectId,
                            sub.SubjectName,
                            sub.SubjectType,
                            sub.SubjectTypeId
                        })
                    }),
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(staff);
        }

        /// <summary>
        /// Returns all NMUH staff that belong to the given institution (by InstId).
        /// </summary>
        [HttpGet("nmuh-staff/by-institution/{institutionId}")]
        public async Task<IActionResult> GetNmuhStaffByInstitution([FromRoute] int institutionId)
        {
            var staff = await _context.NmuhStaff
                .Where(s => s.InstId == institutionId)
                .Select(s => new
                {
                    s.Id,
                    s.NmuhStaffId,
                    s.InstId,
                    s.InternalSchoolId,
                    s.RegionId,
                    s.InstName,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Address,
                    s.Phone,
                    s.Citizenship,
                    s.Nationality,
                    s.IdentDocument,
                    s.IdentDocumentNumber,
                    s.FromCountry,
                    s.InFiz,
                    s.Druyq,
                    s.PartlyIds,
                    s.PartlyInstNames,
                    s.PositionName,
                    s.PositionId,
                    s.PositionDetailId,
                    s.PositionDetailName,
                    s.GroupIds,
                    Groups = s.Groups.Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        Subjects = g.Subjects.Select(sub => new
                        {
                            sub.SubjectId,
                            sub.SubjectName,
                            sub.SubjectType,
                            sub.SubjectTypeId
                        })
                    }),
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(staff);
        }

        /// <summary>
        /// Returns NMUH teachers (PositionName == "Դասախոս") for the given institution (by InstId).
        /// </summary>
        [HttpGet("nmuh-teachers/by-institution/{institutionId}")]
        public async Task<IActionResult> GetNmuhTeachersByInstitution([FromRoute] int institutionId)
        {
            var teachers = await _context.NmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.InstId == institutionId && s.PositionName == "Դասախոս")
                .ToListAsync();

            var result = teachers.Select(s => new
            {
                Id = s.Id,
                KtakTeacherId = s.NmuhStaffId,
                KtakSchoolId = s.InstId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                SocNumber = s.SocNumber,
                Phone = s.Phone,
                Address = s.Address,
                SchoolName = s.InstName,
                SubjectNames = s.Groups.SelectMany(g => g.Subjects.Select(sub => sub.SubjectName)).Distinct().ToList(),
                Place = KtakPlace.Nmuh
            });

            return Ok(result);
        }

        /// <summary>
        /// Returns NMUH teachers (PositionName == "Դասախոս") with their groups and subjects for the given institution.
        /// </summary>
        [HttpGet("nmuh-teachers/with-subjects/by-institution/{institutionId}")]
        public async Task<IActionResult> GetNmuhTeachersWithSubjectsByInstitution([FromRoute] int institutionId)
        {
            var teachers = await _context.NmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.InstId == institutionId && s.PositionName == "Դասախոս")
                .ToListAsync();

            var result = teachers.Select(s => new
            {
                Id = s.Id,
                KtakTeacherId = s.NmuhStaffId,
                KtakSchoolId = s.InstId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                SocNumber = s.SocNumber,
                Phone = s.Phone,
                Address = s.Address,
                SchoolName = s.InstName,
                Place = KtakPlace.Nmuh,
                SubjectNames = s.Groups
                    .SelectMany(g => g.Subjects.Select(sub => sub.SubjectName))
                    .Distinct()
                    .ToList()
            });

            return Ok(result);
        }

        /// <summary>
        /// Returns FirstName, LastName, KtakTeacherId and KtakSchoolId for an NMUH teacher
        /// identified by InstId and SocNumber.
        /// </summary>
        [HttpGet("nmuh-teachers/by-institution/{institutionId}/{socNumber}")]
        public async Task<IActionResult> GetNmuhTeacherByInstitutionAndSoc(
            [FromRoute] int institutionId,
            [FromRoute] string socNumber)
        {
            var nmuhEntity = await _context.NmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.InstId == institutionId && s.SocNumber == socNumber && s.PositionName == "Դասախոս")
                .FirstOrDefaultAsync();

            if (nmuhEntity == null)
                return NotFound(new { message = $"No NMUH teacher found for institutionId={institutionId} and socNumber={socNumber}" });

            var teacher = new
            {
                Id = nmuhEntity.Id,
                KtakTeacherId = nmuhEntity.NmuhStaffId,
                KtakSchoolId = nmuhEntity.InstId,
                FirstName = nmuhEntity.FirstName,
                LastName = nmuhEntity.LastName,
                Phone = nmuhEntity.Phone,
                Address = nmuhEntity.Address,
                SchoolName = nmuhEntity.InstName,
                Place = KtakPlace.Nmuh,
                SubjectNames = nmuhEntity.Groups
                    .SelectMany(g => g.Subjects.Select(s => s.SubjectName))
                    .Distinct()
                    .ToList()
            };

            return Ok(teacher);
        }

        // ── Director endpoints ───────────────────────────────────────────────

        /// <summary>
        /// Returns all directors (employees with directed schools) for the given region.
        /// </summary>
        [HttpGet("directors/by-region/{regionId}")]
        public async Task<IActionResult> GetDirectorsByRegion([FromRoute] int regionId)
        {
            var directors = await _context.SchoolEmployees
                .Where(e => e.RegionId == regionId &&
                            (e.Position == "Տնօրեն" || e.Position == "Վարչատնտեսական համակարգող"))
                .Select(e => new
                {
                    e.Id,
                    e.PersonId,
                    e.SchoolId,
                    e.RegionId,
                    e.FirstName,
                    e.LastName,
                    e.FatherName,
                    e.SocNumber,
                    e.Phone,
                    e.Position,
                    e.StaffGroup,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .ToListAsync();

            return Ok(directors);
        }

        // ── SchoolEmployee endpoints ─────────────────────────────────────────

        /// <summary>
        /// Returns all school employees for the given region.
        /// </summary>
        [HttpGet("school-employees/by-region/{regionId}")]
        public async Task<IActionResult> GetSchoolEmployeesByRegion([FromRoute] int regionId)
        {
            var employees = await _context.SchoolEmployees
                .Where(e => e.RegionId == regionId)
                .Select(e => new
                {
                    e.Id,
                    e.PersonId,
                    e.SchoolId,
                    e.RegionId,
                    e.FirstName,
                    e.LastName,
                    e.FatherName,
                    e.Sex,
                    e.SocNumber,
                    e.DateOfBirth,
                    e.Address,
                    e.Phone,
                    e.MainSubjectId,
                    e.Position,
                    e.StaffGroup,
                    e.VacationId,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .ToListAsync();

            return Ok(employees);
        }

        // ── Get by ID endpoints ──────────────────────────────────────────────

        /// <summary>
        /// Returns a single school by its internal DshhSchoolId.
        /// </summary>
        [HttpGet("schools/{id:guid}")]
        public async Task<IActionResult> GetSchoolById([FromRoute] Guid id)
        {
            var school = await _context.Schools
                .Where(s => s.DshhSchoolId == id)
                .Select(s => new SchoolDto
                {
                    DshhSchoolId = s.DshhSchoolId,
                    KtakSchoolId = s.KtakSchoolId,
                    RegionId     = s.RegionId,
                    Name         = s.Name,
                    Marz         = s.Marz,
                    Region       = s.Region,
                    Community    = s.Community,
                    CreatedAt    = s.CreatedAt,
                    UpdatedAt    = s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (school == null)
                return NotFound(new { message = $"School {id} not found." });

            return Ok(school);
        }

        /// <summary>
        /// Returns a single classroom by its Id.
        /// </summary>
        [HttpGet("classrooms/{id:guid}")]
        public async Task<IActionResult> GetClassroomById([FromRoute] Guid id)
        {
            var classroom = await _context.Classrooms
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.KtakSchoolId,
                    c.KtakClassroomId,
                    c.RegionId,
                    c.Grade,
                    c.Classifier,
                    c.ClassName,
                    c.Stream,
                    c.SchoolId,
                    c.CreatedAt,
                    c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (classroom == null)
                return NotFound(new { message = $"Classroom {id} not found." });

            return Ok(classroom);
        }

        /// <summary>
        /// Returns a single pupil by its Id.
        /// </summary>
        [HttpGet("pupils/{id:guid}")]
        public async Task<IActionResult> GetPupilById([FromRoute] Guid id)
        {
            var pupil = await _context.Pupils
                .Where(p => p.Id == id)
                .Select(p => new PupilDto
                {
                    Id                  = p.Id,
                    KtakPupilId         = p.KtakPupilId,
                    KtakSchoolId        = p.KtakSchoolId,
                    RegionId            = p.RegionId,
                    ClassroomId         = p.ClassroomId,
                    ClassroomInternalId = p.ClassroomInternalId,
                    Place               = p.Place,
                    Grade               = p.Grade,
                    SubGrade            = p.SubGrade,
                    FirstName           = p.FirstName,
                    LastName            = p.LastName,
                    FatherName          = p.FatherName,
                    CertificateType     = p.CertificateType,
                    Certificate         = p.Certificate,
                    SocNumber           = p.SocNumber,
                    Birthday            = p.Birthday,
                    Gender              = p.Gender,
                    Status              = p.Status,
                    CreatedAt           = p.CreatedAt,
                    UpdatedAt           = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (pupil == null)
                return NotFound(new { message = $"Pupil {id} not found." });

            return Ok(pupil);
        }

        /// <summary>
        /// Returns a single teacher (with subjects) by its Id.
        /// </summary>
        [HttpGet("teachers/{id:guid}")]
        public async Task<IActionResult> GetTeacherById([FromRoute] Guid id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Subjects)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.KtakTeacherId,
                    t.KtakSchoolId,
                    t.RegionId,
                    t.Place,
                    t.FirstName,
                    t.LastName,
                    t.FatherName,
                    t.Gender,
                    t.Birthday,
                    t.Phone,
                    t.Address,
                    t.Email,
                    t.SocNumber,
                    t.Experience,
                    t.AcademicRank,
                    t.Education,
                    t.CommandDate,
                    t.DigitLevel,
                    t.Activated,
                    t.WorkType,
                    t.CreatedAt,
                    t.UpdatedAt,
                    Subjects = t.Subjects.Select(s => new
                    {
                        s.Id,
                        s.SubjectId,
                        s.Grade,
                        s.SubGrade,
                        s.Name
                    })
                })
                .FirstOrDefaultAsync();

            if (teacher == null)
                return NotFound(new { message = $"Teacher {id} not found." });

            return Ok(teacher);
        }

        /// <summary>
        /// Returns a single MMUH student by its Id.
        /// </summary>
        [HttpGet("mmuh-students/{id:guid}")]
        public async Task<IActionResult> GetMmuhStudentById([FromRoute] Guid id)
        {
            var student = await _context.MmuhStudents
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.MmuhStudentId,
                    s.MmuhSchoolId,
                    s.RegionId,
                    s.SchoolName,
                    s.Marz,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Graduated,
                    s.GroupId,
                    s.ClassroomGrade,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound(new { message = $"MmuhStudent {id} not found." });

            return Ok(student);
        }

        /// <summary>
        /// Returns a single MMUH staff member by its Id.
        /// </summary>
        [HttpGet("mmuh-staff/{id:guid}")]
        public async Task<IActionResult> GetMmuhStaffById([FromRoute] Guid id)
        {
            var staff = await _context.MmuhStaff
                .Include(s => s.Groups)
                    .ThenInclude(g => g.Subjects)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.MmuhStaffId,
                    s.InstId,
                    s.InternalSchoolId,
                    s.RegionId,
                    s.InstName,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Address,
                    s.Phone,
                    s.Citizenship,
                    s.Nationality,
                    s.IdentDocument,
                    s.IdentDocumentNumber,
                    s.FromCountry,
                    s.InFiz,
                    s.Druyq,
                    s.PartlyIds,
                    s.PartlyInstNames,
                    s.PositionName,
                    s.PositionId,
                    s.PositionDetailId,
                    s.PositionDetailName,
                    s.GroupIds,
                    Groups = s.Groups.Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        Subjects = g.Subjects.Select(sub => new
                        {
                            sub.SubjectId,
                            sub.SubjectName,
                            sub.SubjectType,
                            sub.SubjectTypeId
                        })
                    }),
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (staff == null)
                return NotFound(new { message = $"MmuhStaff {id} not found." });

            return Ok(staff);
        }

        /// <summary>
        /// Returns a single NMUH student by its Id.
        /// </summary>
        [HttpGet("nmuh-students/{id:guid}")]
        public async Task<IActionResult> GetNmuhStudentById([FromRoute] Guid id)
        {
            var student = await _context.NmuhStudents
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.NmuhStudentId,
                    s.NmuhSchoolId,
                    s.RegionId,
                    s.SchoolName,
                    s.Marz,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Graduated,
                    s.EduYear,
                    s.GroupId,
                    s.ClassroomGrade,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound(new { message = $"NmuhStudent {id} not found." });

            return Ok(student);
        }

        /// <summary>
        /// Returns a single NMUH staff member by its Id.
        /// </summary>
        [HttpGet("nmuh-staff/{id:guid}")]
        public async Task<IActionResult> GetNmuhStaffById([FromRoute] Guid id)
        {
            var staff = await _context.NmuhStaff
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.NmuhStaffId,
                    s.InstId,
                    s.RegionId,
                    s.InstName,
                    s.FirstName,
                    s.LastName,
                    s.FatherName,
                    s.DateOfBirth,
                    s.SocNumber,
                    s.Sex,
                    s.Address,
                    s.Phone,
                    s.Citizenship,
                    s.Nationality,
                    s.IdentDocument,
                    s.IdentDocumentNumber,
                    s.FromCountry,
                    s.InFiz,
                    s.Druyq,
                    s.PartlyIds,
                    s.PartlyInstNames,
                    s.PositionName,
                    s.PositionId,
                    s.PositionDetailId,
                    s.PositionDetailName,
                    s.GroupIds,
                    Groups = s.Groups.Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        Subjects = g.Subjects.Select(sub => new
                        {
                            sub.SubjectId,
                            sub.SubjectName,
                            sub.SubjectType,
                            sub.SubjectTypeId
                        })
                    }),
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (staff == null)
                return NotFound(new { message = $"NmuhStaff {id} not found." });

            return Ok(staff);
        }

        /// <summary>
        /// Returns a single school employee by its Id.
        /// </summary>
        [HttpGet("school-employees/{id:guid}")]
        public async Task<IActionResult> GetSchoolEmployeeById([FromRoute] Guid id)
        {
            var employee = await _context.SchoolEmployees
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.PersonId,
                    e.SchoolId,
                    e.RegionId,
                    e.FirstName,
                    e.LastName,
                    e.FatherName,
                    e.Sex,
                    e.SocNumber,
                    e.DateOfBirth,
                    e.Address,
                    e.Phone,
                    e.MainSubjectId,
                    e.Position,
                    e.StaffGroup,
                    e.VacationId,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound(new { message = $"SchoolEmployee {id} not found." });

            return Ok(employee);
        }

        // ── Exempt endpoints ─────────────────────────────────────────────────

        /// <summary>
        /// Forwards a pupil-exemption request to the external CRM API and relays the response.
        /// Protected by X-API-KEY header (IncomingApiKey). Attaches OutgoingApiKey when calling CRM.
        /// </summary>
        [HttpPost("pupils/exempt")]
        public async Task<IActionResult> ExemptPupils([FromBody] StudentExempt payload)
        {
            var client = _httpClientFactory.CreateClient("ktakapi");

            var outgoingKey = _configuration["Integration:OutgoingApiKey"];
            client.DefaultRequestHeaders.Remove("X-API-KEY");
            client.DefaultRequestHeaders.Add("X-API-KEY", outgoingKey);

            var response = await client.PostAsJsonAsync("exempt-pupils", payload);

            var body = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, body);
        }
    }
}
