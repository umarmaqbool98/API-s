using System.ComponentModel.DataAnnotations;

namespace MarksApi.Models;

public class Student
{
    public int Id { get; set; }
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
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
}
