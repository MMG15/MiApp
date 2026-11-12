using System.Reflection;
using System.Text;
using MiApp.Application;
using MiApp.Application.Contracts.Infrastructure;
using MiApp.Infrastructure;
using MiApp.Infrastructure.Persistence.Seed;
using MiApp.Infrastructure.Services.Payments;
using MiApp.WebApi.Http;
using MiApp.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// ───────────── FASE 1: registro de servicios (DI) ─────────────

// Capas propias: cada una registra lo suyo con un extension method
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── JWT (autenticación) ──
// El secreto NO está en appsettings.json: en desarrollo viene de appsettings.Development.json
// y en producción de la variable de entorno JwtSettings__Secret.
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException(
        "Falta JwtSettings:Secret (mínimo 32 caracteres). Configuralo en appsettings.Development.json " +
        "o con la variable de entorno JwtSettings__Secret.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Comunicación con PaymentService (typed client + IHttpClientFactory) ──
// La URL sale de appsettings ("Services:Payment"): si cambia el puerto, se cambia ahí.
var paymentServiceUrl = builder.Configuration["Services:Payment"]
    ?? throw new InvalidOperationException("Falta la configuración Services:Payment.");

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenForwardingHandler>();

builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(client =>
    {
        client.BaseAddress = new Uri(paymentServiceUrl);
        client.Timeout = TimeSpan.FromSeconds(10);           // tope total, con reintentos incluidos
    })
    .AddHttpMessageHandler<BearerTokenForwardingHandler>()   // reenvía el JWT del usuario
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()                          // errores de red, 5xx y 408
        .Or<Polly.Timeout.TimeoutRejectedException>()
        .WaitAndRetryAsync(2, intento => TimeSpan.FromMilliseconds(300 * intento)))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3))); // por intento

builder.Services.AddTransient<GlobalExceptionHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MiApp — E-commerce API",
        Version = "v1",
        Description = "Backend 2026 — Trabajo Final. Se comunica con PaymentService por HTTP."
    });

    // Muestra en Swagger los comentarios /// de los controllers
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Pegá solo el token JWT (sin la palabra 'Bearer').",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Aplica migraciones pendientes y crea el Admin inicial (SeedAdmin)
await DbInitializer.InitializeAsync(app.Services);

// ───────────── FASE 2: pipeline de middleware (el orden importa) ─────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandler>();   // atrapa las excepciones y las traduce a HTTP
app.UseAuthentication();                       // ¿quién sos? (lee y valida el JWT)
app.UseAuthorization();                        // ¿qué podés hacer? ([Authorize], roles)
app.MapControllers();

app.Run();
