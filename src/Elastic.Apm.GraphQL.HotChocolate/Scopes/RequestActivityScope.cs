using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Elastic.Apm.Api;
using Elastic.Apm.Logging;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Language;
using IError = HotChocolate.IError;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal class RequestActivityScope : IActivityScope
    {
        private static readonly string ExecuteRequestFailed = "ExecuteRequest instrumentation failed.";

        private readonly IRequestContext _context;
        private readonly ITransaction _transaction;
        private readonly IApmAgent _apmAgent;
        private readonly HotChocolateDiagnosticOptions _options;

        internal RequestActivityScope(
            IRequestContext context,
            ITransaction transaction,
            IApmAgent apmAgent,
            HotChocolateDiagnosticOptions options)
        {
            _context = context;
            _transaction = transaction;
            _apmAgent = apmAgent;
            _options = options;
        }

        public void Dispose()
        {
            try
            {
                OperationDetails operationDetails = GetOperationDetails();
                _transaction.Name = $"[{operationDetails.Name}] {operationDetails.RootSelection}";
                _transaction.Type = ApiConstants.TypeRequest;

                _transaction.SetLabel("selections", string.Join(";", operationDetails.Selections));

                if (_context.Result.HasErrors(out IReadOnlyList<IError>? errors))
                {
                    _apmAgent.CaptureError(errors);
                }

                if (_context.HasException(out Exception? exception))
                {
                    _apmAgent.CaptureException(exception);
                }

                _options.Enrich?.Invoke(_transaction, operationDetails);
            }
            catch (Exception ex)
            {
                _apmAgent.Logger.Log(LogLevel.Error, ExecuteRequestFailed, ex, LogFormatter.Nop);
            }
        }

        private OperationDetails GetOperationDetails()
        {
            IReadOnlyList<IDefinitionNode>? definitions = _context.Document?.Definitions;
            if (definitions?.Count > 0)
            {
                var name = GetOperationName(definitions);

                var selections = definitions.GetFieldNodes()
                    .Select(x => string.IsNullOrEmpty(x.Name.Value) ? "unknown" : x.Name.Value)
                    .DefaultIfEmpty("unknown")
                    .ToImmutableHashSet();

                var rootSelection = definitions.Count > 1
                    ? "multiple"
                    : name == "exec_batch"
                        ? selections.Count > 1
                            ? $"multiple ({definitions.FirstSelectionsCount()})"
                            : $"{selections.FirstOrDefault()} ({definitions.FirstSelectionsCount()})"
                        : selections.FirstOrDefault();

                return new OperationDetails(name, rootSelection, selections, _context.HasFailed());
            }

            return OperationDetails.Empty;
        }

        private string? GetOperationName(IReadOnlyList<IDefinitionNode> definition)
        {
            var name = _context.Request.OperationName;

            if (string.IsNullOrEmpty(name) &&
                definition.Count == 1 && 
                definition[0] is OperationDefinitionNode node)
            {
                name = node.Name?.Value;
            }

            if (string.IsNullOrEmpty(name) || name == "fetch")
            {
                name = _context.Request.QueryId;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = Constants.DefaultOperation;
            }

            return name;
        }
    }
}
