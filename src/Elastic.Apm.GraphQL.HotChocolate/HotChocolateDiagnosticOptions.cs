using Elastic.Apm.Api;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    /// <summary>
    /// Gives the possibility to enrich transaction data just before <see cref="RequestActivityScope"/> gets disposed.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="details"></param>
    public delegate void EnrichTransaction(ITransaction transaction, OperationDetails details);

    public class HotChocolateDiagnosticOptions
    {
        /// <inheritdoc cref="EnrichTransaction"/>
        public EnrichTransaction? Enrich { get; set; }
    }
}
