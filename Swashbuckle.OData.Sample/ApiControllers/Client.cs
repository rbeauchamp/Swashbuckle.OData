using System.Collections.Generic;

namespace SwashbuckleODataSample.Models
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Project> Projects { get; set; }
    }
}