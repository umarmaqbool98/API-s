using MarksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MarksApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("students");
            entity.HasIndex(s => s.RegistrationNo).IsUnique();
            entity.HasIndex(s => s.Email).IsUnique();
            entity.Property(s => s.RegistrationNo).HasMaxLength(20).IsRequired();
            entity.Property(s => s.FullName).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Email).HasMaxLength(150).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_students_email_lowercase", "email = lower(email)"));
            entity.Property(s => s.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");
            entity.HasIndex(c => c.Code).IsUnique();
            entity.Property(c => c.Code).HasMaxLength(10).IsRequired();
            entity.Property(c => c.Title).HasMaxLength(150).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_courses_credit_hours", "credit_hours BETWEEN 1 AND 4"));
            entity.Property(c => c.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollments");
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
            entity.HasOne(e => e.Student).WithMany(s => s.Enrollments).HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Course).WithMany(c => c.Enrollments).HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("CK_enrollments_mark", "mark IS NULL OR (mark >= 0 AND mark <= 100)"));
            entity.Property(e => e.EnrolledAtUtc).HasColumnName("enrolled_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.ToTable(t => t.HasCheckConstraint("CK_users_role", "role IN ('Teacher', 'Student')"));
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(20).IsRequired();
            entity.HasOne(u => u.Student).WithMany().HasForeignKey(u => u.StudentId).OnDelete(DeleteBehavior.SetNull);
        });

        var student1 = new Student { Id = 1, RegistrationNo = "CS-2022-001", FullName = "Ali Raza", Email = "ali.raza@example.com", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        var student2 = new Student { Id = 2, RegistrationNo = "CS-2022-002", FullName = "Sara Khan", Email = "sara.khan@example.com", CreatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) };
        var course1 = new Course { Id = 1, Code = "CS401", Title = "Advanced Programming", CreditHours = 3, CreatedAtUtc = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc) };
        var course2 = new Course { Id = 2, Code = "MA201", Title = "Statistics", CreditHours = 2, CreatedAtUtc = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc) };
        var course3 = new Course { Id = 3, Code = "AI101", Title = "AI Fundamentals", CreditHours = 3, CreatedAtUtc = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc) };

        modelBuilder.Entity<Student>().HasData(student1, student2);
        modelBuilder.Entity<Course>().HasData(course1, course2, course3);

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "teacher1", PasswordHash = "AQAAAAIAAYagAAAAEHBY1+owFQWIN+8snMJphVR/OeelqNrNbrhZIyXLCc7NBb1ciU0ZjtdpucYcsF0qgw==", Role = "Teacher" },
            new User { Id = 2, Username = "student1", PasswordHash = "AQAAAAIAAYagAAAAEC/M7XimnN9I/zqG6irRBLYCZdShQxNeWY+q0uykFL66PDIrZIZuRij7Q6imBOF/GA==", Role = "Student", StudentId = 1 },
            new User { Id = 3, Username = "student2", PasswordHash = "AQAAAAIAAYagAAAAEOtbb+70QVkQJA0r3ataFlZqmh6ZLNaOjb2RjzH3XLThBuxRu7Do/UBzkU+1VK25LQ==", Role = "Student", StudentId = 2 }
        );

        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment { Id = 1, StudentId = 1, CourseId = 1, Mark = 80, EnrolledAtUtc = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
            new Enrollment { Id = 2, StudentId = 1, CourseId = 2, Mark = 60, EnrolledAtUtc = new DateTime(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
            new Enrollment { Id = 3, StudentId = 1, CourseId = 3, Mark = null, EnrolledAtUtc = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc) },
            new Enrollment { Id = 4, StudentId = 2, CourseId = 1, Mark = 75, EnrolledAtUtc = new DateTime(2026, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
            new Enrollment { Id = 5, StudentId = 2, CourseId = 2, Mark = 65, EnrolledAtUtc = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Enrollment { Id = 6, StudentId = 2, CourseId = 3, Mark = 90, EnrolledAtUtc = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc) }
        );
    }

}
