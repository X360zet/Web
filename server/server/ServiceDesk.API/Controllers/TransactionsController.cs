using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.API.Application.Services;
using ServiceDesk.API.DTOs.Operations;
using ServiceDesk.API.DTOs.Transactions;

namespace ServiceDesk.API.Controllers;

[ApiController, Route("api/transactions"), Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service) => _service = service;

    // 🔹 Получение списка транзакций (с фильтрацией очередей)
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] TransactionsQuery query)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User ID not found");
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        
        return Ok(await _service.GetListAsync(query, userId, role));
    }

    // 🔹 Получение транзакции по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User ID not found");
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        
        return Ok(await _service.GetByIdAsync(id, userId, role));
    }

    // 🔹 Создание транзакции (пользователь → статус Pending + создаётся операция на одобрение)
    [HttpPost, Authorize(Roles = "User")]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ProblemDetails 
            { 
                Title = "Validation Failed", 
                Detail = "Некорректные данные" 
            });

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("User ID not found");

            var result = await _service.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    // 🔹 Обновление транзакции
    [HttpPut("{id}"), Authorize(Roles = "User,Operator,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
            
            var result = await _service.UpdateAsync(id, request, userId, role);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Транзакция не найдена" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    // 🔹 Hard Delete (только для Администратора)
    [HttpDelete("admin/{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDelete(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            await _service.DeleteAsync(id, userId, "Admin");
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Транзакция не найдена" });
        }
    }

    // 🔹 Запрос на удаление (пользователь → оператор на одобрение)
    [HttpDelete("{id}"), Authorize(Roles = "User")]
    public async Task<IActionResult> RequestDelete(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var result = await _service.RequestDeleteAsync(id, userId);
            
            return Ok(new 
            { 
                message = "Запрос на удаление отправлен оператору на одобрение",
                operationId = result.Id,
                status = result.Status
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    // 🔹 Взять транзакцию на проверку (оператор)
    [HttpPost("{id}/assign"), Authorize(Roles = "Operator")]
    public async Task<IActionResult> AssignToMe(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var role = "Operator";
            
            return Ok(await _service.AssignToMeAsync(id, userId, role));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("уже взят") || ex.Message.Contains("already"))
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    // 🔹 Смена статуса транзакции (оператор)
    [HttpPost("{id}/status"), Authorize(Roles = "Operator")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var role = "Operator";
            
            return Ok(await _service.ChangeStatusAsync(id, request, userId, role));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }

    // 🔹 Отклонение транзакции (оператор)
    [HttpPost("{id}/reject"), Authorize(Roles = "Operator")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var role = "Operator";
            
            return Ok(await _service.RejectAsync(id, request, userId, role));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Validation Failed", Detail = ex.Message });
        }
    }
}