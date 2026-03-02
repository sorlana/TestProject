namespace WebApi.Exceptions;

/// <summary>
/// Исключение ненайденного ресурса
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException()
        : base("Ресурс не найден")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} с идентификатором {key} не найден")
    {
    }
}
