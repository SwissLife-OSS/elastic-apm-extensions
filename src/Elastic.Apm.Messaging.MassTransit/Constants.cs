namespace Elastic.Apm.Messaging.MassTransit
{
    internal struct Constants
    {
        internal const string TraceHeaderName = "Elastic.Apm";

        internal struct DiagnosticListener
        {
            internal const string Name = "MassTransit";
        }

        internal struct Events
        {
            internal const string SendStart = "MassTransit.Transport.Send.Start";
            internal const string SendStop = "MassTransit.Transport.Send.Stop";
            internal const string ReceiveStart = "MassTransit.Transport.Receive.Start";
            internal const string ReceiveStop = "MassTransit.Transport.Receive.Stop";
        }

        internal struct Apm
        {
            internal const string Type = "messaging";
            internal const string SendAction = "send";
        }
    }
}
