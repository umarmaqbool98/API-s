using MarksApi.Dtos;
using MarksApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IReportService _reportService;

    public CoursesController(ICourseService courseService, IEnrollmentService enrollmentService, IReportService reportService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _reportService = reportService;
    }

    [HttpGet]
    [Authorize(Roles = "Teacher,Student")]
    public async Task<ActionResult<PagedResultDto<CourseSummaryDto>>> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        return Ok(await _courseService.GetCoursesAsync(page, pageSize, search, sort, ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Teacher,Student")]
    public async Task<ActionResult<CourseDetailDto>> GetCourse(int id, CancellationToken ct)
    {
        return Ok(await _courseService.GetCourseByIdAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<CourseDetailDto>> CreateCourse([FromBody] CreateCourseRequestDto request, CancellationToken ct)
    {
        var course = await _courseService.CreateCourseAsync(request, ct);
        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<CourseDetailDto>> UpdateCourse(int id, [FromBody] UpdateCourseRequestDto request, CancellationToken ct)
    {
        return Ok(await _courseService.UpdateCourseAsync(id, request, ct));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        await _courseService.DeleteCourseAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{courseId:int}/enrollments")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<EnrollmentDetailDto>> CreateEnrollment(int courseId, [FromBody] CreateEnrollmentRequestDto request, CancellationToken ct)
    {
        var enrollment = await _enrollmentService.CreateEnrollmentAsync(courseId, request, ct);
        return CreatedAtAction(nameof(GetEnrollmentsForCourse), new { courseId, id = enrollment.Id }, enrollment);
    }

    [HttpGet("{courseId:int}/enrollments")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<PagedResultDto<EnrollmentDetailDto>>> GetEnrollmentsForCourse(
        int courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        return Ok(await _courseService.GetEnrollmentsForCourseAsync(courseId, page, pageSize, search, sort, ct));
    }

    [HttpGet("{courseId:int}/stats")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<CourseStatsDto>> GetCourseStats(int courseId, CancellationToken ct)
    {
        return Ok(await _reportService.GetCourseStatsAsync(courseId, ct));
    }
}
