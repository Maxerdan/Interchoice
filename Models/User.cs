using Microsoft.AspNetCore.Identity;

namespace Interchoice.Models
{
    public class User : IdentityUser
    {
        public string JwtToken { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string BirthDate { get; set; } // 31.12.1999

        public string Country { get; set; }
    }
}
