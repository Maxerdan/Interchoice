using System.ComponentModel.DataAnnotations;

namespace Interchoice.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string BirthDate { get; set; } // 31.12.1999

        [Required]
        public string Country { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PasswordHashCheck { get; set; }
    }
}
