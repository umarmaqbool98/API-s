using MarksApi.Dtos;
using MarksApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<PagedResultDto<StudentSummaryDto>>> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        return Ok(await _studentService.GetStudentsAsync(page, pageSize, search, sort, ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Teacher,Student")]
    public async Task<ActionResult<StudentDetailDto>> GetStudent(int id, CancellationToken ct)
    {
        var student = await _studentService.GetStudentByIdAsync(id, ct);

        if (User.IsInRole("Student"))
        {
            var currentStudentId = User.FindFirst("student_id")?.Value;
            if (string.IsNullOrWhiteSpace(currentStudentId) || !int.TryParse(currentStudentId, out var parsedStudentId) || parsedStudentId != id)
            {
                throw new ForbiddenException("A student may only read their own record.");
            }
        }

        return Ok(student);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<StudentDetailDto>> CreateStudent([FromBody] CreateStudentRequestDto request, CancellationToken ct)
    {
        var student = await _studentService.CreateStudentAsync(request, ct);
        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<StudentDetailDto>> UpdateStudent(int id, [FromBody] UpdateStudentRequestDto request, CancellationToken ct)
    {
        return Ok(await _studentService.UpdateStudentAsync(id, request, ct));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteStudent(int id, CancellationToken ct)
    {
        await _studentService.DeleteStudentAsync(id, ct);
        return NoContent();
    }
}
