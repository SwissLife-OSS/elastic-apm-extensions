using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HotChocolate;
using HotChocolate.Execution;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal static class ExecutionResultExtensions
    {
        internal static bool HasErrors(
            this IExecutionResult? result,
            [NotNullWhen(true)] out IReadOnlyList<IError>? errors)
        {
            errors = result?.Errors;
            return errors?.Any() ?? false;
        }

        internal static bool HasException(
            this IRequestContext context,
            [NotNullWhen(true)] out Exception? exception)
        {
            exception = context.Exception;
            return exception != null;
        }
    }
}
