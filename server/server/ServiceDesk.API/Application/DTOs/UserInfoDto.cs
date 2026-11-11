namespace ServiceDesk.API.Application.DTOs;

public class UserInfoDto
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
     public required string Email { get; set; }
    public required string Role { get; set; }
}