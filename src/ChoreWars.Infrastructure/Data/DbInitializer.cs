using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ChoreWars.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Create DungeonMaster role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("DungeonMaster"))
        {
            await roleManager.CreateAsync(new IdentityRole("DungeonMaster"));
        }
    }
}
