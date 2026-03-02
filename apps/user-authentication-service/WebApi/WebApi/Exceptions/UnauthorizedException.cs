namespace WebApi.Exceptions;

/// <summary>
/// Исключение неавторизованного доступа
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Неавторизованный доступ")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
