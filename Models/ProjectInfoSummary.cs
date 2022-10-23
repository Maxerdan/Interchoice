using Interchoice.Models.Graph;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace Interchoice.Models
{
    public class ProjectInfoSummary
    {
        public ProjectInfoSummary(ProjectInfo projectInfo)
        {
            ProjectId = projectInfo.ProjectId;
            UserId = projectInfo.UserId;
            Name = projectInfo.Name;
            ShortDescription = projectInfo.ShortDescription;
            FullDescription = projectInfo.FullDescription;

            PreviewUrl = GetPreviewUrl(projectInfo);
            if (!string.IsNullOrEmpty(projectInfo.NodesId))
            {
                var nodesIds = projectInfo.NodesId.Split("\n").Select(x => new Guid(x)).ToList();
                List<NodeSummary> nodesSummary = new List<NodeSummary>();
                foreach (var nodeId in nodesIds)
                    nodesSummary.Add(new NodeSummary(nodeId, projectInfo.UserId));
                Nodes = nodesSummary;

                using (var context = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlServer(Startup._conStr).Options))
                {
                    var nodesFromProject = projectInfo.NodesId.Split('\n').Where(x => !string.IsNullOrEmpty(x)).Select(x => new Guid(x)).ToList();
                    var nodes = new List<Node>();
                    foreach(var nodeGuid in nodesFromProject)
                    {
                        var node = context.Nodes.Find(nodeGuid);
                        if(node.IsBeginning)
                            nodes.Add(node);
                    }
                    
                    if(nodes.Count != 0)
                        FirstNode = new NodeSummary(nodes.First().Id, projectInfo.UserId);
                    else if(nodes.Count == 0)
                    {
                        var nodeStringId = projectInfo.NodesId.Split("\n").FirstOrDefault(GetNode);

                        bool GetNode(string id)
                        {
                            var node = context.Nodes.Find(new Guid(id));
                            if (node is null)
                                return false;
                            return string.IsNullOrEmpty(node.ParentGuids);
                        }
                        if (nodeStringId != null)
                            FirstNode = new NodeSummary(new Guid(nodeStringId), projectInfo.UserId);
                    }
                }
            }
            else
            {
                Nodes = new List<NodeSummary>();
            }
        }

        public Guid ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string PreviewUrl { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public List<NodeSummary> Nodes { get; set; }
        public NodeSummary FirstNode { get; set; }

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
