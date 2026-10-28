
using Microsoft.AspNetCore.Identity;

namespace GarageAI.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
