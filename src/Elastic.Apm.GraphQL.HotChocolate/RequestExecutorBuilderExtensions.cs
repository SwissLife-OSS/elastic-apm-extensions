using System;
using Elastic.Apm.Config;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    /// <summary>
    /// Report diagnostic events to Elastic <see cref="IApmAgent"/>
    /// </summary>
    public static class RequestExecutorBuilderExtensions
    {
        public static IRequestExecutorBuilder AddObservability(
            this IRequestExecutorBuilder builder,
            Action<HotChocolateDiagnosticOptions>? configure = default)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new HotChocolateDiagnosticOptions();
            configure?.Invoke(options);

            return builder.AddDiagnosticEventListener(sp =>
            {
                IApmAgent apmAgent = sp.GetApplicationService<IApmAgent>();
                IConfigurationReader configuration = sp.GetApplicationService<IConfigurationReader>();
                return new HotChocolateDiagnosticListener(apmAgent, configuration, options);
            });
        }
    }
}
