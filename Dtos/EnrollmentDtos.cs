using System.ComponentModel.DataAnnotations;

namespace MarksApi.Dtos;

public class EnrollmentDetailDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public int? Mark { get; set; }
    public DateTime EnrolledAtUtc { get; set; }
}

public class CreateEnrollmentRequestDto
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }
    [Range(0, 100)]
    public int? Mark { get; set; }
}

public class UpdateEnrollmentMarkRequestDto
{
    [Range(0, 100)]
    public int Mark { get; set; }
}
