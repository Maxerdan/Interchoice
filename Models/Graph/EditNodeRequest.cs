using Microsoft.AspNetCore.Http;
using System;

namespace Interchoice.Models.Graph
{

    public class EditNodeRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ButtonName { get; set; }
    }
}
