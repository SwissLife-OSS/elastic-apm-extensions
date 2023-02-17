using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elastic.Apm.Api;
using HotChocolate.Execution;
using HotChocolate.Execution.Processing;
using IError = HotChocolate.IError;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal class RequestActivityScope : IDisposable
    {
        private static readonly string ExecuteRequestFailed = "ExecuteRequest instrumentation failed.";

        private readonly IRequestContext _context;
        private readonly ITransaction _transaction;
        private readonly HotChocolateDiagnosticOptions _options;
        private bool _disposed;

        internal RequestActivityScope(
            IRequestContext context,
            ITransaction transaction,
            HotChocolateDiagnosticOptions options)
        {
            _context = context;
            _transaction = transaction;
            _options = options;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                EnrichTransaction();
                _disposed = true;
            }
        }

        private void EnrichTransaction()
        {
            try
            {
                OperationDetails operationDetails = GetOperationDetails();
                _transaction.Name = operationDetails.Name;
                _transaction.Type = ApiConstants.TypeRequest;

                _transaction.SetLabel("graphql.document.id", _context.DocumentId);
                _transaction.SetLabel("graphql.document.hash", _context.DocumentHash);
                _transaction.SetLabel("graphql.document.valid", _context.IsValidDocument);
                _transaction.SetLabel("graphql.operation.id", _context.OperationId);
                _transaction.SetLabel("graphql.operation.kind", _context.Operation?.Type.ToString());
                _transaction.SetLabel("graphql.operation.name", _context.Operation?.Name);

                if (_context.Result is IQueryResult result)
                {
                    var errorCount = result.Errors?.Count ?? 0;
                    _transaction.SetLabel("graphql.errors.count", errorCount);
                }

                if (_context.Result.HasErrors(out IReadOnlyList<IError>? errors))
                {
                    Agent.Tracer.CaptureError(errors);
                }

                if (_context.HasException(out Exception? exception))
                {
                    Agent.Tracer.CaptureException(exception);
                }

                _options.Enrich?.Invoke(_transaction, operationDetails);
            }
            catch (Exception ex)
            {
                Agent.Tracer.CaptureErrorLog(new ErrorLog(ExecuteRequestFailed), exception: ex);
            }
        }

        private string? CreateOperationDisplayName()
        {
            if (_context.Operation is { } operation)
            {
                var displayName = new StringBuilder();

                ISelectionSet rootSelectionSet = operation.RootSelectionSet;

                displayName.Append('{');
                displayName.Append(' ');

                foreach (ISelection selection in rootSelectionSet.Selections.Take(3))
                {
                    if (displayName.Length > 2)
                    {
                        displayName.Append(' ');
                    }

                    displayName.Append(selection.ResponseName);
                }

                if (rootSelectionSet.Selections.Count > 3)
                {
                    displayName.Append(' ');
                    displayName.Append('.');
                    displayName.Append('.');
                    displayName.Append('.');
                }

                displayName.Append(' ');
                displayName.Append('}');

                if (operation.Name != null)
                {
                    displayName.Insert(0, ' ');
                    displayName.Insert(0, operation.Name);
                }

                displayName.Insert(0, ' ');
                displayName.Insert(0, operation.Definition.Operation.ToString().ToLowerInvariant());

                return displayName.ToString();
            }

            return null;
        }

        private OperationDetails GetOperationDetails()
        {
            var operationDisplayName = CreateOperationDisplayName();
            return new OperationDetails(operationDisplayName ?? "unnamed", _context.HasFailed());
        }
    }
}
