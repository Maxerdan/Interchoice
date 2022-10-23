using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Interchoice.Models.Graph
{
    [Table("Node")]
    public class Node
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ParentGuids { get; set; }

        public string ChildGuids { get; set; }

        public string Name { get; set; }

        public string VideoFileName { get; set; }

        public string Description { get; set; }

        public string ButtonName { get; set; }

        public string? Question { get; set; }

        public bool IsBeginning { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
