using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Swashbuckle.OData.Tests
{
    public class UnitTestControllerSelector : DefaultHttpControllerSelector
    {
        private readonly Type[] _targetControllers;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestControllerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="targetControllers">The controller being targeted in the unit test.</param>
        public UnitTestControllerSelector(HttpConfiguration configuration, Type[] targetControllers) : base(configuration)
        {
            _targetControllers = targetControllers;
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            if (_targetControllers != null)
            {
                return base.GetControllerMapping()
                    .Where(pair => _targetControllers.Contains(pair.Value.ControllerType))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            return new Dictionary<string, HttpControllerDescriptor>();
        }
    }
}