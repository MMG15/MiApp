# MiApp — Clean Architecture con .NET 10

Sistema de e-commerce con autenticación JWT, roles y CQRS.  
**Asignatura:** Backend 2026 — Tecnicatura Universitaria en Desarrollo de Software

---

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com) con extensión **C# Dev Kit**
- Extensión **dotnet-ef** (se instala automáticamente con el paso 3)

---

## Pasos para ejecutar el proyecto

### 1. Clonar o descomprimir el proyecto
```bash
cd C:\Users\TuUsuario\
# Si clonás:
git clone <url-del-repo> MiApp
cd MiApp
```

### 2. Restaurar dependencias
```bash
dotnet restore
```

### 3. Instalar herramienta de migraciones (solo la primera vez)
```bash
dotnet tool install --global dotnet-ef
```

### 4. Aplicar las migraciones (crea la base de datos SQLite)
```bash
dotnet ef database update \
  --project src/MiApp.Infrastructure \
  --startup-project src/MiApp.WebApi
```
Esto crea el archivo `miapp.db` dentro de `src/MiApp.WebApi/`.

### 5. Ejecutar la API
```bash
dotnet run --project src/MiApp.WebApi
```
La API queda disponible en: `http://localhost:5141`

### 6. Abrir la UI de pruebas
Abrí en el navegador: **http://localhost:5141/scalar/v1**

---

## Estructura del proyecto

```
MiApp/
├── src/
│   ├── MiApp.Domain/           ← Entidades, interfaces, excepciones (sin dependencias)
│   │   ├── Entities/           ← Product, User, Category, Order, OrderItem
│   │   ├── Interfaces/         ← IProductRepository, IUserRepository, IOrderRepository, IUnitOfWork
│   │   ├── Exceptions/         ← DomainException, ProductNotFoundException, InsufficientStockException
│   │   ├── ValueObjects/       ← Email, Money
│   │   └── Enums/              ← OrderStatus, UserRole (Admin, Customer)
│   │
│   ├── MiApp.Application/      ← Casos de uso con CQRS + MediatR
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   │   ├── Commands/Register/  ← RegisterCommand, Handler, Validator, Response
│   │   │   │   └── Commands/Login/     ← LoginCommand, Handler, Validator, Response
│   │   │   ├── Productos/
│   │   │   │   ├── Commands/CrearProducto/   ← CrearProductoCommand + Handler + Validator
│   │   │   │   └── Queries/GetProductoById/  ← GetProductoByIdQuery + Handler + Dto
│   │   │   │   └── Queries/GetAllProductos/  ← GetAllProductosQuery + Handler
│   │   │   └── Ordenes/
│   │   │       └── Commands/CrearOrden/      ← CrearOrdenCommand + Handler + Validator
│   │   ├── Common/
│   │   │   ├── Behaviors/      ← ValidationBehavior, LoggingBehavior
│   │   │   └── Exceptions/     ← NotFoundException
│   │   └── Contracts/
│   │       └── Infrastructure/ ← IJwtTokenService, IPasswordHasher, IEmailService, IPaymentGateway
│   │
│   ├── MiApp.Infrastructure/   ← EF Core, repositorios, JWT, BCrypt
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Configurations/ ← ProductConfiguration, UserConfiguration, OrderConfiguration, etc.
│   │   ├── Repositories/       ← ProductRepository, UserRepository, OrderRepository
│   │   ├── Services/           ← JwtTokenService, PasswordHasher
│   │   └── Migrations/         ← InitialCreate, AddAuthAndOrders
│   │
│   └── MiApp.WebApi/           ← Controllers, Program.cs, Middleware
│       ├── Controllers/        ← AuthController, ProductsController, OrdersController
│       └── Middleware/         ← GlobalExceptionHandler
└── README.md
```

---

## Endpoints disponibles

### Autenticación (sin JWT requerido)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/register` | Registrar nuevo usuario (rol: Customer) |
| POST | `/api/auth/login` | Iniciar sesión, devuelve JWT |

### Productos (requiere JWT)

| Método | Ruta | Rol requerido | Descripción |
|--------|------|---------------|-------------|
| GET | `/api/products` | Admin / Customer | Listar todos los productos |
| GET | `/api/products/{id}` | Admin / Customer | Obtener producto por ID |
| POST | `/api/products` | **Admin** | Crear producto |

### Órdenes (requiere JWT)

| Método | Ruta | Rol requerido | Descripción |
|--------|------|---------------|-------------|
| POST | `/api/orders` | Admin / Customer | Crear una orden |

---

## Cómo probar paso a paso

### Paso 1 — Registrar un usuario
```json
POST /api/auth/register
{
  "name": "Juan Pérez",
  "email": "juan@email.com",
  "password": "123456"
}
```
**Respuesta:** HTTP 201 con token JWT

### Paso 2 — Iniciar sesión
```json
POST /api/auth/login
{
  "email": "juan@email.com",
  "password": "123456"
}
```
**Respuesta:** HTTP 200 con token JWT. Copiá el campo `"token"`.

### Paso 3 — Usar el token en Scalar
1. En Scalar (`http://localhost:5141/scalar/v1`) buscá el botón **Authorize** o **Bearer**
2. Ingresá el token: `Bearer eyJhbGci...`

### Paso 4 — Crear un producto (solo Admin)
Para crear un usuario Admin, registrate normalmente y luego editá `Role = 1` directamente en la BD con DB Browser for SQLite.

```json
POST /api/products
Authorization: Bearer <token-admin>
{
  "nombre": "Laptop Gaming",
  "descripcion": "16GB RAM, RTX 4060",
  "precio": 1500.00,
  "stock": 10
}
```

### Paso 5 — Crear una orden
```json
POST /api/orders
Authorization: Bearer <tu-token>
{
  "items": [
    { "productId": "<id-del-producto>", "quantity": 2 }
  ]
}
```

---

## Roles

| Rol | Valor en BD | Permisos |
|-----|-------------|----------|
| Admin | 1 | Todo: crear productos, ver productos, crear órdenes |
| Customer | 2 | Ver productos, crear órdenes propias |

---

## Base de datos

El proyecto usa **SQLite** en desarrollo. El archivo `miapp.db` se crea automáticamente al correr las migraciones.

Para ver los datos podés usar:
- [DB Browser for SQLite](https://sqlitebrowser.org) — abrí `src/MiApp.WebApi/miapp.db`

### Comandos de migraciones

```bash
# Ver migraciones aplicadas
dotnet ef migrations list --project src/MiApp.Infrastructure --startup-project src/MiApp.WebApi

# Crear nueva migración (cuando cambiás el modelo)
dotnet ef migrations add NombreDeLaMigracion --project src/MiApp.Infrastructure --startup-project src/MiApp.WebApi

# Aplicar migraciones
dotnet ef database update --project src/MiApp.Infrastructure --startup-project src/MiApp.WebApi
```

---

## Tecnologías utilizadas

| Tecnología | Uso |
|------------|-----|
| .NET 10 | Framework principal |
| ASP.NET Core | Web API |
| Entity Framework Core | ORM / acceso a datos |
| SQLite | Base de datos (desarrollo) |
| MediatR | Patrón Mediator / CQRS |
| FluentValidation | Validación de comandos |
| BCrypt.Net | Hash de contraseñas |
| JWT Bearer | Autenticación stateless |
| Scalar | UI de documentación API |

---

*Prof. Tec. Nicolás Ortiz — Backend 2026 — TUDS Tercer Año*
