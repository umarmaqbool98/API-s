using MarksApi.Data;
using MarksApi.Dtos;
using MarksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MarksApi.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(AppDbContext context, ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResultDto<CourseSummaryDto>> GetCoursesAsync(int page, int pageSize, string? search, string? sort, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _context.Courses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Code.Contains(term) || c.Title.Contains(term));
        }

        query = sort?.ToLowerInvariant() switch
        {
            "code" => query.OrderBy(c => c.Code),
            "credithours" => query.OrderBy(c => c.CreditHours),
            _ => query.OrderBy(c => c.Title)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(c => new CourseSummaryDto { Id = c.Id, Code = c.Code, Title = c.Title, CreditHours = c.CreditHours, CreatedAtUtc = c.CreatedAtUtc }).ToListAsync(ct);

        return new PagedResultDto<CourseSummaryDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<CourseDetailDto> GetCourseByIdAsync(int id, CancellationToken ct)
    {
        var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course is null) throw new NotFoundException($"Course with id {id} was not found.");

        return new CourseDetailDto { Id = course.Id, Code = course.Code, Title = course.Title, CreditHours = course.CreditHours, CreatedAtUtc = course.CreatedAtUtc };
    }

    public async Task<CourseDetailDto> CreateCourseAsync(CreateCourseRequestDto request, CancellationToken ct)
    {
        var code = request.Code?.Trim();
        var title = request.Title?.Trim();

        if (string.IsNullOrWhiteSpace(code)) throw new BadRequestException("Course code is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new BadRequestException("Course title is required.");
        if (request.CreditHours < 1 || request.CreditHours > 4) throw new BadRequestException("Credit hours must be between 1 and 4.");

        var entity = new Course { Code = code, Title = title, CreditHours = request.CreditHours, CreatedAtUtc = DateTime.UtcNow };
        _context.Courses.Add(entity);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created course {CourseId} with code {CourseCode}", entity.Id, entity.Code);

        return new CourseDetailDto { Id = entity.Id, Code = entity.Code, Title = entity.Title, CreditHours = entity.CreditHours, CreatedAtUtc = entity.CreatedAtUtc };
    }

    public async Task<CourseDetailDto> UpdateCourseAsync(int id, UpdateCourseRequestDto request, CancellationToken ct)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course is null) throw new NotFoundException($"Course with id {id} was not found.");

        var code = request.Code?.Trim();
        var title = request.Title?.Trim();

        if (string.IsNullOrWhiteSpace(code)) throw new BadRequestException("Course code is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new BadRequestException("Course title is required.");
        if (request.CreditHours < 1 || request.CreditHours > 4) throw new BadRequestException("Credit hours must be between 1 and 4.");

        course.Code = code;
        course.Title = title;
        course.CreditHours = request.CreditHours;
        await _context.SaveChangesAsync(ct);

        return new CourseDetailDto { Id = course.Id, Code = course.Code, Title = course.Title, CreditHours = course.CreditHours, CreatedAtUtc = course.CreatedAtUtc };
    }

    public async Task DeleteCourseAsync(int id, CancellationToken ct)
    {
        var course = await _context.Courses.Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course is null) throw new NotFoundException($"Course with id {id} was not found.");
        if (course.Enrollments.Any()) throw new ConflictException("Course cannot be deleted because it still has enrollments.");

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted course {CourseId}", id);
    }

    public async Task<PagedResultDto<EnrollmentDetailDto>> GetEnrollmentsForCourseAsync(
        int courseId, int page, int pageSize, string? search, string? sort, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var exists = await _context.Courses.AsNoTracking().AnyAsync(c => c.Id == courseId, ct);
        if (!exists) throw new NotFoundException($"Course with id {courseId} was not found.");

        var query = _context.Enrollments.AsNoTracking().Where(e => e.CourseId == courseId);
        if (!string.IsNullOrWhiteSpace(search) && int.TryParse(search, out var studentId))
        {
            query = query.Where(e => e.StudentId == studentId);
        }

        query = sort?.ToLowerInvariant() switch
        {
            "mark" => query.OrderBy(e => e.Mark),
            "studentid" => query.OrderBy(e => e.StudentId),
            _ => query.OrderBy(e => e.Id)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EnrollmentDetailDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                Mark = e.Mark,
                EnrolledAtUtc = e.EnrolledAtUtc
            })
            .ToListAsync(ct);

        return new PagedResultDto<EnrollmentDetailDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
