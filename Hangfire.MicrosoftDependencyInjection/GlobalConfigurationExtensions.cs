using Hangfire.Annotations;
using System;

namespace Hangfire.MicrosoftDependencyInjection
{
    /// <summary>
    /// Global Configuration extensions
    /// </summary>
    public static class GlobalConfigurationExtensions
    {
        /// <summary>
        /// Tells global configuration to use the specified Service Provider container as a job activator.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>An instance of <see cref="IGlobalConfiguration{ServiceProviderJobActivator}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">configuration or container</exception>
        public static IGlobalConfiguration<ServiceProviderJobActivator> UseServiceProviderJobActivator([NotNull] this IGlobalConfiguration configuration, [NotNull] IServiceProvider serviceProvider)
        {
            if(configuration == null) throw new ArgumentNullException(nameof(configuration));
            if(serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            return configuration.UseActivator(new ServiceProviderJobActivator(serviceProvider));
        }
    }
}
