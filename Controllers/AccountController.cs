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
using Interchoice.Models.Graph;

namespace Interchoice.Controllers
{
    public class AccountController : Controller
    {
        private readonly string currentDirectory = Directory.GetCurrentDirectory() + $"\\ClientApp\\public";
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

        [HttpPost("Test2")]
        [EnableCors]
        public IActionResult Test2()
        {
            
            /*using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                context.ProjectsInfo = context.Set<ProjectInfo>();
                context.ProjectsInfo.Add(new ProjectInfo() { UserId = "1", Name = "name", FullDescription = "full", ShortDescription = "short", Overview = fileUploadAPI.file.Name });
                context.SaveChanges();
                return Ok();
            }*/
            if (HttpContext.Request.Form.Files[0] != null)
            {
                var file = HttpContext.Request.Form.Files[0];
                using (FileStream fs = new FileStream(currentDirectory + "\\" + file.FileName, FileMode.CreateNew, FileAccess.Write, FileShare.Write))
                {
                    file.CopyTo(fs);
                }
            }
            return Ok();
        }

        [Authorize]
        [EnableCors]
        [HttpGet("Test")]
        public IActionResult Test()
        {
            
            var smt = GetValue(HttpContext.User, ClaimTypes.Name);

            return Ok($"{smt}");
        }

        /// <summary>
        /// Remove connection between nodes from(parent) node id to (child) node id
        /// </summary>
        /// <param name="connectRequest"></param>
        /// <returns></returns>
        /// <response code="200 (11)">Successful remove connection between nodes</response>
        [Authorize]
        [EnableCors]
        [HttpGet("RemoveNodesConnection")]
        public async Task<IActionResult> RemoveNodesConnection(ConnectRequest connectRequest)
        {
            
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var foundParentNode = context.Nodes.Find(new Guid(connectRequest.FromId));
                if (!string.IsNullOrEmpty(foundParentNode.ChildGuids))
                    foundParentNode.ChildGuids = foundParentNode.ChildGuids.Replace($"{connectRequest.ToId}", "");
                context.Nodes.Update(foundParentNode);
                context.SaveChanges();

                var foundChildNode = context.Nodes.Find(new Guid(connectRequest.ToId));
                if (!string.IsNullOrEmpty(foundChildNode.ParentGuids))
                    foundChildNode.ParentGuids = foundChildNode.ParentGuids.Replace($"{connectRequest.FromId}", "");
                context.Nodes.Update(foundChildNode);
                context.SaveChanges();

                return Json(new TransportResult(11, $"Successful remove connection between nodes"));
            }
        }

