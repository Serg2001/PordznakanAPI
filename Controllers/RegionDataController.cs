using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Enums;

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

        
    }
}
