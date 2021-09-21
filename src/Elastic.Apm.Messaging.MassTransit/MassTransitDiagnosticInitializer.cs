using System;
using System.Diagnostics;
using MassTransit;

namespace Elastic.Apm.Messaging.MassTransit
{
    internal class MassTransitDiagnosticInitializer : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly IApmAgent _apmAgent;
        private IDisposable? _sourceSubscription;
        private readonly Func<ReceiveContext, string> _transactionNameFunc;

        internal MassTransitDiagnosticInitializer(IApmAgent apmAgent, Func<ReceiveContext, string> transactionNameFunc)
        {
            _apmAgent = apmAgent;
            _transactionNameFunc = transactionNameFunc;
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
                _sourceSubscription = value.Subscribe(new MassTransitDiagnosticListener(_apmAgent, _transactionNameFunc));
            }
        }
    }
}
