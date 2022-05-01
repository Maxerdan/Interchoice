using Interchoice.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Net;

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
        public class FileUploadAPI
        {
            public IFormFile file { get; set; }
        }

        [HttpPost("Test")]
        public IActionResult Test(FileUploadAPI formFile)
        {
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                context.ProjectsInfo = context.Set<ProjectInfo>();
                context.ProjectsInfo.Add(new ProjectInfo() { UserId = "1", Name = "name", FullDescription = "full", ShortDescription = "short", Overview = formFile.file.Name });
                context.SaveChanges();
                return Ok();
            }
        }

        [Authorize]
        [HttpGet("Test")]
        public IActionResult Test()
        {
            var smt = GetValue(HttpContext.User, ClaimTypes.Name);

            return Ok($"{smt}");
        }

        /// <summary>
        /// Create project handle
        /// </summary>
        /// <param name="projectModel"></param>
        /// <returns></returns>
        /// <response code="200 (4)">Successful created project</response>
        /// <response code="403 (140)">Exception message</response>
        [Authorize]
        [EnableCors]
        [HttpPost("CreateProject")]
        public async Task<IActionResult> CreateProject(CreateProjectModel projectModel)
        {
            try
            {
                var email = GetValue(HttpContext.User, ClaimTypes.Name);
                var emailName = email.Split('@').First();
                var userFolderName = $"\\{emailName}\\";
                var projectName = $"{projectModel.Name}\\";
                var user = await _userManager.FindByEmailAsync(email);
                if (!Directory.Exists(Directory.GetCurrentDirectory() + userFolderName))
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + userFolderName);
                if (!Directory.Exists(Directory.GetCurrentDirectory() + userFolderName + projectName))
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + userFolderName + projectName);
                using (FileStream fileStream = System.IO.File.Create(Directory.GetCurrentDirectory() + userFolderName + projectName + projectModel.Overview.FileName))
                {
                    projectModel.Overview.CopyTo(fileStream);
                    fileStream.Flush();
                    using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
                    {
                        context.ProjectsInfo = context.Set<ProjectInfo>();
                        context.ProjectsInfo.Add(new ProjectInfo()
                        {
                            UserId = user.Id,
                            Name = projectModel.Name,
                            FullDescription = projectModel.FullDescription,
                            ShortDescription = projectModel.ShortDescription,
                            Overview = projectModel.Overview.Name
                        });
                        context.SaveChanges();
                    }
                    return Json(new TransportResult(4, $"Successful created project", true));
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(140, $"{ex.Message}", false));
            }
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="registerVm"></param>
        /// <returns></returns>
        /// <response code="403 (100)">'@email' is not valid email</response>  
        /// <response code="403 (110)">'@email' is already in use</response>  
        /// <response code="200 (1)">Successful register for: '@email'</response>  
        /// <response code="403 (120)">Something went wrong while saving user to database: '@email'</response>  
        [EnableCors]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterViewModel registerVm)
        {
            if (!IsValidEmailAddress(registerVm.Email))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(100, $"'{registerVm.Email}' is not valid email", false));
            }

            var users = _userManager.Users.ToList();
            // check for same email
            if (users.Any(x => x.Email == registerVm.Email))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(110, $"'{registerVm.Email}' is already in use", false));
            }

            var user = new User() { UserName = registerVm.Email, Email = registerVm.Email, EmailConfirmed = true, FirstName = registerVm.FirstName, LastName = registerVm.LastName, BirthDate = registerVm.BirthDate, Country = registerVm.Country };
            var result = await _userManager.CreateAsync(user, registerVm.Password);
            if (result.Succeeded)
                return Json(new TransportResult(1, $"Successful register for: '{user.UserName}'", true));
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(120, $"Something went wrong while saving user to database: '{user.Email}' \n{string.Join("\n", result.Errors.Select(x => x.Description))}", false));
            }
        }

        /// <summary>
        /// Login user using credentionals: password and email
        /// </summary>
        /// <param name="loginVm"></param>
        /// <returns></returns>
        /// <response code="200 (2)">Successful login, returns token in value field</response>  
        /// <response code="403 (130)">Email or password is incorrect</response>  
        [EnableCors]
        [HttpPost("Login")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] LoginViewModel loginVm)
        {
            var user = await _userManager.FindByEmailAsync(loginVm.Email);

            var result = await _signInManager.PasswordSignInAsync(loginVm.Email, loginVm.Password, false, false);
            if (!result.Succeeded)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(130, $"Email or password is incorrect", false));
            }

            // authentication successful so generate jwt token
            user.JwtToken = GenerateJwtToken(user);
            await _userManager.UpdateAsync(user);
            await Authenticate(loginVm.Email);

            return Json(new TransportResult(2, "", true, user.JwtToken));
        }

        /// <summary>
        /// Logout handler
        /// </summary>
        /// <returns></returns>
        /// <response code="200 (3)">Logout complete</response>
        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Json(new TransportResult(3, "Logout complete", true));
        }

        private bool IsValidEmailAddress(string email)
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

        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        private string GetValue(ClaimsPrincipal principal, string key)
        {
            if (principal == null)
                return string.Empty;

            return principal.FindFirstValue(key);
        }
    }
}
