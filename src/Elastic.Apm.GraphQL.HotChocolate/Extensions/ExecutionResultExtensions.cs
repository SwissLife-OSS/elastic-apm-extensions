using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HotChocolate.Execution;
using IError = HotChocolate.IError;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal static class ExecutionResultExtensions
    {
        internal static bool HasErrors(
            this IExecutionResult? result,
            [NotNullWhen(true)] out IReadOnlyList<IError>? errors)
        {
            errors = null;
            if (result is not IQueryResult queryResult) return false;

            errors = queryResult?.Errors;
            return errors?.Any() ?? false;
        }

        internal static bool HasException(
            this IRequestContext context,
            [NotNullWhen(true)] out Exception? exception)
        {
            exception = context.Exception;
            return exception != null;
        }

        internal static bool HasFailed(this IRequestContext context)
        {
            return context.Result.HasErrors(out _) || context.HasException(out _);
        }
    }
}
