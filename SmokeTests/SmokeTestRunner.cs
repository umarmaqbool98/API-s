using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MarksApi.SmokeTests;

public sealed class SmokeTestRunner
{
    private readonly string _baseUrl;

    public SmokeTestRunner(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<bool> RunAsync(CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        var checks = new List<(string Method, string Path, int Expected, int Actual)>();
        var unique = DateTime.UtcNow.Ticks.ToString();
        var registrationNo = $"SM{unique[^10..]}";
        var email = $"smoke-{unique}@example.com";
        var courseCode = $"S{unique[^9..]}";
        var studentId = 0;
        var courseId = 0;
        var enrollmentId = 0;
        var teacherToken = string.Empty;
        var studentToken = string.Empty;

        async Task<(int Status, string Body)> SendAsync(
            HttpMethod method,
            string path,
            object? payload = null,
            string? token = null)
        {
            using var request = new HttpRequestMessage(method, path);
            if (payload is not null)
            {
                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await client.SendAsync(request, cancellationToken);
            return ((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
        }

        async Task<(int Status, string Body)> CheckAsync(
            HttpMethod method,
            string path,
            int expected,
            object? payload = null,
            string? token = null)
        {
            var result = await SendAsync(method, path, payload, token);
            checks.Add((method.Method, path, expected, result.Status));
            return result;
        }

        static int ReadId(string json) =>
            JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();

        static string ReadToken(string json) =>
            JsonDocument.Parse(json).RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Login response did not contain a token.");

        try
        {
            await CheckAsync(HttpMethod.Get, "/health", 200);

            var teacherLogin = await CheckAsync(
                HttpMethod.Post,
                "/api/auth/login",
                200,
                new { username = "teacher1", password = "Teacher123!" });
            if (teacherLogin.Status == 200)
            {
                teacherToken = ReadToken(teacherLogin.Body);
            }

            await CheckAsync(
                HttpMethod.Post,
                "/api/auth/login",
                401,
                new { username = "teacher1", password = "wrong-password" });

            var studentLogin = await CheckAsync(
                HttpMethod.Post,
                "/api/auth/login",
                200,
                new { username = "student1", password = "Student123!" });
            if (studentLogin.Status == 200)
            {
                studentToken = ReadToken(studentLogin.Body);
            }

            await CheckAsync(HttpMethod.Get, "/api/students?page=1&pageSize=10", 401);
            await CheckAsync(HttpMethod.Get, "/api/students?page=1&pageSize=10", 200, token: teacherToken);
            await CheckAsync(HttpMethod.Get, "/api/students?page=1&pageSize=10", 403, token: studentToken);
            await CheckAsync(HttpMethod.Get, "/api/students/99999999", 404, token: teacherToken);
            await CheckAsync(HttpMethod.Get, "/api/students/1", 200, token: studentToken);
            await CheckAsync(HttpMethod.Get, "/api/students/2", 403, token: studentToken);

            var studentCreate = await CheckAsync(
                HttpMethod.Post,
                "/api/students",
                201,
                new { registrationNo, fullName = "Smoke Student", email },
                teacherToken);
            if (studentCreate.Status == 201)
            {
                studentId = ReadId(studentCreate.Body);
            }

            await CheckAsync(HttpMethod.Post, "/api/students", 400, new { registrationNo = "", fullName = "", email = "bad" }, teacherToken);
            await CheckAsync(
                HttpMethod.Post,
                "/api/students",
                409,
                new { registrationNo, fullName = "Duplicate Smoke Student", email = $"duplicate-{unique}@example.com" },
                teacherToken);

            if (studentId > 0)
            {
                await CheckAsync(HttpMethod.Get, $"/api/students/{studentId}", 200, token: teacherToken);
                await CheckAsync(
                    HttpMethod.Put,
                    $"/api/students/{studentId}",
                    200,
                    new { registrationNo, fullName = "Updated Smoke Student", email },
                    teacherToken);
                await CheckAsync(
                    HttpMethod.Put,
                    "/api/students/99999999",
                    404,
                    new { registrationNo = "SM-MISSING", fullName = "Missing", email = "missing@example.com" },
                    teacherToken);
            }

            await CheckAsync(HttpMethod.Get, "/api/courses?page=1&pageSize=10", 200, token: teacherToken);
            await CheckAsync(HttpMethod.Get, "/api/courses/99999999", 404, token: teacherToken);
            await CheckAsync(HttpMethod.Get, "/api/courses/1", 200, token: studentToken);

            var courseCreate = await CheckAsync(
                HttpMethod.Post,
                "/api/courses",
                201,
                new { code = courseCode, title = "Smoke Course", creditHours = 3 },
                teacherToken);
            if (courseCreate.Status == 201)
            {
                courseId = ReadId(courseCreate.Body);
            }

            await CheckAsync(
                HttpMethod.Post,
                "/api/courses",
                400,
                new { code = "BAD", title = "Bad Credit Hours", creditHours = 9 },
                teacherToken);
            await CheckAsync(
                HttpMethod.Post,
                "/api/courses",
                409,
                new { code = "CS401", title = "Duplicate Course", creditHours = 3 },
                teacherToken);

            if (courseId > 0)
            {
                await CheckAsync(
                    HttpMethod.Put,
                    $"/api/courses/{courseId}",
                    200,
                    new { code = courseCode, title = "Updated Smoke Course", creditHours = 4 },
                    teacherToken);
                await CheckAsync(
                    HttpMethod.Put,
                    "/api/courses/99999999",
                    404,
                    new { code = "SM-MISSING", title = "Missing", creditHours = 3 },
                    teacherToken);
                await CheckAsync(HttpMethod.Get, $"/api/courses/{courseId}/enrollments", 200, token: teacherToken);
                await CheckAsync(HttpMethod.Get, $"/api/courses/{courseId}/stats", 200, token: teacherToken);
                await CheckAsync(HttpMethod.Get, "/api/courses/99999999/stats", 404, token: teacherToken);
            }

            if (studentId > 0 && courseId > 0)
            {
                var enrollmentCreate = await CheckAsync(
                    HttpMethod.Post,
                    $"/api/courses/{courseId}/enrollments",
                    201,
                    new { studentId, mark = (int?)null },
                    teacherToken);
                if (enrollmentCreate.Status == 201)
                {
                    enrollmentId = ReadId(enrollmentCreate.Body);
                }

                await CheckAsync(
                    HttpMethod.Post,
                    $"/api/courses/{courseId}/enrollments",
                    409,
                    new { studentId, mark = (int?)null },
                    teacherToken);
                await CheckAsync(
                    HttpMethod.Post,
                    "/api/courses/99999999/enrollments",
                    404,
                    new { studentId },
                    teacherToken);
                await CheckAsync(
                    HttpMethod.Post,
                    $"/api/courses/{courseId}/enrollments",
                    404,
                    new { studentId = 99999999 },
                    teacherToken);
                await CheckAsync(
                    HttpMethod.Get,
                    $"/api/students/{studentId}/transcript",
                    200,
                    token: teacherToken);
                await CheckAsync(
                    HttpMethod.Get,
                    "/api/students/2/transcript",
                    403,
                    token: studentToken);
            }

            if (enrollmentId > 0)
            {
                await CheckAsync(
                    HttpMethod.Put,
                    $"/api/enrollments/{enrollmentId}/mark",
                    200,
                    new { mark = 82 },
                    teacherToken);
                await CheckAsync(
                    HttpMethod.Put,
                    $"/api/enrollments/{enrollmentId}/mark",
                    400,
                    new { mark = 101 },
                    teacherToken);
                await CheckAsync(
                    HttpMethod.Put,
                    $"/api/enrollments/{enrollmentId}/mark",
                    400,
                    new { mark = -1 },
                    teacherToken);
                await CheckAsync(
                    HttpMethod.Put,
                    "/api/enrollments/99999999/mark",
                    404,
                    new { mark = 50 },
                    teacherToken);
                await CheckAsync(HttpMethod.Delete, $"/api/students/{studentId}", 409, token: teacherToken);
                await CheckAsync(HttpMethod.Delete, $"/api/courses/{courseId}", 409, token: teacherToken);
                await CheckAsync(HttpMethod.Delete, "/api/enrollments/99999999", 404, token: teacherToken);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"FAIL smoke-test runner: {exception.Message}");
        }
        finally
        {
            if (enrollmentId > 0)
            {
                await CheckAsync(HttpMethod.Delete, $"/api/enrollments/{enrollmentId}", 204, token: teacherToken);
            }

            if (studentId > 0)
            {
                await CheckAsync(HttpMethod.Delete, $"/api/students/{studentId}", 204, token: teacherToken);
            }

            if (courseId > 0)
            {
                await CheckAsync(HttpMethod.Delete, $"/api/courses/{courseId}", 204, token: teacherToken);
            }
        }

        foreach (var check in checks)
        {
            Console.WriteLine(
                $"{(check.Expected == check.Actual ? "PASS" : "FAIL")} {check.Method} {check.Path} expected {check.Expected} actual {check.Actual}");
        }

        var passed = checks.Count > 0 && checks.All(check => check.Expected == check.Actual);
        Console.WriteLine($"SUMMARY smoke-test {(passed ? "PASS" : "FAIL")} ({checks.Count(check => check.Expected == check.Actual)}/{checks.Count} checks)");
        return passed;
    }
}
