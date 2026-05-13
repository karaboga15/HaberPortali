using Microsoft.AspNetCore.Identity;

namespace HaberPortali.API.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}