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
        private MassTransitDiagnosticOptions _options;

        public MassTransitDiagnosticsSubscriber(Action<MassTransitDiagnosticOptions>? configure = default)
        {
            _options = new MassTransitDiagnosticOptions();
            configure?.Invoke(_options);
        }

        /// <inheritdoc cref="IDiagnosticsSubscriber"/>
        public IDisposable Subscribe(IApmAgent components)
        {
            var compositeDisposable = new CompositeDisposable();

            if (!components.ConfigurationReader.Enabled)
            {
                return compositeDisposable;
            }

            var initializer = new MassTransitDiagnosticInitializer(components, _options);
            compositeDisposable.Add(initializer);
            compositeDisposable.Add(DiagnosticListener.AllListeners.Subscribe(initializer));

            return compositeDisposable;
        }
    }
}
