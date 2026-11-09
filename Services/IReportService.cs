using MarksApi.Dtos;

namespace MarksApi.Services;

public interface IReportService
{
    Task<CourseStatsDto> GetCourseStatsAsync(int courseId, CancellationToken ct);
    Task<StudentTranscriptDto> GetStudentTranscriptAsync(int studentId, CancellationToken ct);
}
