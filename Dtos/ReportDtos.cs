namespace MarksApi.Dtos;

public class TranscriptCourseDto
{
    public string Code { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public int? Mark { get; set; }
    public bool Passed { get; set; }
}

public class StudentTranscriptDto
{
    public int StudentId { get; set; }
    public string RegistrationNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<TranscriptCourseDto> Courses { get; set; } = new();
    public double? WeightedAverage { get; set; }
}
