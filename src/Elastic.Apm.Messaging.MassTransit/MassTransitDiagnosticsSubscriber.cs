using System;
using System.Diagnostics;
using Elastic.Apm.DiagnosticSource;

namespace Elastic.Apm.Messaging.MassTransit
{
    /// <summary>
    /// Diagnostic events subscriber for MassTransit.
    /// </summary>
    public class MassTransitDiagnosticsSubscriber : IDiagnosticsSubscriber
    {
        private readonly MassTransitDiagnosticOptions _options;

        public MassTransitDiagnosticsSubscriber(Action<MassTransitDiagnosticOptions>? configure = default)
        {
            _options = new MassTransitDiagnosticOptions();
            configure?.Invoke(_options);
        }

        /// <inheritdoc cref="IDiagnosticsSubscriber"/>
        public IDisposable Subscribe(IApmAgent apmAgent)
        {
            var compositeDisposable = new CompositeDisposable();

            if (!apmAgent.ConfigurationReader.Enabled)
            {
                return compositeDisposable;
            }

            var initializer = new MassTransitDiagnosticInitializer(apmAgent, _options);
            compositeDisposable.Add(initializer);
            compositeDisposable.Add(DiagnosticListener.AllListeners.Subscribe(initializer));

            return compositeDisposable;
        }
    }
}
