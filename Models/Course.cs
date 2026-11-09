using System.ComponentModel.DataAnnotations;

namespace MarksApi.Models;

public class Course
{
    public int Id { get; set; }
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;
    [Range(1, 4)]
    public int CreditHours { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
}
