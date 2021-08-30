using System;
using System.Collections.Generic;
using System.Diagnostics;
using Elastic.Apm.Api;
using IError = HotChocolate.IError;
using LogLevel = Elastic.Apm.Logging.LogLevel;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal static class ApmAgentExtensions
    {
        private static readonly string CaptureErrorFailed = $"{nameof(CaptureError)} failed.";
        private static readonly string CaptureExceptionFailed = $"{nameof(CaptureException)} failed.";

        internal static void CaptureError(this IApmAgent apmAgent, IReadOnlyList<IError> errors)
        {
            try
            {
                IExecutionSegment? executionSegment = apmAgent.Tracer.GetExecutionSegment();
                if (executionSegment == null) return;

                foreach (IError error in errors)
                {
                    var path = error.Path?.ToString();
                    if (error.Exception != null)
                    {
                        executionSegment.CaptureException(error.Exception, path);
                    }
                    else
                    {
                        executionSegment.CaptureError(error.Message, path, Array.Empty<StackFrame>());
                    }

                    var message = $"{error.Message} {path}";
                    apmAgent.Logger.Log(LogLevel.Error, message, default, LogFormatter.Nop);
                }
            }
            catch (Exception ex)
            {
                apmAgent.Logger.Log(LogLevel.Error, CaptureErrorFailed, ex, LogFormatter.Nop);
            }
        }

        internal static void CaptureException(this IApmAgent apmAgent, Exception exception)
        {
            try
            {
                IExecutionSegment? executionSegment = apmAgent.Tracer.GetExecutionSegment();
                if (executionSegment == null) return;

                executionSegment.CaptureException(exception);

                apmAgent.Logger.Log(LogLevel.Error, exception.Message, default, LogFormatter.Nop);
            }
            catch (Exception ex)
            {
                apmAgent.Logger.Log(LogLevel.Error, CaptureExceptionFailed, ex, LogFormatter.Nop);
            }
        }
    }
}
