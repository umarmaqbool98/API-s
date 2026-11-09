using MarksApi.Dtos;

namespace MarksApi.Services;

public interface IEnrollmentService
{
    Task<EnrollmentDetailDto> CreateEnrollmentAsync(int courseId, CreateEnrollmentRequestDto request, CancellationToken ct);
    Task<EnrollmentDetailDto> UpdateMarkAsync(int enrollmentId, UpdateEnrollmentMarkRequestDto request, CancellationToken ct);
    Task DeleteEnrollmentAsync(int enrollmentId, CancellationToken ct);
}
