using MarksApi.Data;
using MarksApi.Dtos;
using MarksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MarksApi.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(AppDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResultDto<StudentSummaryDto>> GetStudentsAsync(int page, int pageSize, string? search, string? sort, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _context.Students.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.FullName.Contains(term) || s.RegistrationNo.Contains(term) || s.Email.Contains(term));
        }

        query = sort?.ToLowerInvariant() switch
        {
            "registrationno" => query.OrderBy(s => s.RegistrationNo),
            "email" => query.OrderBy(s => s.Email),
            _ => query.OrderBy(s => s.FullName)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentSummaryDto
            {
                Id = s.Id,
                RegistrationNo = s.RegistrationNo,
                FullName = s.FullName,
                Email = s.Email,
                CreatedAtUtc = s.CreatedAtUtc
            })
            .ToListAsync(ct);

        return new PagedResultDto<StudentSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StudentDetailDto> GetStudentByIdAsync(int id, CancellationToken ct)
    {
        var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null) throw new NotFoundException($"Student with id {id} was not found.");

        return new StudentDetailDto
        {
            Id = student.Id,
            RegistrationNo = student.RegistrationNo,
            FullName = student.FullName,
            Email = student.Email,
            CreatedAtUtc = student.CreatedAtUtc
        };
    }

    public async Task<StudentDetailDto> CreateStudentAsync(CreateStudentRequestDto request, CancellationToken ct)
    {
        var registrationNo = request.RegistrationNo?.Trim();
        var fullName = request.FullName?.Trim();
        var email = request.Email?.Trim();

        if (string.IsNullOrWhiteSpace(registrationNo)) throw new BadRequestException("Registration number is required.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new BadRequestException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new BadRequestException("Email is required.");

        var normalizedEmail = email.ToLowerInvariant();
        var entity = new Student
        {
            RegistrationNo = registrationNo,
            FullName = fullName,
            Email = normalizedEmail,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Students.Add(entity);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created student {StudentId} with registration number {RegistrationNo}", entity.Id, entity.RegistrationNo);

        return new StudentDetailDto { Id = entity.Id, RegistrationNo = entity.RegistrationNo, FullName = entity.FullName, Email = entity.Email, CreatedAtUtc = entity.CreatedAtUtc };
    }

    public async Task<StudentDetailDto> UpdateStudentAsync(int id, UpdateStudentRequestDto request, CancellationToken ct)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null) throw new NotFoundException($"Student with id {id} was not found.");

        var registrationNo = request.RegistrationNo?.Trim();
        var fullName = request.FullName?.Trim();
        var email = request.Email?.Trim();

        if (string.IsNullOrWhiteSpace(registrationNo)) throw new BadRequestException("Registration number is required.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new BadRequestException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new BadRequestException("Email is required.");

        var normalizedEmail = email.ToLowerInvariant();
        student.RegistrationNo = registrationNo;
        student.FullName = fullName;
        student.Email = normalizedEmail;

        await _context.SaveChangesAsync(ct);

        return new StudentDetailDto { Id = student.Id, RegistrationNo = student.RegistrationNo, FullName = student.FullName, Email = student.Email, CreatedAtUtc = student.CreatedAtUtc };
    }

    public async Task DeleteStudentAsync(int id, CancellationToken ct)
    {
        var student = await _context.Students.Include(s => s.Enrollments).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null) throw new NotFoundException($"Student with id {id} was not found.");
        if (student.Enrollments.Any()) throw new ConflictException("Student cannot be deleted because they still have enrollments.");

        _context.Students.Remove(student);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted student {StudentId}", id);
    }
}
