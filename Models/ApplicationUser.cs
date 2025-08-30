using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FreedomITAS.Models
{
 
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Role { get; set; } // e.g., "Admin", "User"
    }
}
