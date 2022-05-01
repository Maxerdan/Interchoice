using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using static Interchoice.Controllers.AccountController;

namespace Interchoice.Models
{
    [Keyless]
    public class CreateProjectModel
    {
        public string Name { get; set; }
        public IFormFile Overview { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
    }
}
