using MarksApi.Data;
using MarksApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MarksApi.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public ReportService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<CourseStatsDto> GetCourseStatsAsync(int courseId, CancellationToken ct)
    {
        var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId, ct);
        if (course is null) throw new NotFoundException($"Course with id {courseId} was not found.");

        var enrollments = await _context.Enrollments.AsNoTracking().Where(e => e.CourseId == courseId).ToListAsync(ct);
        var graded = enrollments.Where(e => e.Mark.HasValue).Select(e => e.Mark!.Value).ToList();
        var passMark = _configuration.GetValue<int>("PassMark", 50);

        return new CourseStatsDto
        {
            CourseId = course.Id,
            CourseCode = course.Code,
            Enrolled = enrollments.Count,
            Graded = graded.Count,
            Average = graded.Any() ? graded.Average() : null,
            Highest = graded.Any() ? graded.Max() : null,
            Lowest = graded.Any() ? graded.Min() : null,
            Passed = graded.Count(m => m >= passMark)
        };
    }

    public async Task<StudentTranscriptDto> GetStudentTranscriptAsync(int studentId, CancellationToken ct)
    {
        var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId, ct);
        if (student is null) throw new NotFoundException($"Student with id {studentId} was not found.");

        var enrollments = await _context.Enrollments.AsNoTracking().Include(e => e.Course).Where(e => e.StudentId == studentId).ToListAsync(ct);
        var passMark = _configuration.GetValue<int>("PassMark", 50);

        var courses = enrollments.Select(e => new TranscriptCourseDto
        {
            Code = e.Course.Code,
            CreditHours = e.Course.CreditHours,
            Mark = e.Mark,
            Passed = e.Mark.HasValue && e.Mark.Value >= passMark
        }).ToList();

        var gradedCourses = courses.Where(c => c.Mark.HasValue).ToList();
        double? weightedAverage = gradedCourses.Count > 0
            ? gradedCourses.Sum(c => c.Mark!.Value * c.CreditHours) / (double)gradedCourses.Sum(c => c.CreditHours)
            : null;

        return new StudentTranscriptDto
        {
            StudentId = student.Id,
            RegistrationNo = student.RegistrationNo,
            FullName = student.FullName,
            Courses = courses,
            WeightedAverage = weightedAverage
        };
    }
}
