using MarksApi.Models;

namespace MarksApi.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
