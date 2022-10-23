using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace Interchoice.Models.Graph
{
    public class NodeSummary
    {
        public NodeSummary(Guid id, string userId)
        {
            Id = id;

            using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
            {
                var user = context.Users.Find(userId);
                var email = user.Email;
                var emailName = email.Split('@').First();
                var userFolderName = $"{emailName}";
                var project = context.ProjectsInfo.Where(x => x.NodesId != null).ToList().Where(x => x.NodesId.Contains(id.ToString())).First();
                var projectName = $"{project.ProjectId}";
                var foundNode = context.Nodes.Find(id);
                if (foundNode is null)
                    return;
                if (string.IsNullOrEmpty(foundNode.VideoFileName))
                    VideoUrl = "";
                else
                    VideoUrl = Path.Combine(Constants.Https, userFolderName, projectName, foundNode.VideoFileName);
                if(foundNode.ParentGuids != "")
                    ParentGuids = foundNode.ParentGuids?.Split("\n").Where(x=>!string.IsNullOrEmpty(x)).Select(x=>new Guid(x)).ToList();
                if (foundNode.ChildGuids != "")
                    ChildGuids = foundNode.ChildGuids?.Split("\n").Where(x => !string.IsNullOrEmpty(x)).Select(x => new Guid(x)).ToList();
                if(ParentGuids is null)
                    ParentGuids = new List<Guid>();
                if(ChildGuids is null)
                    ChildGuids = new List<Guid>();
                Name = foundNode.Name;
                Description = foundNode.Description;
                ButtonName = foundNode.ButtonName;
                Question = foundNode.Question;
                IsBeginning = foundNode.IsBeginning;
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

        public string? Question { get; set; }

        public bool IsBeginning { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
