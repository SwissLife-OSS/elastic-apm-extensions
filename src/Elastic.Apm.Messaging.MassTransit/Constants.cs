namespace Elastic.Apm.Messaging.MassTransit
{
    internal struct Constants
    {
        internal const string TraceHeader = "Elastic.Apm";
        internal const string ReceiveResponseHeader = "Elastic.Apm.ReceiveResponse";
        internal const string MessageSourceHeader = "Elastic.Apm.MessageSource";
        internal const string MessageResponseHeader = "Elastic.Apm.MessageResponse";
        internal const string AcceptTypeHeader = "MT-Request-AcceptType";

        internal struct DiagnosticListener
        {
            internal const string Name = "MassTransit";
        }

        internal struct Events
        {
            internal const string SendStart = "MassTransit.Transport.Send.Start";
            internal const string SendStop = "MassTransit.Transport.Send.Stop";
            internal const string ConsumeStart = "MassTransit.Consumer.Consume.Start";
            internal const string ConsumeStop = "MassTransit.Consumer.Consume.Stop";
            internal const string ReceiveStart = "MassTransit.Transport.Receive.Start";
            internal const string ReceiveStop = "MassTransit.Transport.Receive.Stop";
        }

        internal struct Apm
        {
            internal const string Type = "messaging";
            internal const string SendAction = "send";
            internal const string ConsumeAction = "consume";
        }
    }
}