        /// <summary>
        /// Connects nodes from(parent) node id to (child) node id
        /// </summary>
        /// <param name="connectRequest"></param>
        /// <returns></returns>
        /// <response code="200 (10)">Successful connect nodes</response>
        [Authorize]
        [EnableCors]
        [HttpGet("ConnectNodes")]
        public async Task<IActionResult> ConnectNodes(ConnectRequest connectRequest)
        {
            
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var foundParentNode = context.Nodes.Find(new Guid(connectRequest.FromId));
                if (string.IsNullOrEmpty(foundParentNode.ChildGuids))
                    foundParentNode.ChildGuids = connectRequest.ToId;
                else
                    foundParentNode.ChildGuids += $"\n{connectRequest.ToId}";
                context.Nodes.Update(foundParentNode);
                context.SaveChanges();

                var foundChildNode = context.Nodes.Find(new Guid(connectRequest.ToId));
                if (string.IsNullOrEmpty(foundChildNode.ParentGuids))
                    foundChildNode.ParentGuids = connectRequest.FromId;
                else
                    foundChildNode.ParentGuids += $"\n{connectRequest.FromId}";
                context.Nodes.Update(foundChildNode);
                context.SaveChanges();

                return Json(new TransportResult(10, $"Successful connect nodes"));
            }
        }


        /// <summary>
        /// Returns video url by node id
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <response code="200 (9)">VideoUrl</response>
        /// <response code="404 (190)">Node has no video file</response>
        [Authorize]
        [EnableCors]
        [HttpGet("scene/{id}/video")]
        public async Task<IActionResult> GetVideoUrl(Guid id)
        {
            
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var email = GetValue(HttpContext.User, ClaimTypes.Name);
                var emailName = email.Split('@').First();
                var userFolderName = $"\\{emailName}\\";
                var project = context.ProjectsInfo.Where(x => x.NodesId != null).ToList().Where(x => x.NodesId.Contains(id.ToString())).First();
                var projectName = $"{project.Name}\\";
                var foundNode = context.Nodes.Find(id);
                if (string.IsNullOrEmpty(foundNode.VideoFileName))
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new TransportResult(190, $"Node has no video file"));
                }
                var videoLocalUrl = $"https://localhost:5001" + userFolderName + projectName + foundNode.VideoFileName;
                return Json(new TransportResult(9, $"", videoLocalUrl));
            }
        }

        /// <summary>
        /// Removes node with id and all connections
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <response code="200 (8)">Successful deleted node</response>
        [Authorize]
        [EnableCors]
        [HttpDelete("RemoveNode")]
        public async Task<IActionResult> RemoveNode(Ids node)
        {
            
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var foundNode = context.Nodes.Find(new Guid(node.Id));
                context.Nodes.Remove(foundNode);
                context.SaveChanges();

                var nodes = context.Nodes.ToList();
                foreach (var n in nodes)
                {
                    if (n.ParentGuids != null && n.ParentGuids.Contains(node.Id))
                        n.ParentGuids.Replace(node.Id, "");
                    if (n.ChildGuids != null && n.ChildGuids.Contains(node.Id))
                        n.ChildGuids.Replace(node.Id, "");
                }
                context.UpdateRange(nodes);
                context.SaveChanges();
                return Json(new TransportResult(8, $"Successful deleted node"));
            }
        }

        /// <summary>
        /// loads video
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200 (11)">Successful load video</response>
        [Authorize]
        [EnableCors]
        [HttpPost("scene/{id}/video")]
        public async Task<IActionResult> LoadVideo(Guid id)
        {
            
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var email = GetValue(HttpContext.User, ClaimTypes.Name);
                var emailName = email.Split('@').First();
                var userFolderName = $"\\{emailName}\\";
                var project = context.ProjectsInfo.Where(x => x.NodesId != null).ToList().Where(x => x.NodesId.Contains(id.ToString())).First();
                var projectName = $"{project.Name}\\";
                var foundNode = context.Nodes.Find(id);

                if (HttpContext.Request.Form.Files[0] != null)
                {
                    var file = HttpContext.Request.Form.Files[0];
                    using (FileStream fileStream = System.IO.File.Create(currentDirectory + userFolderName + projectName + file.FileName))
                    {
                        foundNode.VideoFileName = file.FileName;
                        file.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }

                context.Nodes.Update(foundNode);
                context.SaveChanges();
                return Json(new TransportResult(11, $"Successful load video"));
            }
        }

        /// <summary>
        /// Removes video
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200 (12)">Successful removed video</response>
        [Authorize]
        [EnableCors]
        [HttpDelete("scene/{id}/video")]
        public async Task<IActionResult> RemoveVideo(Guid id)
        {
            
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var email = GetValue(HttpContext.User, ClaimTypes.Name);
                var emailName = email.Split('@').First();
                var userFolderName = $"\\{emailName}\\";
                var project = context.ProjectsInfo.Where(x => x.NodesId != null).ToList().Where(x => x.NodesId.Contains(id.ToString())).First();
                var projectName = $"{project.Name}\\";
                var foundNode = context.Nodes.Find(id);
                System.IO.File.Delete(currentDirectory + userFolderName + projectName + foundNode.VideoFileName);

                foundNode.VideoFileName = "";
                context.Nodes.Update(foundNode);
                context.SaveChanges();


                return Json(new TransportResult(12, $"Successful removed video"));
            }
        }

        /// <summary>
        /// Edit node in database
        /// </summary>
        /// <returns></returns>
        /// <response code="200 (7)">Successful edit node</response>
        [Authorize]
        [EnableCors]
        [HttpPut("scene/{id}/video")]
        public async Task<IActionResult> EditNode(Guid id, [FromBody]EditNodeRequest editNode)
        {
            
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var email = GetValue(HttpContext.User, ClaimTypes.Name);
                var emailName = email.Split('@').First();
                var userFolderName = $"\\{emailName}\\";
                var project = context.ProjectsInfo.Where(x => x.NodesId != null).ToList().Where(x => x.NodesId.Contains(id.ToString())).First();
                var projectName = $"{project.Name}\\";

                context.Nodes = context.Set<Node>();
                var foundNode = context.Nodes.Find(id);
                foundNode.Name = editNode.Name;
                foundNode.Description = editNode.Description;
                foundNode.ButtonName = editNode.ButtonName;

                context.Nodes.Update(foundNode);
                context.SaveChanges();
                return Json(new TransportResult(7, $"Successful edit node"));
            }
        }

        /// <summary>
        /// Create node, return id
        /// </summary>
        /// <returns></returns>
        /// <response code="200 (6)">Successful created node, return id</response>
        [Authorize]
        [EnableCors]
        [HttpGet("CreateNode")]
        public async Task<IActionResult> CreateNode(Ids projectsId)
        {
            
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var node = new Node();
                context.Nodes = context.Set<Node>();
                context.Nodes.Add(node);

                var project = context.ProjectsInfo.Find(new Guid(projectsId.Id));
                if (string.IsNullOrEmpty(project.NodesId))
                    project.NodesId = node.Id.ToString();
                else
                    project.NodesId += $"\n{node.Id}";
                context.ProjectsInfo.Update(project);

                context.SaveChanges();
                return Json(new TransportResult(6, $"Successful created node", node.Id));
            }
        }

        /// <summary>
        /// Returns user info
        /// </summary>
        /// <returns></returns>
        /// <response code="200 (5)">Successful return user info</response>
        /// <response code="403 (150)">No user found</response>
        [Authorize]
        [EnableCors]
        [HttpGet("UserInfo")]
        public async Task<IActionResult> UserInfo()
        {
            
            var email = GetValue(HttpContext.User, ClaimTypes.Name);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(150, $"No user found"));
            }

            return Json(new UserInfoResponse(user));
        }


        /// <summary>
        /// Create project handle
        /// </summary>
        /// <param name="projectModel"></param>
        /// <returns></returns>
        /// <response code="200 (4)">Successful created project, returned project id</response>
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
                if (!Directory.Exists(currentDirectory + userFolderName))
                    Directory.CreateDirectory(currentDirectory + userFolderName);
                if (!Directory.Exists(currentDirectory + userFolderName + projectName))
                    Directory.CreateDirectory(currentDirectory + userFolderName + projectName);
                using (FileStream fileStream = System.IO.File.Create(currentDirectory + userFolderName + projectName + projectModel.Overview.FileName))
                {
                    projectModel.Overview.CopyTo(fileStream);
                    fileStream.Flush();
                    using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
                    {
                        context.ProjectsInfo = context.Set<ProjectInfo>();
                        var project = new ProjectInfo()
                        {
                            UserId = user.Id,
                            Name = projectModel.Name,
                            FullDescription = projectModel.FullDescription,
                            ShortDescription = projectModel.ShortDescription,
                            Overview = projectModel.Overview.Name
                        };
                        context.ProjectsInfo.Add(project);


                        context.SaveChanges();
                        return Json(new TransportResult(4, $"Successful created project", project.ProjectId));
                    }
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(140, $"{ex.Message}"));
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
                return Json(new TransportResult(100, $"'{registerVm.Email}' is not valid email"));
            }

            var users = _userManager.Users.ToList();
            // check for same email
            if (users.Any(x => x.Email == registerVm.Email))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(110, $"'{registerVm.Email}' is already in use"));
            }

            var user = new User() { UserName = registerVm.Email, Email = registerVm.Email, EmailConfirmed = true, FirstName = registerVm.FirstName, LastName = registerVm.LastName, BirthDate = registerVm.BirthDate, Country = registerVm.Country };
            var result = await _userManager.CreateAsync(user, registerVm.Password);
            if (result.Succeeded)
                return Json(new TransportResult(1, $"Successful register for: '{user.UserName}'"));
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new TransportResult(120, $"Something went wrong while saving user to database: '{user.Email}' \n{string.Join("\n", result.Errors.Select(x => x.Description))}"));
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
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new TransportResult(130, $"Email or password is incorrect"));
            }

            // authentication successful so generate jwt token
            user.JwtToken = GenerateJwtToken(user);
            await _userManager.UpdateAsync(user);
            await Authenticate(loginVm.Email);

            return Json(new TransportResult(2, "", user.JwtToken));
        }

        /// <summary>
        /// Logout handler
        /// </summary>
        /// <returns></returns>
        /// <response code="200 (3)">Logout complete</response>
        [HttpGet("Logout")]
        [EnableCors]
        public async Task<IActionResult> Logout()
        {
            
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Json(new TransportResult(3, "Logout complete"));
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
