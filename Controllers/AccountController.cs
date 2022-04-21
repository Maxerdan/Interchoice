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

        [EnableCors]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody]RegisterViewModel registerVm)
        {
            if(!IsValidEmailAddress(registerVm.Email))
                return BadRequest($"'{registerVm.Email}' is not valid email");

            var users = _userManager.Users.ToList();
            // check for same email
            if (users.Any(x => x.Email == registerVm.Email))
                return BadRequest(new { message = $"{registerVm.Email} is already in use" });

            var user = new User() { UserName = registerVm.Email, Email = registerVm.Email, EmailConfirmed = true, FirstName = registerVm.FirstName, LastName = registerVm.LastName, BirthDate = registerVm.BirthDate, Country = registerVm.Country };
            var result = await _userManager.CreateAsync(user, registerVm.PasswordHash);
            if (result.Succeeded)
                return Ok($"Great register for: {user.UserName}");
            else
                return BadRequest($"Something went wrong while saving: {user.Email} \n{string.Join("\n", result.Errors.Select(x => x.Description))}");
        }

        [EnableCors]
        [HttpPost("Login")]
        public async Task<IActionResult> AuthenticateAsync([FromBody]LoginViewModel loginVm)
        {
            var user = await _userManager.FindByEmailAsync(loginVm.Email);

            var result = await _signInManager.PasswordSignInAsync(loginVm.Email, loginVm.PasswordHash, false, false);
            if (!result.Succeeded)
                return BadRequest("Can't login");

            // authentication successful so generate jwt token
            user.JwtToken = GenerateJwtToken(user);
            await _userManager.UpdateAsync(user);

            return Ok(new JwtToken() { Token = user.JwtToken });
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
