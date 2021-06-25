using System;
using System.Collections.Generic;
using Elastic.Apm.Api;
using IError = HotChocolate.IError;
using LogLevel = Elastic.Apm.Logging.LogLevel;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal static class ApmAgentExtensions
    {
        internal static void CaptureError(this IApmAgent apmAgent, IReadOnlyList<IError> errors)
        {
            try
            {
                IExecutionSegment? executionSegment = apmAgent.Tracer.GetExecutionSegment();
                if (executionSegment == null) return;

                foreach (IError error in errors)
                {
                    var path = error.Path?.ToString();
                    executionSegment.CaptureError(error.Message, path, error.GetStackFrames());

                    // TODO: Use application ILogger ?
                    apmAgent.Logger.Log(LogLevel.Error, $"{error.Message} {path}", default, default);
                }
            }
            catch (Exception ex)
            {
                var message = "CaptureError failed.";
                apmAgent.Logger.Log(LogLevel.Error, message, ex, default);
            }
        }

        internal static void CaptureException(this IApmAgent apmAgent, Exception exception)
        {
            try
            {
                IExecutionSegment? executionSegment = apmAgent.Tracer.GetExecutionSegment();
                if (executionSegment == null) return;

                executionSegment.CaptureException(exception);

                // TODO: Use application ILogger ?
                apmAgent.Logger.Log(LogLevel.Error, $"{exception.Message}", default, default);
            }
            catch (Exception ex)
            {
                var message = "CaptureException failed.";
                apmAgent.Logger.Log(LogLevel.Error, message, ex, default);
            }
        }
    }
}
