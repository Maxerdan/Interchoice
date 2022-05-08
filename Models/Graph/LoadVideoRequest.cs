using Microsoft.AspNetCore.Http;
using System;

namespace Interchoice.Models.Graph
{
    public class LoadVideoRequest
    {
        public IFormFile VideoFile { get; set; }
    }
}
