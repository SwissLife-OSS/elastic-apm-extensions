using System;
using System.Diagnostics;

namespace Elastic.Apm.Messaging.MassTransit
{
    internal class MassTransitDiagnosticInitializer : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly IApmAgent _apmAgent;
        private IDisposable? _sourceSubscription;
        private readonly MassTransitDiagnosticOptions _options;

        internal MassTransitDiagnosticInitializer(IApmAgent apmAgent, MassTransitDiagnosticOptions options)
        {
            _apmAgent = apmAgent;
            _options = options;
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
                _sourceSubscription = value.Subscribe(new MassTransitDiagnosticListener(_apmAgent, _options));
            }
        }
    }
}
