using Microsoft.AspNetCore.Http;

namespace MarksApi.Services;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }

    protected AppException(int statusCode, string title, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(StatusCodes.Status404NotFound, "Not found", message) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(StatusCodes.Status409Conflict, "Conflict", message) { }
}

public sealed class BadRequestException : AppException
{
    public BadRequestException(string message) : base(StatusCodes.Status400BadRequest, "Bad request", message) { }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(StatusCodes.Status403Forbidden, "Forbidden", message) { }
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(StatusCodes.Status401Unauthorized, "Unauthorized", message) { }
}
