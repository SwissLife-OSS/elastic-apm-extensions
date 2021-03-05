using Elastic.Apm.Api;
using MassTransit;

namespace Elastic.Apm.Messaging.MassTransit
{
    internal static class MassTransitExtensions
    {
        internal static void SetTracingData(this SendContext context, ISpan span)
        {
            var tracingData = span.OutgoingDistributedTracingData.SerializeToString();
            context.Headers.Set(Constants.TraceHeaderName, tracingData);
        }

        internal static DistributedTracingData? GetTracingData(this ReceiveContext context)
        {
            var tracingData = context.TransportHeaders.Get<string>(Constants.TraceHeaderName);
            return DistributedTracingData.TryDeserializeFromString(tracingData);
        }
    }
}
