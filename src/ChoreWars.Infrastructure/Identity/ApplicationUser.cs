using Microsoft.AspNetCore.Identity;

namespace ChoreWars.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    // Link to our domain User entity
    public Guid? DomainUserId { get; set; }
}
