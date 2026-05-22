using MiApp.Domain.Exceptions;

namespace MiApp.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }

    private Category() { Name = null!; }

    public static Category Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la categoría es obligatorio.");

        return new Category { Name = name.Trim() };
    }
}
