# Guía de Estudio — MiApp con Clean Architecture y .NET 10

**Materia:** Backend 2026 — TUDS Tercer Año | **Prof.:** Nicolás Ortiz

---

## ¿Qué construimos?

Una Web API de e-commerce con:
- **4 capas** (Clean Architecture)
- **Autenticación JWT** con 2 roles (Admin y Customer)
- **CQRS con MediatR** (casos de uso separados por Commands y Queries)
- **EF Core + SQLite** (base de datos)
- **FluentValidation** (validaciones automáticas)

---

## Las 4 Capas — Resumen Visual

```
┌─────────────────────────────┐
│   WebApi  (Controllers)     │  ← Recibe HTTP, devuelve JSON
├─────────────────────────────┤
│   Infrastructure            │  ← EF Core, JWT, BCrypt, BD
├─────────────────────────────┤
│   Application  ★            │  ← Casos de uso, validaciones
├─────────────────────────────┤
│   Domain                    │  ← Entidades, reglas de negocio
└─────────────────────────────┘
         ↑ depende de la capa de abajo, nunca al revés
```

**Regla de oro:** Domain no sabe nada de nadie. Application solo conoce Domain. Infrastructure implementa lo que Application necesita.

---

## 1. DOMAIN — El núcleo (sin dependencias externas)

### Entidades creadas
Todas heredan de `BaseEntity` que tiene el `Id (Guid)` generado automáticamente.

| Entidad | Propiedades clave |
|---------|------------------|
| `Product` | Name, Price, Stock, IsActive |
| `User` | Name, Email, PasswordHash, Role |
| `Category` | Name |
| `Order` | UserId, Status, Items, Total |
| `OrderItem` | ProductId, Quantity, UnitPrice, Subtotal |

### Cómo se crea una entidad (Factory Method)
```csharp
// ❌ MAL — el constructor es privado, no se puede hacer así
var p = new Product();

// ✅ BIEN — única forma válida de crear un producto
var product = Product.Create("Laptop", "Desc", 1500, 10);
```
El método `Create()` valida los datos antes de crear el objeto. Si algo está mal, lanza una `DomainException`.

### Reglas de negocio en la entidad
```csharp
// La regla vive en la entidad, no en el Controller ni en el Handler
product.RemoveStock(5);   // valida que haya stock suficiente
product.UpdatePrice(-10); // lanza DomainException: "El precio debe ser mayor a cero"
order.Confirm();          // valida que el pedido tenga items y esté en estado Pending
```

### Enums
```csharp
public enum OrderStatus { Pending=1, Confirmed=2, Shipped=3, Delivered=4, Cancelled=5 }
public enum UserRole    { Admin=1, Customer=2 }
```

### Interfaces (contratos)
El Domain define QUÉ necesita, sin saber CÓMO se implementa:
```csharp
public interface IProductRepository   // implementado en Infrastructure
public interface IUserRepository
public interface IOrderRepository
public interface IUnitOfWork          // SaveChangesAsync()
```

---

## 2. APPLICATION — Casos de uso con CQRS

### ¿Qué es CQRS?
Separar las operaciones en dos tipos:

| Command | Query |
|---------|-------|
| Modifica el sistema | Solo lee |
| `CrearProductoCommand` | `GetProductoByIdQuery` |
| Retorna confirmación | Retorna DTO |

### ¿Qué es MediatR?
En vez de que el Controller llame directo al repositorio, envía un mensaje y MediatR lo enruta al Handler correcto:
```
Controller → mediator.Send(command) → Handler → Repositorio → BD
```

### Casos de uso implementados

| Caso de uso | Tipo | Quién puede usarlo |
|-------------|------|-------------------|
| `RegisterCommand` | Command | Público |
| `LoginCommand` | Command | Público |
| `CrearProductoCommand` | Command | Solo Admin |
| `GetAllProductosQuery` | Query | Admin + Customer |
| `GetProductoByIdQuery` | Query | Admin + Customer |
| `CrearOrdenCommand` | Command | Admin + Customer |

### Estructura de un caso de uso (ejemplo: CrearProducto)
```
Features/Productos/Commands/CrearProducto/
├── CrearProductoCommand.cs       ← el "mensaje" con los datos
├── CrearProductoResponse.cs      ← lo que devuelve
├── CrearProductoCommandHandler.cs ← la lógica
└── CrearProductoCommandValidator.cs ← las validaciones
```

### Cómo se ve un Command
```csharp
// Command = record con los datos de entrada
public record CrearProductoCommand(
    string Nombre,
    string Descripcion,
    decimal Precio,
    int Stock) : IRequest<CrearProductoResponse>;
```

