using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace Interchoice.Models.Graph
{
    public class NodeSummary
    {
        private readonly HttpContext _httpContext;
        public NodeSummary(Guid id, HttpContext httpContext)
        {
            Id = id;
            _httpContext = httpContext;

            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var email = GetValue(_httpContext.User, ClaimTypes.Name);
                var emailName = email.Split('@').First();
                var userFolderName = $"/{emailName}/";
                var project = context.ProjectsInfo.Where(x => x.NodesId != null).ToList().Where(x => x.NodesId.Contains(id.ToString())).First();
                var projectName = $"{project.ProjectId}/";
                var foundNode = context.Nodes.Find(id);
                if (foundNode is null)
                    return;
                if (string.IsNullOrEmpty(foundNode.VideoFileName))
                    VideoUrl = "";
                else
                    VideoUrl = Constants.Https + userFolderName + projectName + foundNode.VideoFileName;
                if(foundNode.ParentGuids != "")
                ParentGuids = foundNode.ParentGuids?.Split("\n").Select(x=>new Guid(x)).ToList();
                if (foundNode.ChildGuids != "")
                    ChildGuids = foundNode.ChildGuids?.Split("\n").Select(x => new Guid(x)).ToList();
                if(ParentGuids is null)
                    ParentGuids = new List<Guid>();
                if(ChildGuids is null)
                    ChildGuids = new List<Guid>();
                Name = foundNode.Name;
                Description = foundNode.Description;
                ButtonName = foundNode.ButtonName;
                X = foundNode.X;
                Y = foundNode.Y;
            }
        }

        public Guid Id { get; set; }

        public List<Guid> ParentGuids { get; set; }

        public List<Guid> ChildGuids { get; set; }

        public string Name { get; set; }

        public string VideoUrl { get; set; }

        public string Description { get; set; }

        public string ButtonName { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        private string GetValue(ClaimsPrincipal principal, string key)
        {
            if (principal == null)
                return string.Empty;

            return principal.FindFirstValue(key);
        }
    }
}
