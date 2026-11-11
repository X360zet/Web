namespace ServiceDesk.API.Application.DTOs;

public class LoginResultDto
{
    public required string AccessToken { get; set; }
    public required UserInfoDto User { get; set; }
}