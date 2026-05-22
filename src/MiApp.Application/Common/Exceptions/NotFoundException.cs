namespace MiApp.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} con Id '{key}' no fue encontrado.") { }
}
