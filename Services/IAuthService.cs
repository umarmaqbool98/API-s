using MarksApi.Dtos;

namespace MarksApi.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct);
}
