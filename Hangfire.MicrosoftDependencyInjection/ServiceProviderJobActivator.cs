using System;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.MicrosoftDependencyInjection
{
    public class ServiceProviderJobActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public override object ActivateJob(Type jobType)
        {
            return _serviceProvider.GetService(jobType);
        }
        
        [Obsolete]
        public override JobActivatorScope BeginScope()
        {
            return new ServiceScopeDependencyScope(_serviceProvider.CreateScope());
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new ServiceScopeDependencyScope(_serviceProvider.CreateScope());
        }

        private class ServiceScopeDependencyScope : JobActivatorScope
        {
            private readonly IServiceScope _serviceScope;

            public ServiceScopeDependencyScope(IServiceScope serviceScope)
            {
                _serviceScope = serviceScope;
            }

            public override object Resolve(Type type)
            {
                return _serviceScope.ServiceProvider.GetService(type);
            }

            public override void DisposeScope()
            {
                _serviceScope.Dispose();
            }
        }
    }
}
