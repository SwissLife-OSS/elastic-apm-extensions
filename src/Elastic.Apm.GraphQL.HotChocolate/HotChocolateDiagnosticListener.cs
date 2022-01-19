using System;
using System.Collections.Generic;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using IError = HotChocolate.IError;
using LogLevel = Elastic.Apm.Logging.LogLevel;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal class HotChocolateDiagnosticListener : ExecutionDiagnosticEventListener
    {
        private static readonly string ResolveFieldValueFailed = $"{nameof(ResolveFieldValue)} failed.";

        private readonly IConfigurationReader _configuration;
        private readonly HotChocolateDiagnosticOptions _options;

        internal HotChocolateDiagnosticListener(
            IConfigurationReader configuration,
            HotChocolateDiagnosticOptions options)
        {
            _configuration = configuration;
            _options = options;
        }

        public override IDisposable ExecuteRequest(IRequestContext context)
        {
            if (!Agent.IsConfigured)
            {
                return EmptyScope;
            }

            ITransaction? transaction = Agent.Tracer.CurrentTransaction;
            return transaction != null
                ? new RequestActivityScope(context, transaction, _options)
                : EmptyScope;
        }

        public override IDisposable ResolveFieldValue(IMiddlewareContext context)
        {
            if (_configuration.LogLevel > LogLevel.Debug || !Agent.IsConfigured)
            {
                return EmptyScope;
            }

            ITransaction? transaction = Agent.Tracer.CurrentTransaction;
            return transaction != null && transaction.Configuration.LogLevel <= LogLevel.Debug
                ? new FieldActivityScope(context, transaction, _options)
                : EmptyScope;
        }

        public override void RequestError(IRequestContext context, Exception exception)
        {
            if (Agent.IsConfigured)
            {
                Agent.Tracer.CaptureException(exception);
            }
        }

        public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
        {
            if (Agent.IsConfigured)
            {
                Agent.Tracer.CaptureError(errors);
            }
        }
    }
}
