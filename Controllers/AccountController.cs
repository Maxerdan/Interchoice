using Interchoice.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Interchoice.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public bool IsValidEmailAddress(string email)
        {
            try
            {
                var emailChecked = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="registerVm"></param>
        /// <returns></returns>
        /// <response code="100">'@email' is not valid email</response>  
        /// <response code="110">'@email' is already in use</response>  
        /// <response code="1">Successful register for: '@email'</response>  
        /// <response code="200">Something went wrong while saving user to database: '@email'</response>  
        [EnableCors]
        [HttpPost("Register")]
        [ProducesResponseType(100)]
        [ProducesResponseType(110)]
        [ProducesResponseType(1)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterViewModel registerVm)
        {
            if (!IsValidEmailAddress(registerVm.Email))
                return Json(new TransportResult(100, $"'{registerVm.Email}' is not valid email", false));

            var users = _userManager.Users.ToList();
            // check for same email
            if (users.Any(x => x.Email == registerVm.Email))
                return Json(new TransportResult(110, $"'{registerVm.Email}' is already in use", false));

            var user = new User() { UserName = registerVm.Email, Email = registerVm.Email, EmailConfirmed = true, FirstName = registerVm.FirstName, LastName = registerVm.LastName, BirthDate = registerVm.BirthDate, Country = registerVm.Country };
            var result = await _userManager.CreateAsync(user, registerVm.PasswordHash);
            if (result.Succeeded)
                return Json(new TransportResult(1, $"Successful register for: '{user.UserName}'", true));
            else
                return Json(new TransportResult(200, $"Something went wrong while saving user to database: '{user.Email}' \n{string.Join("\n", result.Errors.Select(x => x.Description))}", false));
        }


        /// <summary>
        /// Login user using credentionals: password and email
        /// </summary>
        /// <param name="loginVm"></param>
        /// <returns></returns>
        /// /// <response code="2">Returns token in value field</response>  
        /// /// <response code="120">Email or password is incorrect</response>  
        [EnableCors]
        [HttpPost("Login")]
        [ProducesResponseType(120)]
        public async Task<IActionResult> AuthenticateAsync([FromBody] LoginViewModel loginVm)
        {
            var user = await _userManager.FindByEmailAsync(loginVm.Email);

            var result = await _signInManager.PasswordSignInAsync(loginVm.Email, loginVm.PasswordHash, false, false);
            if (!result.Succeeded)
                return Json(new TransportResult(120, $"Email or password is incorrect",false));

            // authentication successful so generate jwt token
            user.JwtToken = GenerateJwtToken(user);
            await _userManager.UpdateAsync(user);

            return Json(new TransportResult(2,"", true, user.JwtToken));
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("THIS IS USED TO SIGN AND VERIFY JWT TOKENS, REPLACE IT WITH YOUR OWN SECRET, IT CAN BE ANY STRING");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
