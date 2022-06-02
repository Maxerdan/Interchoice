using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace Interchoice.Models
{
    public class ProjectInfoShort
    {
        public ProjectInfoShort(ProjectInfo projectInfo)
        {
            ProjectId = projectInfo.ProjectId;
            UserId = projectInfo.UserId;
            Name = projectInfo.Name;
            ShortDescription = projectInfo.ShortDescription;
            FullDescription = projectInfo.FullDescription;
            PreviewUrl = GetPreviewUrl(projectInfo);
        }

        public Guid ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string PreviewUrl { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }

        private string GetPreviewUrl(ProjectInfo projectInfo)
        {
            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var user = context.Users.Find(projectInfo.UserId);
                var email = user.Email;
                var emailName = email.Split('@').First();
                var userFolderName = $"{emailName}";
                var projectName = $"{ProjectId}";

                return Path.Combine(Constants.Https, userFolderName, projectName, projectInfo.Overview);
            }
        }
    }
}
