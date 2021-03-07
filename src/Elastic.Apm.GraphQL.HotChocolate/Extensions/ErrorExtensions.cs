using System.Diagnostics;
using HotChocolate;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal static class ErrorExtensions
    {
        private static readonly StackFrame[] EmptyStackFrame = new StackFrame[0];

        internal static StackFrame[] GetStackFrames(this IError error)
        {
            if (error.Exception != null)
            {
                return new EnhancedStackTrace(error.Exception).GetFrames() ?? EmptyStackFrame;
            }

            return EmptyStackFrame;
        }
    }
}
