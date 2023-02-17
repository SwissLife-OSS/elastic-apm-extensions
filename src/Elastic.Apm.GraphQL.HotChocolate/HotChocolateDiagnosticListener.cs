using System;
using System.Collections.Generic;
using Elastic.Apm.Api;
using Elastic.Apm.Logging;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using IError = HotChocolate.IError;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal class HotChocolateDiagnosticListener : ExecutionDiagnosticEventListener
    {
        private static readonly string ResolveFieldValueFailed = $"{nameof(ResolveFieldValue)} failed.";

        private readonly HotChocolateDiagnosticOptions _options;

        internal HotChocolateDiagnosticListener(HotChocolateDiagnosticOptions options)
        {
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
            if (!Agent.IsConfigured ||
                Agent.Tracer.CurrentTransaction == null ||
                Agent.Tracer.CurrentTransaction.Configuration.LogLevel > LogLevel.Debug)
            {
                return EmptyScope;
            }

            try
            {
                if (context.Path.Length == 0 &&
                    context.Operation.Document.Definitions.Count == 1 &&
                    context.Operation.Document.Definitions[0] is OperationDefinitionNode { Name: { Value: "exec_batch" } })
                {
                    IExecutionSegment? executionSegment = Agent.Tracer.GetExecutionSegment();

                    if (executionSegment == null) return EmptyScope;

                    ISpan span = executionSegment.StartSpan(
                        context.Selection.Field.Name, ApiConstants.TypeRequest, Constants.Apm.SubType);

                    return new FieldActivityScope(span);
                }
            }
            catch (Exception ex)
            {
                Agent.Tracer.CaptureErrorLog(new ErrorLog(ResolveFieldValueFailed), exception: ex);
            }

            return EmptyScope;
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
