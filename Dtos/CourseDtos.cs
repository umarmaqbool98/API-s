using System.ComponentModel.DataAnnotations;

namespace MarksApi.Dtos;

public class CourseSummaryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CourseDetailDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateCourseRequestDto
{
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;
    [Range(1, 4)]
    public int CreditHours { get; set; }
}

public class UpdateCourseRequestDto
{
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;
    [Range(1, 4)]
    public int CreditHours { get; set; }
}

public class CourseStatsDto
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public int Enrolled { get; set; }
    public int Graded { get; set; }
    public double? Average { get; set; }
    public int? Highest { get; set; }
    public int? Lowest { get; set; }
    public int Passed { get; set; }
}
