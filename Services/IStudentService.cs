using MarksApi.Dtos;

namespace MarksApi.Services;

public interface IStudentService
{
    Task<PagedResultDto<StudentSummaryDto>> GetStudentsAsync(int page, int pageSize, string? search, string? sort, CancellationToken ct);
    Task<StudentDetailDto> GetStudentByIdAsync(int id, CancellationToken ct);
    Task<StudentDetailDto> CreateStudentAsync(CreateStudentRequestDto request, CancellationToken ct);
    Task<StudentDetailDto> UpdateStudentAsync(int id, UpdateStudentRequestDto request, CancellationToken ct);
    Task DeleteStudentAsync(int id, CancellationToken ct);
}
