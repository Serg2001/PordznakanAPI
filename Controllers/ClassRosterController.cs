using Microsoft.AspNetCore.Mvc;
using PordznakanAPI.DTOs;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    /// <summary>
    /// Read-only lookups that join teachers, classes and pupils. Every {id} accepts either our
    /// internal id (GUID) or the KTAK/EMIS id (integer).
    /// </summary>
    [Route("api/roster")]
    [ApiController]
    public class ClassRosterController : ControllerBase
    {
        private readonly IClassRosterService _roster;

        public ClassRosterController(IClassRosterService roster)
        {
            _roster = roster;
        }

        /// <summary>
        /// The classes a teacher teaches, each with its pupils.
        /// </summary>
        [HttpGet("teachers/{id}/classes")]
        [ProducesResponseType(typeof(TeacherClassesResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeacherClasses([FromRoute] string id, CancellationToken ct) =>
            ToActionResult(await _roster.GetTeacherClassesAsync(id, ct));

        /// <summary>
        /// The subjects a teacher teaches.
        /// </summary>
        [HttpGet("teachers/{id}/subjects")]
        [ProducesResponseType(typeof(TeacherSubjectsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeacherSubjects([FromRoute] string id, CancellationToken ct) =>
            ToActionResult(await _roster.GetTeacherSubjectsAsync(id, ct));

        /// <summary>
        /// The pupil, their class and every other pupil of the same class.
        /// </summary>
        [HttpGet("pupils/{id}/classmates")]
        [ProducesResponseType(typeof(ClassmatesResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClassmates([FromRoute] string id, CancellationToken ct) =>
            ToActionResult(await _roster.GetClassmatesAsync(id, ct));

        private IActionResult ToActionResult<T>(RosterResult<T> result) where T : class =>
            result.Status switch
            {
                RosterStatus.Ok => Ok(result.Value),
                RosterStatus.InvalidId => BadRequest(new { message = result.Message }),
                _ => NotFound(new { message = result.Message })
            };
    }
}
