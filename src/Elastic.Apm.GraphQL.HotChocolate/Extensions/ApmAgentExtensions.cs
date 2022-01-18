using System;
using System.Collections.Generic;
using System.Diagnostics;
using Elastic.Apm.Api;
using IError = HotChocolate.IError;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal static class ApmAgentExtensions
    {
        private static readonly string CaptureErrorFailed = $"{nameof(CaptureError)} failed.";
        private static readonly string CaptureExceptionFailed = $"{nameof(CaptureException)} failed.";

        internal static void CaptureError(this ITracer tracer, IReadOnlyList<IError> errors)
        {
            try
            {
                IExecutionSegment? executionSegment = tracer.GetExecutionSegment();
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
                }
            }
            catch (Exception ex)
            {
                tracer.CaptureErrorLog(new ErrorLog(CaptureErrorFailed), exception: ex);
            }
        }

        internal static void CaptureException(this ITracer tracer, Exception exception)
        {
            try
            {
                IExecutionSegment? executionSegment = tracer.GetExecutionSegment();
                if (executionSegment == null) return;

                executionSegment.CaptureException(exception);
            }
            catch (Exception ex)
            {
                tracer.CaptureErrorLog(new ErrorLog(CaptureExceptionFailed), exception: ex);
            }
        }
    }
}
