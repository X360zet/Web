namespace ServiceDesk.API.DTOs.Users;
public sealed class UserResponse
{
    public string Id { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
}