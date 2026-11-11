using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.API.Application.Services;

namespace ServiceDesk.API.Controllers; // ← Проверьте, что namespace совпадает

[ApiController, Route("api/users"), Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserAdminService _service;

    public UsersController(IUserAdminService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPut("{id}/role")]
    public async Task<IActionResult> ChangeRole(string id, [FromBody] string role)
    {
        await _service.ChangeRoleAsync(id, role);
        return NoContent();
    }
}