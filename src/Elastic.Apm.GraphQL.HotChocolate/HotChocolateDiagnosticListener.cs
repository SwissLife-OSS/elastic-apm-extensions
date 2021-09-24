using System;
using System.Collections.Generic;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using Elastic.Apm.Logging;
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

        private readonly IApmAgent _apmAgent;
        private readonly IConfigurationReader _configuration;
        private readonly HotChocolateDiagnosticOptions _options;
        private readonly IApmLogger _apmLogger;

        internal HotChocolateDiagnosticListener(
            IApmAgent apmAgent,
            IConfigurationReader configuration,
            HotChocolateDiagnosticOptions options)
        {
            _apmAgent = apmAgent;
            _configuration = configuration;
            _options = options;
            _apmLogger = _apmAgent.Logger;
        }

        public override IDisposable ExecuteRequest(IRequestContext context)
        {
            ITransaction? transaction = _apmAgent.Tracer.CurrentTransaction;
            return transaction != null
                ? new RequestActivityScope(context, transaction, _apmAgent, _options)
                : EmptyScope;
        }

        public override IDisposable ResolveFieldValue(IMiddlewareContext context)
        {
            if (_configuration.LogLevel > LogLevel.Debug)
            {
                return EmptyScope;
            }

            try
            {
                if (context.Path.Depth == 0 &&
                    context.Document.Definitions.Count == 1 &&
                    context.Document.Definitions[0] is OperationDefinitionNode { Name: { Value: "exec_batch" } })
                {
                    IExecutionSegment? executionSegment = _apmAgent.Tracer.GetExecutionSegment();

                    if (executionSegment == null) return EmptyScope;

                    ISpan span = executionSegment.StartSpan(
                        context.Field.Name!.Value, ApiConstants.TypeRequest, Constants.Apm.SubType);

                    return new FieldActivityScope(span);
                }
            }
            catch (Exception ex)
            {
                _apmLogger.Log(LogLevel.Error, ResolveFieldValueFailed, ex, LogFormatter.Nop);
            }

            return EmptyScope;
        }

        public override void RequestError(IRequestContext context, Exception exception)
        {
            _apmAgent.CaptureException(exception);
        }

        public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
        {
            _apmAgent.CaptureError(errors);
        }
    }
}
