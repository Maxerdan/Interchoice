using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Interchoice.Models
{
    public class ProjectInfoResponse
    {
        public ProjectInfoResponse(ProjectInfo projectInfo)
        {
            ProjectId = projectInfo.ProjectId;
            UserId = projectInfo.UserId;
            Name = projectInfo.Name;
            Overview = projectInfo.Overview;
            ShortDescription = projectInfo.ShortDescription;
            FullDescription = projectInfo.FullDescription;
            if (projectInfo.NodesId != null)
                Nodes = projectInfo.NodesId.Split("\n").Select(x => new Guid(x)).ToList();
            else
                Nodes = new List<Guid>();
        }

        public Guid ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public List<Guid> Nodes { get; set; }
    }
}
