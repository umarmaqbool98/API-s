using MarksApi.Data;
using MarksApi.Dtos;
using MarksApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarksApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(AppDbContext context, ITokenService tokenService, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Authentication failed because the login request was incomplete");
            throw new UnauthorizedException("Invalid username or password.");
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim(), ct);

        if (user is null)
        {
            _logger.LogWarning("Authentication failed for username {Username}", request.Username);
            throw new UnauthorizedException("Invalid username or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Authentication failed for username {Username}", request.Username);
            throw new UnauthorizedException("Invalid username or password.");
        }

        _logger.LogInformation("User {Username} signed in with role {Role}", user.Username, user.Role);
        var token = _tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:LifetimeMinutes", 60));

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAt,
            Role = user.Role
        };
    }
}