### Cómo se ve un Handler
```csharp
public class CrearProductoCommandHandler
    : IRequestHandler<CrearProductoCommand, CrearProductoResponse>
{
    // Inyecta INTERFACES, nunca clases concretas
    private readonly IProductRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<CrearProductoResponse> Handle(
        CrearProductoCommand request, CancellationToken ct)
    {
        // 1. Verificar regla de negocio
        if (await _repo.ExistsAsync(request.Nombre, ct))
            throw new DomainException("Ya existe un producto con ese nombre.");

        // 2. Delegar creación al DOMINIO (la regla vive en Product.Create)
        var producto = Product.Create(request.Nombre, request.Descripcion,
                                      request.Precio, request.Stock);
        // 3. Persistir
        await _repo.AddAsync(producto, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Retornar DTO — NUNCA la entidad de dominio
        return new CrearProductoResponse(producto.Id, producto.Name, producto.Price);
    }
}
```

### Validación automática con FluentValidation
```csharp
public class CrearProductoCommandValidator : AbstractValidator<CrearProductoCommand>
{
    public CrearProductoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Precio).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
```
El `ValidationBehavior` corre estos validators automáticamente antes de cada Handler. Si falla → HTTP 400.

### Pipeline de MediatR (Behaviors)
Cada request pasa por este pipeline antes de llegar al Handler:
```
Request
  ↓
[ValidationBehavior]  → valida con FluentValidation → si falla: 400 Bad Request
  ↓
[LoggingBehavior]     → registra "Iniciando CrearProductoCommand..."
  ↓
[Handler]             → ejecuta el caso de uso
  ↑
[LoggingBehavior]     → registra "Completado CrearProductoCommand"
  ↑
Response
```

### Contratos de Infrastructure (interfaces)
Application define qué servicios necesita de Infrastructure:
```csharp
public interface IJwtTokenService  { string GenerateToken(User user); }
public interface IPasswordHasher   { string Hash(string p); bool Verify(string p, string h); }
public interface IEmailService     { Task EnviarConfirmacionAsync(...); }
```

---

## 3. INFRASTRUCTURE — Implementación técnica

### AppDbContext (EF Core)
```csharp
public class AppDbContext : DbContext, IUnitOfWork
{
    // Cada DbSet = una tabla en la BD
    public DbSet<Product>   Products  { get; set; }
    public DbSet<User>      Users     { get; set; }
    public DbSet<Order>     Orders    { get; set; }
    public DbSet<OrderItem> OrderItems{ get; set; }
    public DbSet<Category>  Categories{ get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
        => mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    // IUnitOfWork se satisface con SaveChangesAsync() de DbContext
}
```

