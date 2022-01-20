using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        internal static void SetTracingData(this SendContext context, ISpan span, bool isResponse)
        {
            var tracingData = span.OutgoingDistributedTracingData.SerializeToString();
            context.Headers.Set(
                Constants.TraceHeader,
                tracingData);
            context.Headers.Set(
                Constants.MessageSourceHeader,
                context.DestinationAddress.AbsolutePath);
            context.Headers.Set(
                Constants.ReceiveResponseHeader,
                $"{isResponse}",
                true);

            if (context.Headers.TryGetHeader(Constants.AcceptTypeHeader, out var value) &&
                value is IList<string> values)
            {
                var acceptType = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(acceptType) &&
                    Uri.TryCreate(acceptType, UriKind.RelativeOrAbsolute, out Uri? acceptTypeUrn))
                {
                    context.Headers.Set(Constants.MessageResponseHeader, acceptTypeUrn.AbsolutePath);
                }
            }

            if (isResponse)
            {
                context.Headers.Set(
                    Constants.MessageResponseHeader,
                    context.Headers.Get<string>(Constants.MessageResponseHeader),
                    true);
            }
        }

        internal static DistributedTracingData? GetTracingData(this ReceiveContext context)
        {
            var tracingData = context.TransportHeaders.Get<string>(Constants.TraceHeader);
            return DistributedTracingData.TryDeserializeFromString(tracingData);
        }

        internal static bool WaitForResponse(this ReceiveContext context)
        {
            return context.TransportHeaders.TryGetHeader(Constants.MessageResponseHeader, out _);
        }

        internal static bool TryGetMessageResponse(this Headers headers, [NotNullWhen(true)]out string? value)
        {
            value = default;
            var hasHeader = headers.TryGetHeader(Constants.MessageResponseHeader, out var rawValue);
            if (hasHeader)
            {
                value = rawValue as string;
            }

            return hasHeader;
        }

        internal static bool TryGetMessageResponse(this ReceiveContext context, [NotNullWhen(true)] out string? value)
        {
            value = default;
            var hasReceiveResponse = context.TransportHeaders.TryGetHeader(Constants.ReceiveResponseHeader, out var rawReceiveResponse);
            if (hasReceiveResponse &&
                rawReceiveResponse is string receiveResponse &&
                receiveResponse.Equals("True", StringComparison.InvariantCultureIgnoreCase))
            {
                return TryGetMessageResponse(context.TransportHeaders, out value);
            }

            return false;
        }

        internal static string GetMessageSource(this ReceiveContext context)
        {
            return context.TransportHeaders.Get<string>(Constants.MessageSourceHeader);
        }

        internal static string GetSpanSubType(this SendContext context)
        {
            var scheme = context.DestinationAddress.Scheme;

            return SchemeToSubType.TryGetValue(scheme, out var value) ? value : scheme;
        }

        internal static string GetSpanSubType(this ReceiveContext context)
        {
            var scheme = context.InputAddress.Scheme;

            return SchemeToSubType.TryGetValue(scheme, out var value) ? value : scheme;
        }

        internal static string GetAbsoluteName(this Uri address)
        {
            return address.AbsolutePath
                .AsSpan(1, address.AbsolutePath.Length - 1)
                .ToString();
        }

        internal static string GetInputAbsoluteName(this ReceiveContext context)
        {
            return context.InputAddress.AbsolutePath
                .AsSpan(1, context.InputAddress.AbsolutePath.Length - 1)
                .ToString();
        }
    }
}
