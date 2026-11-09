using MarksApi.Dtos;
using MarksApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarksApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("students/{studentId:int}/transcript")]
    [Authorize(Roles = "Teacher,Student")]
    public async Task<ActionResult<StudentTranscriptDto>> GetTranscript(int studentId, CancellationToken ct)
    {
        if (User.IsInRole("Student"))
        {
            var currentStudentId = User.FindFirst("student_id")?.Value;
            if (string.IsNullOrWhiteSpace(currentStudentId) || !int.TryParse(currentStudentId, out var parsedStudentId) || parsedStudentId != studentId)
            {
                throw new ForbiddenException("A student may only read their own transcript.");
            }
        }

        return Ok(await _reportService.GetStudentTranscriptAsync(studentId, ct));
    }
}
