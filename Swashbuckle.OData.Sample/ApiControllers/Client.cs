using System.Collections.Generic;

namespace SwashbuckleODataSample.ApiControllers
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Project> Projects { get; set; }
    }
}