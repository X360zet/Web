using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.API.Application.Services;
using ServiceDesk.API.DTOs.Operations;

namespace ServiceDesk.API.Controllers;

[ApiController, Route("api/operations"), Authorize]
public class OperationsController : ControllerBase
{
    private readonly ITransactionOperationService _service;
    public OperationsController(ITransactionOperationService service) => _service = service;

    // Пользователь создаёт операцию (при создании/удалении транзакции)
    [HttpPost, Authorize(Roles = "User")]
    public async Task<IActionResult> CreateOperation([FromBody] TransactionOperationRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        var result = await _service.CreateOperationAsync(request, userId);
        return CreatedAtAction(nameof(GetOperationById), new { id = result.Id }, result);
    }

    // Оператор получает.pending операции
    [HttpGet("pending"), Authorize(Roles = "Operator")]
    public async Task<IActionResult> GetPendingOperations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var operatorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        var result = await _service.GetPendingOperationsAsync(operatorId, page, pageSize);
        return Ok(result);
    }

    // Оператор одобряет операцию
    [HttpPost("{id}/approve"), Authorize(Roles = "Operator")]
    public async Task<IActionResult> ApproveOperation(int id, [FromBody] ApproveOperationRequest? request = null)
    {
        var operatorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        var result = await _service.ApproveOperationAsync(id, operatorId, request);
        return Ok(result);
    }

    // Оператор отклоняет операцию
    [HttpPost("{id}/reject"), Authorize(Roles = "Operator")]
    public async Task<IActionResult> RejectOperation(int id, [FromBody] string reason)
    {
        var operatorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        var result = await _service.RejectOperationAsync(id, operatorId, reason);
        return Ok(result);
    }

    private IActionResult GetOperationById(int id) => NotFound(); // Заглушка
}