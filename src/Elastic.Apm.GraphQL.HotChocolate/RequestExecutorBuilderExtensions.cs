using System;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    /// <summary>
    /// Gives the possibility to enrich transaction data just before <see cref="RequestActivityScope"/> gets disposed.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="details"></param>
    public delegate void EnrichTransaction(ITransaction transaction, OperationDetails details);

    /// <summary>
    /// Report diagnostic events to Elastic <see cref="IApmAgent"/>
    /// </summary>
    public static class RequestExecutorBuilderExtensions
    {
        public static IRequestExecutorBuilder AddObservability(
            this IRequestExecutorBuilder builder,
            EnrichTransaction? enrich = default)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddDiagnosticEventListener(sp =>
            {
                IApmAgent apmAgent = sp.GetApplicationService<IApmAgent>();
                IConfigurationReader configuration = sp.GetApplicationService<IConfigurationReader>();
                return new HotChocolateDiagnosticListener(apmAgent, configuration, enrich);
            });
        }
    }
}
