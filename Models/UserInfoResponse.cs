namespace Interchoice.Models
{
    public class UserInfoResponse
    {
        public string JwtToken { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string BirthDate { get; set; } // 31.12.1999

        public string Country { get; set; }

        public string Email { get; set; }

        public UserInfoResponse(User user)
        {
            JwtToken = user.JwtToken;
            FirstName = user.FirstName;
            LastName = user.LastName;
            BirthDate = user.BirthDate;
            Country = user.Country;
            Email = user.Email;
        }
    }
}
