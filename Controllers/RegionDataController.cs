using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Enums;
using PordznakanAPI.Models;

namespace PordznakanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionDataController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegionDataController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 1.1 – Returns each school's DshhSchoolId, KtakSchoolId, and Name for the given region. avelacnel hasce
        /// </summary>
        [HttpGet("schools/summary/by-region/{regionId}")]
        public async Task<IActionResult> GetSchoolSummaryByRegion([FromRoute] int regionId)
        {
            var schools = await _context.Schools
                .Where(s => s.RegionId == regionId)
                .Select(s => new
                {
                    s.DshhSchoolId,
                    s.KtakSchoolId,
                    s.Name
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
                .Where(p => p.KtakSchoolId == schoolId)
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
                    t.DigitLevel
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
            var teachers = await _context.Teachers
                .Where(t => t.KtakSchoolId == schoolId)
                .Select(t => new
                {
                    t.FirstName,
                    t.LastName,
                    t.SocNumber,
                    t.DigitLevel
                })
                .ToListAsync();

            return Ok(teachers);
        }

        /// <summary>
        /// T.5 – Returns FirstName, LastName, DigitLevel and Subjects for a teacher
        /// identified by KtakSchoolId and SocNumber.
        /// </summary>
        [HttpGet("teachers/by-school/{schoolId}/{socNumber}")]
        public async Task<IActionResult> GetTeacherBySchoolAndSoc(
            [FromRoute] int schoolId,
            [FromRoute] string socNumber)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Subjects)
                .Where(t => t.KtakSchoolId == schoolId && t.SocNumber == socNumber)
                .Select(t => new
                {
                    t.FirstName,
                    t.LastName,
                    t.DigitLevel,
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
                return NotFound(new { message = $"No teacher found for schoolId={schoolId} and socNumber={socNumber}" });

            return Ok(teacher);
        }

        /// <summary>
        /// P.6 – Returns FirstName, LastName, Grade and SubGrade for a pupil
        /// identified by KtakSchoolId and Certificate number (ident_document_number).
        /// </summary>
        [HttpGet("pupils/by-school/{schoolId}/{certNumber}")]
        public async Task<IActionResult> GetPupilBySchoolAndCert(
            [FromRoute] int schoolId,
            [FromRoute] string certNumber)
        {
            var pupil = await _context.Pupils
                .Where(p => p.KtakSchoolId == schoolId && p.Certificate == certNumber)
                .Select(p => new
                {
                    p.FirstName,
                    p.LastName,
                    p.Grade,
                    p.SubGrade
                })
                .FirstOrDefaultAsync();

            if (pupil == null)
                return NotFound(new { message = $"No pupil found for schoolId={schoolId} and certificate={certNumber}" });

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
                    i.CreatedAt,
                    i.UpdatedAt
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

        // ── MmuhStaff endpoints ──────────────────────────────────────────────

        /// <summary>
        /// Returns all MMUH staff for the given region.
        /// </summary>
        [HttpGet("mmuh-staff/by-region/{regionId}")]
        public async Task<IActionResult> GetMmuhStaffByRegion([FromRoute] int regionId)
        {
            var staff = await _context.MmuhStaff
                .Where(s => s.RegionId == regionId)
                .Select(s => new
                {
                    s.Id,
                    s.MmuhStaffId,
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
                    s.GroupId,
                    s.GroupsJson,
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
                    s.GroupId,
                    s.GroupsJson,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(staff);
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
                    i.CreatedAt,
                    i.UpdatedAt
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
                    s.GroupId,
                    s.GroupsJson,
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
                    s.GroupId,
                    s.GroupsJson,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(staff);
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
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.MmuhStaffId,
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
                    s.GroupId,
                    s.GroupsJson,
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
                    s.GroupId,
                    s.GroupsJson,
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
    }
}
