# ServiceDesk API (server)

## Назначение серверной части
Серверная часть реализует API для аутентификации и авторизации пользователей в системе ServiceDesk на базе ASP.NET Core 8, Identity, JWT и EF Core.

## Что реально реализовано
- ASP.NET Core Web API (`net8.0`).
- Подключен `AppDbContext` (наследник `IdentityDbContext<ApplicationUser>`).
- Подключен ASP.NET Identity с пользователем `ApplicationUser` (добавлено поле `DisplayName`).
- JWT-аутентификация (Bearer token) и авторизация по ролям.
- Глобальная обработка ошибок через `ExceptionMiddleware` с ответом в формате `application/problem+json`.
- Swagger в режиме `Development`.
- Инициализация ролей (`Student`, `Operator`, `Admin`) при старте.
- Seed demo-пользователей в режиме `Development` (`DbInitializer`).
- Health-check endpoint: `GET /health`.

## Структура проекта
```text
server/
  ServiceDesk.sln
  ServiceDesk.API/
    Application/Services/
      AuthService.cs
      IAuthService.cs
    Controllers/
      AuthController.cs
    Domain/
      ApplicationUser.cs
    DTOs/Auth/
      RegisterRequest.cs
      LoginRequest.cs
      AuthResponse.cs
      MeResponse.cs
    Exceptions/
      BusinessException.cs
      UnauthorizedException.cs
    Infrastructure/
      Auth/
        TokenService.cs
        ITokenService.cs
      Data/
        AppDbContext.cs
      Seed/
        DbInitializer.cs
    Middleware/
      ExceptionMiddleware.cs
    Migrations/
      20260329053136_InitialCreate.cs
    Program.cs
    appsettings.json
    appsettings.Development.json
```

## Требования для запуска
- .NET SDK 8.
- SQL Server (`UseSqlServer(...)` с `ConnectionStrings:DefaultConnection`).
- Доступная строка подключения в `appsettings.Development.json` или через переопределение конфигурации.
- Для применения миграций через CLI: `dotnet-ef` (если команда `dotnet ef ...` недоступна, нужно установить инструмент).

## Конфигурация (appsettings, connection string, JWT)

### Connection strings
В `appsettings.Development.json` есть:
- `ConnectionStrings:DefaultConnection` (используется приложением).
- `ConnectionStrings:DefaultConnection_2` (PostgreSQL, в текущем коде не используется).

Фактически в `Program.cs` активен только:
```csharp
options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"));
```

### JWT
Используются параметры:
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiresMinutes`

Источник ключа:
1. `Jwt:Key` из конфигурации.
2. Если пусто, переменная окружения `JWT_KEY`.
3. Если ни один вариант не задан, приложение выбрасывает `InvalidOperationException`.

`appsettings.json` содержит пустой `Jwt:Key`, а `appsettings.Development.json` содержит dev-ключ.

Также при старте дополнительно валидируются:
- `Jwt:Issuer`
- `Jwt:Audience`
- минимальная длина `Jwt:Key` — 32 символа.

## Реально существующие auth endpoints
Базовый префикс контроллера: `/api/auth`.

1. `POST /api/auth/register`
- Тело: `email`, `password`, `displayName`.
- Создает пользователя, назначает роль `Student`, возвращает JWT (`accessToken`).

2. `POST /api/auth/login`
- Тело: `email`, `password`.
- Проверяет учетные данные, возвращает JWT (`accessToken`).

3. `GET /api/auth/me`
- Требует Bearer JWT (`[Authorize]`).
- Возвращает профиль текущего пользователя: `id`, `email`, `displayName`, `role`.

Дополнительно:
4. `GET /health`
- Простой health endpoint, возвращает `"Healthy"`.

## Пошаговый локальный запуск
1. Перейти в API-проект:
```bash
cd server/ServiceDesk.API
```
2. Восстановить пакеты:
```bash
dotnet restore
```
3. Проверить `appsettings.Development.json`:
- `ConnectionStrings:DefaultConnection`
- `Jwt:Key` (или задать `JWT_KEY` в окружении)
4. Применить миграции к БД:
```bash
dotnet ef database update
```
5. Запустить сервер:
```bash
dotnet run
```
6. Проверить:
- API: `http://localhost:5051`
- Swagger (Development): `http://localhost:5051/swagger`
- Health: `http://localhost:5051/health`

## Текущее состояние проекта
- На данный момент реализован только модуль аутентификации/авторизации и health-check.
- `AppDbContext` пока не содержит бизнес-сущностей ServiceDesk, только Identity-модель.
- Большая часть предметной логики ServiceDesk не реализована.

### Demo-пользователи (создаются в Development через `DbInitializer`)
- `admin@demo.com / Admin123!` (Admin)
- `operator1@demo.com / Operator123!` (Operator)
- `operator2@demo.com / Operator123!` (Operator)
- `student1@demo.com / Student123!` (Student)
- `student2@demo.com / Student123!` (Student)
- `student3@demo.com / Student123!` (Student)
