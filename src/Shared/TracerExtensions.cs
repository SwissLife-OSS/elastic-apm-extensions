using Elastic.Apm.Api;

namespace Elastic.Apm
{
    internal static class TracerExtensions
    {
        public static IExecutionSegment GetExecutionSegment(this ITracer tracer)
        {
            ITransaction transaction = tracer.CurrentTransaction;
            return tracer.CurrentSpan ?? (IExecutionSegment)transaction;
        }
    }
}
