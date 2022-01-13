using System;
using MassTransit;

namespace Elastic.Apm.Messaging.MassTransit
{
    public class MassTransitDiagnosticOptions
    {
        private readonly Func<SendContext, string> _defaultSendLabel =
            context => context.DestinationAddress.AbsolutePath;

        private readonly Func<ReceiveContext, string> _defaultReceiveLabel =
            context => $"on {context.InputAddress.AbsolutePath} from {context.GetMessageSource()}";

        internal MassTransitDiagnosticOptions()
        {
            SendLabel = _defaultSendLabel;
            ReceiveLabel = _defaultReceiveLabel;
        }

        internal string GetSendLabel(SendContext context) =>
            GetLabel(context, SendLabel, _defaultSendLabel);

        internal string GetReceiveLabel(ReceiveContext context) =>
            GetLabel(context, ReceiveLabel, _defaultReceiveLabel);

        private string GetLabel<T>(
            T context,
            Func<T, string> userResolver,
            Func<T, string> defaultResolver)
        {
            var label = userResolver(context);

            if (string.IsNullOrEmpty(label))
            {
                label = defaultResolver(context);
            }

            return label;
        }

        /// <summary>
        /// Replace the default label for Send message.
        /// If the return value is empty or null, it will be replace with the default label.
        /// </summary>
        public Func<SendContext, string> SendLabel { get; set; }

        /// <summary>
        /// Replace the default label for Receive message.
        /// If the return value is empty or null, it will be replace with the default label.
        /// </summary>
        public Func<ReceiveContext, string> ReceiveLabel { get; set; }

        /// <summary>
        /// True if the receive transaction should has as a parent the send span and
        /// false if the receive transaction is a root transaction.
        /// Default: false.
        /// </summary>
        public bool InlineReceiveTransaction { get; set; } = false;

    }
}
