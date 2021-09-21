using System;
using System.Diagnostics;
using Elastic.Apm.DiagnosticSource;
using MassTransit;

namespace Elastic.Apm.Messaging.MassTransit
{
    /// <summary>
    /// Diagnostic events subscriber for MassTransit.
    /// </summary>
    public class MassTransitDiagnosticsSubscriber : IDiagnosticsSubscriber
    {
        private readonly Func<ReceiveContext, string> _transactionNameFunc;

        public MassTransitDiagnosticsSubscriber()
            : this(ctx => $"Receive {ctx.InputAddress.AbsolutePath}")
        { }

        public MassTransitDiagnosticsSubscriber(Func<ReceiveContext, string> transactionNameFunc)
        {
            _transactionNameFunc = transactionNameFunc;
        }

        /// <inheritdoc cref="IDiagnosticsSubscriber"/>
        public IDisposable Subscribe(IApmAgent components)
        {
            var compositeDisposable = new CompositeDisposable();

            if (!components.ConfigurationReader.Enabled)
            {
                return compositeDisposable;
            }

            var initializer = new MassTransitDiagnosticInitializer(components, _transactionNameFunc);
            compositeDisposable.Add(initializer);
            compositeDisposable.Add(DiagnosticListener.AllListeners.Subscribe(initializer));

            return compositeDisposable;
        }
    }
}
