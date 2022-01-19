using System;
using System.Text;
using Elastic.Apm.Api;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal class FieldActivityScope : IDisposable
    {
        private static readonly string ResolveFieldFailed = "ResolveField instrumentation failed.";

        private readonly IMiddlewareContext _context;
        private readonly ITransaction _transaction;
        private readonly HotChocolateDiagnosticOptions _options;
        private bool _disposed;

        internal FieldActivityScope(
            IMiddlewareContext context,
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
                (string path, string hierarchy) = BuildPath();

                IFieldSelection selection = _context.Selection;
                FieldCoordinate coordinate = selection.Field.Coordinate;

                ISpan? span = _transaction
                    .StartSpan(path, ApiConstants.TypeRequest, Constants.Apm.SubType);

                span.SetLabel("graphql.selection.name", selection.ResponseName.Value);
                span.SetLabel("graphql.selection.type", selection.Field.Type.Print());
                span.SetLabel("graphql.selection.path", hierarchy);
                span.SetLabel("graphql.selection.hierarchy", path);
                span.SetLabel("graphql.selection.field.name", coordinate.FieldName.Value);
                span.SetLabel("graphql.selection.field.coordinate", coordinate.ToString());
                span.SetLabel("graphql.selection.field.declaringType", coordinate.TypeName.Value);
                span.SetLabel("graphql.selection.field.isDeprecated", selection.Field.IsDeprecated);

                span.End();
            }
            catch (Exception ex)
            {
                Agent.Tracer.CaptureErrorLog(new ErrorLog(ResolveFieldFailed), exception: ex);
            }
        }

        private (string path, string hierarchy) BuildPath()
        {
            StringBuilder path = new StringBuilder();
            StringBuilder hierarchy = new StringBuilder();
            StringBuilder index = new StringBuilder();

            Path? current = _context.Path;

            do
            {
                if (current is NamePathSegment n)
                {
                    path.Insert(0, '/');
                    hierarchy.Insert(0, '/');
                    path.Insert(1, n.Name.Value);
                    hierarchy.Insert(1, n.Name.Value);

                    if (index.Length > 0)
                    {
                        path.Insert(1 + n.Name.Value.Length, index);
                    }

                    index.Clear();
                }

                if (current is IndexerPathSegment i)
                {
                    var number = i.Index.ToString();
                    index.Insert(0, '[');
                    index.Insert(1, number);
                    index.Insert(1 + number.Length, ']');
                }

                current = current.Parent;
            } while (current is not null && current is not RootPathSegment);

            return (path.ToString(), hierarchy.ToString());
        }
    }
}
