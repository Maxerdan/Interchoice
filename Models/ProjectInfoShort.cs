using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace Interchoice.Models
{
    public class ProjectInfoShort
    {
        private readonly HttpContext _httpContext;

        public ProjectInfoShort(ProjectInfo projectInfo, HttpContext httpContext)
        {
            _httpContext = httpContext;
            ProjectId = projectInfo.ProjectId;
            UserId = projectInfo.UserId;
            Name = projectInfo.Name;
            ShortDescription = projectInfo.ShortDescription;
            FullDescription = projectInfo.FullDescription;
            PreviewUrl = GetPreviewUrl(projectInfo.Overview);
        }

        public Guid ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string PreviewUrl { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }

        private string GetPreviewUrl(string fileName)
        {
            var email = GetValue(_httpContext.User, ClaimTypes.Name);
            var emailName = email.Split('@').First();
            var userFolderName = $"/{emailName}/";
            var projectName = $"{ProjectId}/";

            return "https://localhost:5001" + userFolderName + projectName + fileName;
        }

        private string GetValue(ClaimsPrincipal principal, string key)
        {
            if (principal == null)
                return string.Empty;

            return principal.FindFirstValue(key);
        }
    }
}
