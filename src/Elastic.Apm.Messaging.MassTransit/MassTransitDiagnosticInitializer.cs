using System;
using System.Diagnostics;

namespace Elastic.Apm.Messaging.MassTransit
{
    internal class MassTransitDiagnosticInitializer : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly IApmAgent _apmAgent;
        private IDisposable? _sourceSubscription;

        internal MassTransitDiagnosticInitializer(IApmAgent apmAgent)
        {
            _apmAgent = apmAgent;
        }

        public void Dispose() => _sourceSubscription?.Dispose();
        
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (string.Equals(value.Name, Constants.DiagnosticListener.Name,
                StringComparison.InvariantCultureIgnoreCase))
            {
                _sourceSubscription = value.Subscribe(new MassTransitDiagnosticListener(_apmAgent));
            }
        }
    }
}
