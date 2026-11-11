using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ServiceDesk.API.Application.Services;
using ServiceDesk.API.Infrastructure.Auth;
using ServiceDesk.API.Infrastructure.Data;
using ServiceDesk.API.Infrastructure.Identity;
using ServiceDesk.API.Infrastructure.Seed;
using ServiceDesk.API.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;  // ← Добавлено для ClaimTypes
using System.Text;
using System.Text.Json.Serialization;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 🔹 1. Конфигурация: безопасная загрузка для design-time
var env = builder.Environment;
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 🔹 2. Database (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    if (args.Contains("--design-time") || Environment.GetEnvironmentVariable("DOTNET_RUNNING_MIGRATIONS") == "true")
    {
        connectionString = "Server=localhost;Database=TempDb;Trusted_Connection=True;TrustServerCertificate=True;";
    }
    else
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
    builder.Services.AddScoped<ITransactionOperationService, TransactionOperationService>();

// 🔹 3. Identity — AddIdentityCore + Roles
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddSignInManager();

// 🔹 4. JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

if (jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
    
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        
        // 🔹 КРИТИЧНО: роль должна читаться из стандартного ClaimTypes.Role
        RoleClaimType = ClaimTypes.Role,
        
        // 🔹 Имя пользователя — из стандартного ClaimTypes.Name
        NameClaimType = ClaimTypes.Name
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "A valid Bearer token is required.",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsJsonAsync(problem);
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this action.",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
    };
});

builder.Services.AddAuthorization();

// 🔹 5. Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 🔹 6. Категории и пользователи (Лаба 4)
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();

// 🔹 7. Тикеты (подготовка к Лабе 5)
builder.Services.AddScoped<ITransactionService, TransactionService>();

// 🔹 8. Controllers + ProblemDetails
builder.Services.AddControllers()
    .AddJsonOptions(options =>  // ← ДОБАВЬТЕ ЭТОТ БЛОК
    {
        // 🔹 Сериализация enum как строк ("Expense", а не 0)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        
        // 🔹 Игнорировать null значения
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        // 🔹 Регистронезависимое сравнение имён свойств
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => new { field = e.Key, error = err.ErrorMessage }))
                .ToList();

            return new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path,
                Extensions = { ["errors"] = errors }
            });
        };
    });
    

// 🔹 9. Swagger (Development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HomeFinance API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token (without 'Bearer ' prefix)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// 🔹 10. Инициализация БД и ролей (только в рантайме)
if (!app.Environment.IsEnvironment("DesignTime"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var roleName in new[] { "User", "Operator", "Admin" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            if (app.Environment.IsDevelopment())
            {
                await DbInitializer.SeedAsync(services);
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}

// 🔹 11. Middleware
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 12. Обработка 403 → ProblemDetails
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == StatusCodes.Status403Forbidden && !context.Response.HasStarted)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("403 Forbidden: {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "You do not have permission to perform this action.",
            Instance = context.Request.Path
        });
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);
// 🔹 13. Partial class для тестов
public partial class Program { }