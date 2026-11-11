using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceDesk.API.Application.Services;
using ServiceDesk.API.DTOs.Categories;

namespace ServiceDesk.API.Controllers;

[ApiController, Route("api/categories"), Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService service, ILogger<CategoriesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Получить список категорий
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        try
        {
            // Передаем роль пользователя, чтобы сервис мог фильтровать данные
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";
            var list = await _service.GetAllAsync(includeInactive, role);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка категорий");
            return StatusCode(500, new ProblemDetails 
            { 
                Title = "Internal Server Error",
                Detail = "Не удалось загрузить категории"
            });
        }
    }

    /// <summary>
    /// Создать категорию (Только Admin)
    /// </summary>
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ProblemDetails 
            { 
                Title = "Validation Failed",
                Detail = "Некорректные данные запроса",
                Extensions = { ["errors"] = ModelState }
            });

        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            // Дубликат имени или другая бизнес-ошибка
            _logger.LogWarning("Ошибка создания категории: {Message}", ex.Message);
            return BadRequest(new ProblemDetails 
            { 
                Title = "Validation Failed",
                Detail = ex.Message
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка базы данных при создании категории");
            return Conflict(new ProblemDetails 
            { 
                Title = "Conflict",
                Detail = "Категория с таким именем уже существует"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при создании категории");
            return StatusCode(500, new ProblemDetails 
            { 
                Title = "Internal Server Error",
                Detail = "Произошла внутренняя ошибка сервера"
            });
        }
    }

    /// <summary>
    /// Обновить категорию (Только Admin)
    /// </summary>
    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ProblemDetails 
            { 
                Title = "Validation Failed",
                Detail = "Некорректные данные запроса",
                Extensions = { ["errors"] = ModelState }
            });

        try
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Категория {Id} не найдена", id);
            return NotFound(new ProblemDetails 
            { 
                Title = "Not Found",
                Detail = $"Категория с id {id} не найдена"
            });
        }
        catch (InvalidOperationException ex)
        {
            // Дубликат имени
            _logger.LogWarning("Ошибка обновления категории {Id}: {Message}", id, ex.Message);
            return BadRequest(new ProblemDetails 
            { 
                Title = "Validation Failed",
                Detail = ex.Message
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка базы данных при обновлении категории {Id}", id);
            return Conflict(new ProblemDetails 
            { 
                Title = "Conflict",
                Detail = "Не удалось обновить категорию: возможно, имя уже занято"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при обновлении категории {Id}", id);
            return StatusCode(500, new ProblemDetails 
            { 
                Title = "Internal Server Error",
                Detail = "Произошла внутренняя ошибка сервера"
            });
        }
    }

    /// <summary>
    /// Удалить категорию (Только Admin)
    /// </summary>
 [HttpDelete("{id}"), Authorize(Roles = "Admin")]
public async Task<IActionResult> Delete(int id)
{
    try
    {
        await _service.DeleteAsync(id);
        return NoContent(); // 204 - успешно, без контента
    }
    catch (KeyNotFoundException)
    {
        _logger.LogWarning("Категория {Id} не найдена при удалении", id);
        return NotFound(new ProblemDetails 
        { 
            Title = "Not Found",
            Detail = $"Категория с id {id} не найдена"  // ← Исправлено: id с маленькой буквы
        });
    }
    catch (InvalidOperationException ex)
    {
        // Нельзя удалить, т.к. есть связанные заявки
        _logger.LogWarning("Не удалось удалить категорию {Id}: {Message}", id, ex.Message);
        return BadRequest(new ProblemDetails 
        { 
            Title = "Cannot Delete",
            Detail = ex.Message
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Непредвиденная ошибка при удалении категории {Id}", id);
        return StatusCode(500, new ProblemDetails 
        { 
            Title = "Internal Server Error",
            Detail = "Произошла внутренняя ошибка сервера"
        });
    }
}
    [HttpPatch("{id}/active"), Authorize(Roles = "Admin")]
public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveCategoryRequest request)
{
    try
    {
        var result = await _service.SetActiveAsync(id, request);  // ← Передаём request (DTO)
        return Ok(result);
    }
    catch (KeyNotFoundException)
    {
        return NotFound(new ProblemDetails 
        { 
            Title = "Category not found",
            Detail = $"Категория с id {id} не найдена"
        });
    }
}
}