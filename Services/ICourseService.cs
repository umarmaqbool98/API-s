using MarksApi.Dtos;

namespace MarksApi.Services;

public interface ICourseService
{
    Task<PagedResultDto<CourseSummaryDto>> GetCoursesAsync(int page, int pageSize, string? search, string? sort, CancellationToken ct);
    Task<CourseDetailDto> GetCourseByIdAsync(int id, CancellationToken ct);
    Task<CourseDetailDto> CreateCourseAsync(CreateCourseRequestDto request, CancellationToken ct);
    Task<CourseDetailDto> UpdateCourseAsync(int id, UpdateCourseRequestDto request, CancellationToken ct);
    Task DeleteCourseAsync(int id, CancellationToken ct);
    Task<PagedResultDto<EnrollmentDetailDto>> GetEnrollmentsForCourseAsync(
        int courseId, int page, int pageSize, string? search, string? sort, CancellationToken ct);
}
