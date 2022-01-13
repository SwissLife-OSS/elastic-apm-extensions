using System.Collections.Generic;
using Elastic.Apm.Api;
using MassTransit;

namespace Elastic.Apm.Messaging.MassTransit
{
    internal static class MassTransitExtensions
    {
        private static readonly Dictionary<string, string> SchemeToSubType = new()
        {
            { "sb", "azureservicebus" }
        };

        internal static void SetTracingData(this SendContext context, ISpan span)
        {
            var tracingData = span.OutgoingDistributedTracingData.SerializeToString();
            context.Headers.Set(
                Constants.TraceHeaderName,
                tracingData);

            context.Headers.Set(
                Constants.MessageSourceHeaderName,
                context.DestinationAddress.AbsolutePath);
        }

        internal static DistributedTracingData? GetTracingData(this ReceiveContext context)
        {
            var tracingData = context.TransportHeaders.Get<string>(Constants.TraceHeaderName);
            return DistributedTracingData.TryDeserializeFromString(tracingData);
        }

        internal static string GetMessageSource(this ReceiveContext context)
        {
            return context.TransportHeaders.Get<string>(Constants.MessageSourceHeaderName);
        }

        internal static string GetSpanSubType(this SendContext context)
        {
            var scheme = context.DestinationAddress.Scheme;

            return SchemeToSubType.TryGetValue(scheme, out var value) ? value : scheme;
        }
    }
}
