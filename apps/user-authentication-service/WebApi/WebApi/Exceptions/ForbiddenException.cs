namespace WebApi.Exceptions;

/// <summary>
/// Исключение запрещенного доступа
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("Доступ запрещен")
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }
}