### Fluent API (configuración de tablas)
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever(); // GUID generado en el dominio
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.HasIndex(p => p.Name); // índice para búsquedas rápidas
    }
}
```

### Repositorio (implementación concreta)
```csharp
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    // AsNoTracking() en lecturas = más eficiente (no guarda estado)
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct)
        => await _context.Products.AsNoTracking().ToListAsync(ct);

    // AddAsync NO llama SaveChanges — lo hace IUnitOfWork en el Handler
    public async Task AddAsync(Product entity, CancellationToken ct)
        => await _context.Products.AddAsync(entity, ct);
}
```

### JWT Token Service
```csharp
public class JwtTokenService : IJwtTokenService
{
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()) // "Admin" o "Customer"
        };
        // Firma el token con la clave secreta del appsettings.json
        var token = new JwtSecurityToken(issuer, audience, claims,
                        expires: DateTime.UtcNow.AddDays(7), signingCredentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Registro de servicios (DI)
```csharp
// InfrastructureServiceRegistration.cs
services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IUserRepository,    UserRepository>();
services.AddScoped<IOrderRepository,   OrderRepository>();
services.AddScoped<IJwtTokenService,   JwtTokenService>();
services.AddScoped<IPasswordHasher,    PasswordHasher>();
```

### Migraciones
```bash
# Crear migración cuando cambiás el modelo
dotnet ef migrations add NombreDeLaMigracion \
  --project src/MiApp.Infrastructure \
  --startup-project src/MiApp.WebApi

# Aplicar a la BD
dotnet ef database update \
  --project src/MiApp.Infrastructure \
  --startup-project src/MiApp.WebApi
```
**Migraciones creadas:**
1. `InitialCreate` — tabla Products
2. `AddAuthAndOrders` — tablas Users, Categories, Orders, OrderItems

---

## 4. WEBAPI — Endpoints HTTP

### Controllers creados

| Controller | Endpoint | Rol |
|------------|----------|-----|
| `AuthController` | `POST /api/auth/register` | Público |
| `AuthController` | `POST /api/auth/login` | Público |
| `ProductsController` | `GET /api/products` | Admin + Customer |
| `ProductsController` | `GET /api/products/{id}` | Admin + Customer |
| `ProductsController` | `POST /api/products` | **Solo Admin** |
| `OrdersController` | `POST /api/orders` | Admin + Customer |

### Cómo protege las rutas
```csharp
[Authorize]              // requiere cualquier JWT válido
[Authorize(Roles = "Admin")]  // requiere rol Admin específicamente
```

### GlobalExceptionHandler (middleware)
Intercepta todas las excepciones y devuelve el HTTP correcto:
```
DomainException    → 400 Bad Request  + mensaje de negocio
NotFoundException  → 404 Not Found
ValidationException→ 400 Bad Request  + lista de errores por campo
Exception          → 500 Internal Server Error
```

### Program.cs — cómo se registra todo
```csharp
builder.Services.AddApplicationServices();    // MediatR + Validators + Behaviors
builder.Services.AddInfrastructureServices(); // EF Core + Repos + JWT + BCrypt
builder.Services.AddAuthentication(JwtBearer);
builder.Services.AddAuthorization();

app.UseMiddleware<GlobalExceptionHandler>(); // primero el manejador de errores
app.UseAuthentication();                     // después auth
app.UseAuthorization();                      // después autorización
app.MapControllers();
```

---

## Flujo completo de una petición

### Ejemplo: `POST /api/products` con JWT de Admin

```
1. HTTP Request llega → ProductsController.Create()
        |
        | [Authorize(Roles="Admin")] → verifica el JWT → si no es Admin: 403
        ↓
2. mediator.Send(new CrearProductoCommand(...))
        ↓
3. [ValidationBehavior] → corre CrearProductoCommandValidator
        → si falla: 400 Bad Request (antes de llegar al Handler)
        ↓
4. [LoggingBehavior] → registra inicio
        ↓
5. CrearProductoCommandHandler.Handle()
        → llama _repo.ExistsAsync() → SELECT en BD
        → llama Product.Create() → valida en el DOMINIO
        → llama _repo.AddAsync() → marca como Added en EF
        → llama _unitOfWork.SaveChangesAsync() → INSERT en BD
        ↓
6. return new CrearProductoResponse(id, nombre, precio)
        ↓
7. HTTP 201 Created con JSON
```

---

## Autenticación JWT — Cómo funciona

```
1. Usuario llama POST /api/auth/login con email + password
2. Handler verifica email en BD → usuario existe?
3. Handler verifica password con BCrypt.Verify()
4. Si ok → JwtTokenService genera token firmado con la clave secreta
5. Token tiene dentro (claims): Id, Email, Role
6. Usuario guarda el token y lo manda en cada request:
   Header: Authorization: Bearer eyJhbGci...
7. ASP.NET Core verifica la firma del token automáticamente
8. Si el token es válido → extrae los claims → [Authorize] funciona
```

### Roles
- **Admin** = Role 1 → puede crear productos
- **Customer** = Role 2 → solo ver productos y crear órdenes
- Todo usuario registrado con `/register` es **Customer** por defecto

---

## Cómo probar la API

**URL:** `http://localhost:5141/scalar/v1`

### Orden de pruebas:

**1. Login:**
```json
POST /api/auth/login
{ "email": "admin@miapp.com", "password": "Admin123" }
→ Copiá el "token" de la respuesta
```

**2. Crear producto** (en Headers agregá `Authorization: Bearer <token>`):
```json
POST /api/products
{ "nombre": "Monitor 4K", "descripcion": "27 pulgadas", "precio": 450.00, "stock": 5 }
→ Copiá el "id" del producto
```

**3. Crear orden:**
```json
POST /api/orders
{ "items": [{ "productId": "<id>", "quantity": 2 }] }
→ Responde con total calculado automáticamente
```

**4. Sin token → 401 | Customer crea producto → 403**

---

## Resumen de conceptos clave

| Concepto | Qué hace |
|----------|---------|
| **Factory Method** | Única forma de crear una entidad válida (`Product.Create()`) |
| **DomainException** | Error de regla de negocio (precio negativo, stock insuficiente) |
| **CQRS** | Separa escritura (Command) de lectura (Query) |
| **MediatR** | Desacopla Controller del Handler — el Controller no sabe quién procesa |
| **Behavior** | Middleware del pipeline de MediatR (validación, logging) |
| **FluentValidation** | Valida datos de entrada antes de llegar al Handler |
| **IUnitOfWork** | Guarda todos los cambios en una sola transacción |
| **AsNoTracking()** | Consultas de solo lectura — más eficiente |
| **ValueGeneratedNever()** | El GUID lo genera el dominio, no la BD |
| **JWT** | Token firmado con claims (id, email, rol) — stateless |
| **BCrypt** | Hash seguro de contraseñas — no se guardan en texto plano |
| **GlobalExceptionHandler** | Convierte excepciones en respuestas HTTP correctas |
| **DIP** | Depender de interfaces, no de clases concretas |
| **Scoped** | Una instancia de DbContext por request HTTP |

---

*Prof. Tec. Nicolás Ortiz — Backend 2026 — TUDS Tercer Año*
