using System.ComponentModel.DataAnnotations;

namespace MarksApi.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    [Range(0, 100)]
    public int? Mark { get; set; }
    public DateTime EnrolledAtUtc { get; set; }
}
