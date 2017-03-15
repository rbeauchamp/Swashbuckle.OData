using System.Collections.Generic;

namespace SwashbuckleODataSample.ApiControllers
{
    /// <summary>
    /// Client Comment
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Client ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Client Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Client Projects
        /// </summary>
        public IList<Project> Projects { get; set; }
    }
}