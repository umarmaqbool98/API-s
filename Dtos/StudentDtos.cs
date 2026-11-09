using System.ComponentModel.DataAnnotations;

namespace MarksApi.Dtos;

public class StudentSummaryDto
{
    public int Id { get; set; }
    public string RegistrationNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class StudentDetailDto
{
    public int Id { get; set; }
    public string RegistrationNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateStudentRequestDto
{
    [Required]
    [StringLength(20)]
    public string RegistrationNo { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}

public class UpdateStudentRequestDto
{
    [Required]
    [StringLength(20)]
    public string RegistrationNo { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}
