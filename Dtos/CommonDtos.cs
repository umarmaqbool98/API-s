using System.ComponentModel.DataAnnotations;

namespace MarksApi.Dtos;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public class LoginRequestDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string Role { get; set; } = string.Empty;
}
