using HotChocolate.Execution.Instrumentation;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    public class EmptyActivityScope : IActivityScope
    {
        public void Dispose()
        {
        }
    }
}
