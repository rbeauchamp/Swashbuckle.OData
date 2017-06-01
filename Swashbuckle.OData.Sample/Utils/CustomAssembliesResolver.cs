using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace SwashbuckleODataSample.Utils
{
    /// <summary>
    /// Custom Assemblies Resolver, using the default assemblies resolver, only for demonstration purposes
    /// </summary>
    public class CustomAssembliesResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            ICollection<Assembly> baseAssemblies = base.GetAssemblies();
            return baseAssemblies;
        }
    }
}