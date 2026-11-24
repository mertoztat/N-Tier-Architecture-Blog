namespace BlogAPI.Core.Exceptions;

/// <summary>
/// Base class for expected business errors. The API's global exception handler turns these
/// into ProblemDetails responses with <see cref="StatusCode"/>; any other exception is a 500.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403) { }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message, 400) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}
