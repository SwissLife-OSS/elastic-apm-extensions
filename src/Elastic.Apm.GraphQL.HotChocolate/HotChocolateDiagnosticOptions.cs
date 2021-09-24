using System;
using Elastic.Apm.Api;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    public class HotChocolateDiagnosticOptions
    {
        /// <summary>
        /// Gives the possibility to enrich transaction data just before <see cref="RequestActivityScope"/> gets disposed.
        /// </summary>
        public Action<ITransaction, OperationDetails>? Enrich { get; set; }
    }
}
