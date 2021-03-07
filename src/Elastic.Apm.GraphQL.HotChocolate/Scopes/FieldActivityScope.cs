using Elastic.Apm.Api;
using HotChocolate.Execution.Instrumentation;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal class FieldActivityScope : IActivityScope
    {
        private readonly ISpan _span;

        internal FieldActivityScope(ISpan span)
        {
            _span = span;
        }

        public void Dispose()
        {
            _span.End();
        }
    }
}
