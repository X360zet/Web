using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceDesk.API.Domain;
using ServiceDesk.API.Infrastructure.Data;
using ServiceDesk.API.Infrastructure.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDesk.API.Infrastructure.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<AppDbContext>();

        // 🔹 Тестовые пользователи (простые логины, без @)
        var users = new[]
        {
            new { UserName = "admin", Password = "admin123", Role = "Admin", DisplayName = "Admin User" },
            new { UserName = "operator", Password = "operator123", Role = "Operator", DisplayName = "Operator One" },
            new { UserName = "user", Password = "user123", Role = "User", DisplayName = "User One" } // 🔹 Role = "User" с большой буквы!
        };

        foreach (var userData in users)
        {
            // 🔹 Ищем по UserName, а не по Email
            var user = await userManager.FindByNameAsync(userData.UserName);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userData.UserName,      // 🔹 Логин
                    Email = $"{userData.UserName}@local", // 🔹 Email-заглушка (не используется для входа)
                    DisplayName = userData.DisplayName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, userData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userData.Role);
                }
            }
        }

        // 🔹 Seed категорий
        var categoryNames = new[] { "Еда", "Транспорт", "ЖКХ", "Развлечения", "Здоровье", "Одежда" };
        foreach (var name in categoryNames)
        {
            if (!await context.Categories.AnyAsync(c => c.Name == name))
            {
                await context.Categories.AddAsync(new Category { Name = name });
            }
        }
        await context.SaveChangesAsync();
if (!await context.Transactions.AnyAsync())
{
    var user = await context.Users.FirstAsync();
    var category = await context.Categories.FirstAsync();
    var opId = (await context.Users.FirstOrDefaultAsync(u => u.Email == "operator@local"))?.Id ?? user.Id;

    context.Transactions.AddRange(new List<Transaction>
    {
        // Свободные заявки (ожидают проверки)
        new() { Title = "Кофе на вынос", Description = "Свободная трата", Amount = 350, Type = TransactionType.Expense, Status = TransactionStatus.Pending, CategoryId = category.Id, UserId = user.Id },
        new() { Title = "Подписка Netflix", Description = "Свободная трата", Amount = 999, Type = TransactionType.Expense, Status = TransactionStatus.Pending, CategoryId = category.Id, UserId = user.Id },
        
        // Заявки на проверке (взяты оператором)
        new() { Title = "Бензин", Description = "На проверке", Amount = 2500, Type = TransactionType.Expense, Status = TransactionStatus.InProgress, CategoryId = category.Id, UserId = user.Id, AssigneeId = opId },
        
        // Одобрённые заявки (вместо Resolved используем Approved)
        new() { Title = "Продукты", Description = "Одобрено", Amount = 4200, Type = TransactionType.Expense, Status = TransactionStatus.Approved, CategoryId = category.Id, UserId = user.Id, AssigneeId = opId, ApprovedAt = DateTimeOffset.UtcNow },
        
        // Отклонённая заявка
        new() { Title = "Игровая консоль", Description = "Отклонено - дорого", Amount = 45000, Type = TransactionType.Expense, Status = TransactionStatus.Rejected, CategoryId = category.Id, UserId = user.Id, AssigneeId = opId, RejectionReason = "Превышает бюджет" }
    });
    await context.SaveChangesAsync();
}
    }
}