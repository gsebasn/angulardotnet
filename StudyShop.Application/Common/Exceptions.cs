namespace StudyShop.Application.Common;

public class CommandValidationException : Exception
{
    public CommandValidationException(IEnumerable<ValidationError> errors) : base("Validation failed")
    {
        Errors = errors;
    }
    public IEnumerable<ValidationError> Errors { get; }
}

public class ValidationError
{
    public string Property { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}


