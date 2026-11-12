# MiApp — Sistema distribuido: E-commerce + PaymentService

**Asignatura:** Backend (Optativa II) 2026 — Trabajo Final Integrador
**Opción elegida:** **Opción 1 — PaymentService** (flujo request/response y manejo de estados de la orden)

Dos proyectos .NET 8 independientes, cada uno con su propia solución, su propio puerto y su propia base de datos,
comunicados **solo por HTTP** (`HttpClient` vía `IHttpClientFactory`).

> **Organización del repo:** un solo repositorio con dos carpetas.
> - `/` (raíz) → **E-commerce** (`MiApp.sln`, carpeta `src/`)
> - `/PaymentService` → **PaymentService** (`PaymentService/PaymentService.sln`, carpeta `PaymentService/src/`)

```
Cliente (Swagger)
      │  POST /api/orders/{id}/confirm   (con JWT)
      ▼
┌────────────────────────┐    HttpClient + JWT     ┌────────────────────────┐
│ E-commerce API  :7100  │ ──────────────────────▶ │ PaymentService  :7200  │
│ Products·Orders·Users  │   { orderId, amount }   │ Aprueba/rechaza pagos  │
│ miapp.db (SQLite)      │ ◀────────────────────── │ payments.db (SQLite)   │
└────────────────────────┘ { status, transactionId }└────────────────────────┘
```

---

## 1. Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Nada más: las bases SQLite se crean solas y las migraciones se aplican al arrancar cada servicio.
  (Opcional: `dotnet tool install --global dotnet-ef` para crear migraciones nuevas.)

## 2. Cómo levantar los dos servicios

Abrí **dos terminales** desde la raíz del repo. Levantá primero PaymentService.

**Terminal 1 — PaymentService (puerto 7200)**
```bash
dotnet run --project PaymentService/src/PaymentService.Api
```

**Terminal 2 — E-commerce (puerto 7100)**
```bash
dotnet run --project src/MiApp.WebApi
```

| Servicio       | URL base                | Swagger                               | Base de datos |
|----------------|-------------------------|---------------------------------------|---------------|
| E-commerce     | `http://localhost:7100` | http://localhost:7100/swagger         | `src/MiApp.WebApi/miapp.db` |
| PaymentService | `http://localhost:7200` | http://localhost:7200/swagger         | `PaymentService/src/PaymentService.Api/payments.db` |

Al arrancar, cada servicio ejecuta `Database.MigrateAsync()` (crea o actualiza su base).
El e-commerce además crea el **usuario Admin** si no existe.

> La URL de PaymentService que usa el e-commerce está en `src/MiApp.WebApi/appsettings.json`
> → `"Services": { "Payment": "http://localhost:7200/" }`. Si cambiás el puerto de PaymentService
> (en su `Properties/launchSettings.json`), cambiá también esa clave.

## 3. Usuario Admin de prueba

| Email               | Contraseña  | Rol   |
|---------------------|-------------|-------|
| `admin@sistema.com` | `Admin123!` | Admin |

- Lo crea `DbInitializer` al iniciar el e-commerce (sección `SeedAdmin` de `appsettings.Development.json`).
- Todo usuario que se registra queda como **Customer**. Un Admin puede promoverlo con
  `PUT /api/users/{id}/role` → `{ "rol": "Admin" }` (el usuario tiene que volver a loguearse para que su token tenga el rol nuevo).

## 4. Regla de negocio de PaymentService

La regla vive en la entidad de dominio `Payment.Process(...)`
(`PaymentService/src/PaymentService.Domain/Entities/Payment.cs`):

| Condición                                   | Resultado    | Motivo (`reason`)                                  |
|---------------------------------------------|--------------|----------------------------------------------------|
| Moneda distinta de `ARS`                    | **Rejected** | "Solo se aceptan pagos en ARS."                    |
| Monto **≥ 100.000 ARS**                     | **Rejected** | "El monto ... supera el límite de aprobación ..."  |
| Monto **> 0 y < 100.000 ARS**               | **Approved** | —                                                  |
| Monto ≤ 0 u `orderId` vacío                 | **400 Bad Request** (validación, no se registra el pago) |

Todos los pagos (aprobados o rechazados) se guardan con un `transactionId` con formato `TX-XXXXXXXX`.

