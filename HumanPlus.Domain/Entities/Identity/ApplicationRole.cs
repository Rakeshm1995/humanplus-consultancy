using Microsoft.AspNetCore.Identity;

namespace HumanPlus.Domain.Entities.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
