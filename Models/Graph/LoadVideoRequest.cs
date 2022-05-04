using Microsoft.AspNetCore.Http;
using System;

namespace Interchoice.Models.Graph
{
    public class LoadVideoRequest
    {
        public Guid Id { get; set; }

        public IFormFile VideoFile { get; set; }
    }
}
