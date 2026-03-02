namespace WebApi.Exceptions;

/// <summary>
/// Исключение валидации данных
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Произошла одна или несколько ошибок валидации")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base("Произошла ошибка валидации")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}
