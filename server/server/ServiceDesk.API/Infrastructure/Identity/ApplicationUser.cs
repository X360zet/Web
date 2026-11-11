using Microsoft.AspNetCore.Identity;

namespace ServiceDesk.API.Infrastructure.Identity; 

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = default!;
}