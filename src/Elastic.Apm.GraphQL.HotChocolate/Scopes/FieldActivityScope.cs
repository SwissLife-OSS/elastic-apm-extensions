using System;
using Elastic.Apm.Api;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal class FieldActivityScope : IDisposable
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
