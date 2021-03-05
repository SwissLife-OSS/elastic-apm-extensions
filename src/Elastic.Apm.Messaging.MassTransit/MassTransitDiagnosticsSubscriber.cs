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
        /// <inheritdoc cref="IDiagnosticsSubscriber"/>
        public IDisposable Subscribe(IApmAgent components)
        {
            var compositeDisposable = new CompositeDisposable();

            if (!components.ConfigurationReader.Enabled)
            {
                return compositeDisposable;
            }

            var initializer = new MassTransitDiagnosticInitializer(components);
            compositeDisposable.Add(initializer);
            compositeDisposable.Add(DiagnosticListener.AllListeners.Subscribe(initializer));

            return compositeDisposable;
        }
    }
}
