using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Interchoice.Models
{
    [Table("ProjectInfo")]
    public class ProjectInfo
    {
        [Key]
        public Guid ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string NodesId { get; set; }
    }
}
