using MarksApi.Data;
using MarksApi.Dtos;
using MarksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MarksApi.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(AppDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EnrollmentDetailDto> CreateEnrollmentAsync(int courseId, CreateEnrollmentRequestDto request, CancellationToken ct)
    {
        var studentId = request.StudentId;
        if (studentId <= 0) throw new BadRequestException("Student id must be greater than zero.");

        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId, ct);
        if (!courseExists) throw new NotFoundException($"Course with id {courseId} was not found.");

        var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId, ct);
        if (!studentExists) throw new NotFoundException($"Student with id {studentId} was not found.");

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            Mark = request.Mark,
            EnrolledAtUtc = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created enrollment {EnrollmentId} for student {StudentId} in course {CourseId}", enrollment.Id, studentId, courseId);

        return new EnrollmentDetailDto { Id = enrollment.Id, StudentId = enrollment.StudentId, CourseId = enrollment.CourseId, Mark = enrollment.Mark, EnrolledAtUtc = enrollment.EnrolledAtUtc };
    }

    public async Task<EnrollmentDetailDto> UpdateMarkAsync(int enrollmentId, UpdateEnrollmentMarkRequestDto request, CancellationToken ct)
    {
        if (request.Mark < 0 || request.Mark > 100) throw new BadRequestException("A mark must be between 0 and 100.");

        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);
        if (enrollment is null) throw new NotFoundException($"Enrollment with id {enrollmentId} was not found.");

        enrollment.Mark = request.Mark;
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Updated mark for enrollment {EnrollmentId} to {Mark}", enrollment.Id, enrollment.Mark);

        return new EnrollmentDetailDto { Id = enrollment.Id, StudentId = enrollment.StudentId, CourseId = enrollment.CourseId, Mark = enrollment.Mark, EnrolledAtUtc = enrollment.EnrolledAtUtc };
    }

    public async Task DeleteEnrollmentAsync(int enrollmentId, CancellationToken ct)
    {
        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);
        if (enrollment is null) throw new NotFoundException($"Enrollment with id {enrollmentId} was not found.");

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted enrollment {EnrollmentId}", enrollmentId);
    }
}