**Idempotencia:** el `orderId` tiene índice único. Si llega dos veces el mismo `orderId`,
PaymentService devuelve el pago ya registrado y no crea uno nuevo (evita cobrar dos veces cuando el e-commerce reintenta).

## 5. Qué hace el e-commerce con la respuesta

Estados de la orden (`OrderStatus`): `Pending` → `Paid` | `PaymentRejected`

| Situación                                  | Estado de la orden | Respuesta al cliente |
|--------------------------------------------|--------------------|----------------------|
| PaymentService responde `Approved`         | **Paid** (se guarda `transactionId` y fecha) | `200` |
| PaymentService responde `Rejected`         | **PaymentRejected** y **se devuelve el stock** | `200` con el motivo |
| PaymentService **apagado o no responde**   | **sigue Pending** (no se guarda nada) | `503` "El servicio de pagos no está disponible... podés volver a confirmarla más tarde" |
| La orden no está Pending (ya pagada/rechazada) | sin cambios | `400` |
| La orden es de otro usuario                | sin cambios | `403` |

**Resiliencia (punto extra):** el `HttpClient` tipado tiene timeout total de 10 s, timeout por intento de 3 s
y **2 reintentos con Polly** (300 ms y 600 ms) ante errores de red, 5xx, 408 o timeout.

## 6. Decisión sobre JWT entre servicios

La llamada e-commerce → PaymentService **va protegida con JWT**.
PaymentService no emite tokens: valida los del e-commerce con el mismo `Issuer`, `Audience` y `Secret`.
Un `DelegatingHandler` (`BearerTokenForwardingHandler`) reenvía el header `Authorization` del usuario
que confirmó la orden. Así PaymentService no queda abierto: sin token responde `401`.

El `Secret` **no** está en `appsettings.json`: está en `appsettings.Development.json` (solo para desarrollo).
En producción se pasa por variable de entorno: `JwtSettings__Secret=...`.

## 7. Endpoints

### E-commerce — `http://localhost:7100`

| Método | Ruta                          | Acceso         | Descripción |
|--------|-------------------------------|----------------|-------------|
| POST   | `/api/auth/register`          | Público        | Registra un usuario (rol Customer) y devuelve token |
| POST   | `/api/auth/login`             | Público        | Devuelve el JWT |
| GET    | `/api/products`               | Logueado       | Lista productos |
| GET    | `/api/products/{id}`          | Logueado       | Un producto |
| POST   | `/api/products`               | **Admin**      | Crea un producto |
| POST   | `/api/orders`                 | Logueado       | Crea una orden (descuenta stock, queda `Pending`) |
| POST   | `/api/orders/{id}/confirm`    | Dueño          | **Confirma/paga: llama a PaymentService** |
| GET    | `/api/orders/{id}`            | Dueño o Admin  | Detalle de una orden |
| GET    | `/api/orders/my`              | Logueado       | Mis órdenes |
| GET    | `/api/orders`                 | **Admin**      | Todas las órdenes |
| PUT    | `/api/users/{id}/role`        | **Admin**      | Cambia el rol de un usuario |

### PaymentService — `http://localhost:7200` (todos requieren JWT)

| Método | Ruta                              | Descripción |
|--------|-----------------------------------|-------------|
| POST   | `/api/payments/process`           | Recibe `{ orderId, amount, currency? }` → `{ paymentId, orderId, status, transactionId, amount, currency, reason, processedAt }` |
| GET    | `/api/payments/order/{orderId}`   | Pago de una orden |
| GET    | `/api/payments`                   | Todos los pagos |

## 8. Reproducir el flujo completo con Swagger

1. Levantá los dos servicios (sección 2).
2. En http://localhost:7100/swagger → `POST /api/auth/login` con el Admin → copiá el `token`.
3. Botón **Authorize** → pegá **solo el token** (sin la palabra "Bearer").
4. `POST /api/products` → `{ "nombre": "Mouse", "descripcion": "Óptico", "precio": 15000, "stock": 10 }` → copiá el `id`.
   Creá otro caro: `{ "nombre": "Notebook", "descripcion": "i7", "precio": 900000, "stock": 5 }`.
