using Microsoft.AspNetCore.Http;
using System;

namespace Interchoice.Models.Graph
{

    public class EditNodeRequest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string ButtonName { get; set; }

        public string? Question { get; set; }

        public bool IsBeginning { get; set; }
    }
}
