using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Swashbuckle.OData.Tests
{
    public class UnitTestControllerSelector : DefaultHttpControllerSelector
    {
        private readonly Type _targetController;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestControllerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="targetController">The controller being targeted in the unit test.</param>
        public UnitTestControllerSelector(HttpConfiguration configuration, Type targetController) : base(configuration)
        {
            _targetController = targetController;
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            if (_targetController != null)
            {
                return base.GetControllerMapping()
                    .Where(pair => pair.Value.ControllerType == _targetController)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            return new Dictionary<string, HttpControllerDescriptor>();
        }
    }
}