5. `POST /api/auth/register` con un cliente → copiá su `token` y volvé a **Authorize** con ese token.
6. `POST /api/orders` → `{ "items": [ { "productId": "<id Mouse>", "quantity": 2 } ] }` → estado `Pending`, total 30000.
7. `POST /api/orders/{id}/confirm` → **`Paid`** con `transactionId`.
8. Verificá en http://localhost:7200/swagger (Authorize con el mismo token) → `GET /api/payments/order/{orderId}` → `Approved`, mismo `transactionId`.
9. Repetí 6–7 con la Notebook → **`PaymentRejected`**; `GET /api/products/{id}` muestra que el stock volvió.
10. **Camino de fallo:** cortá PaymentService (Ctrl+C), creá una orden y confirmala → **503** y `GET /api/orders/{id}` sigue en `Pending`.
    Volvé a levantar PaymentService, confirmá de nuevo → `Paid`.
11. **Roles:** con el token del cliente, `GET /api/orders` → **403**. Con el del Admin → 200.

También están los archivos `src/MiApp.WebApi/MiApp.WebApi.http` y
`PaymentService/src/PaymentService.Api/PaymentService.Api.http` con todo el flujo listo para la extensión *REST Client* de VS Code.

## 9. Arquitectura (igual en los dos proyectos)

Clean Architecture con 4 capas. Regla de dependencia: **las flechas apuntan hacia el Dominio**.

```
Api / WebApi  ──▶  Application  ──▶  Domain
     │                                 ▲
     └──────▶  Infrastructure  ────────┘   (Infrastructure implementa interfaces de Domain/Application)
```

| Capa | E-commerce (`src/`) | PaymentService (`PaymentService/src/`) |
|------|---------------------|----------------------------------------|
| **Domain** (no depende de nada) | `Product`, `Order`, `OrderItem`, `User`, VOs `Email`/`Money`, excepciones, interfaces de repositorio | `Payment` (regla de aprobación), VO `Money`, `PaymentStatus`, `IPaymentRepository` |
| **Application** (CQRS + MediatR + FluentValidation) | Commands: Register, Login, CrearProducto, CrearOrden, **ConfirmarOrden**, CambiarRol. Queries: productos y órdenes. Puerto `IPaymentClient` | Command `ProcessPayment`. Queries `GetPaymentByOrderId`, `GetAllPayments` |
| **Infrastructure** | EF Core (`AppDbContext`, repositorios, migraciones), `JwtTokenService`, BCrypt, **`PaymentClient`** (HTTP), `DbInitializer` | EF Core (`PaymentDbContext`, `PaymentRepository`, migraciones) |
| **Api** | Controllers, middleware de errores, JWT, Swagger, registro del `HttpClient` + Polly | `PaymentsController`, middleware de errores, validación JWT, Swagger |

Paquetes EF Core en **8.0.11** en ambos proyectos.

## 10. Migraciones

| Proyecto | Migración | Qué genera |
|----------|-----------|-----------|
| E-commerce | `InitialCreate` | Tabla `Products` |
| E-commerce | `AddAuthAndOrders` | Tablas `Categories`, `Users` (email único), `Orders`, `OrderItems` |
| E-commerce | `AddPaymentInfoToOrders` | Columnas `TransactionId`, `PaymentProcessedAt` en `Orders` + índice `IX_Orders_UserId` |
| PaymentService | `InitialCreate` | Tabla `Payments` (índice único en `OrderId`, columnas `Amount`/`Currency` del VO Money) |

Crear una migración nueva (ejemplo e-commerce):
```bash
dotnet ef migrations add NombreMigracion --project src/MiApp.Infrastructure --startup-project src/MiApp.WebApi
```
PaymentService:
```bash
dotnet ef migrations add NombreMigracion --project PaymentService/src/PaymentService.Infrastructure --startup-project PaymentService/src/PaymentService.Api
```

## 11. Notas y limitaciones

- Las soluciones están en formato `.sln` (compatible con el SDK de .NET 8; el formato `.slnx` necesita SDK 9+).
- La entidad `Category` existe en el dominio y en la base (ejercicio de la Unidad 4) pero ningún endpoint la usa todavía.
- MediatR 14 requiere licencia para uso comercial; para este trabajo académico no aplica.
- Si tenés una `miapp.db` vieja de antes del trabajo final y algo no cuadra, borrala: se vuelve a crear sola al arrancar.
