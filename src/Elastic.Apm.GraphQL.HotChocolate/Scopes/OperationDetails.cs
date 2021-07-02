using System.Collections.Immutable;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    public class OperationDetails
    {
        internal static readonly OperationDetails Empty = new OperationDetails(
            "unnamed",
            "unknown",
            new[] { "unknown" }.ToImmutableHashSet(),
            true);

        internal OperationDetails(
            string? name,
            string? rootSelection,
            IImmutableSet<string> selections,
            bool hasFailed)
        {
            Name = name;
            RootSelection = rootSelection;
            Selections = selections;
            HasFailed = hasFailed;
        }

        public string? Name { get; }
        public string? RootSelection { get; }
        public IImmutableSet<string> Selections { get; }
        public bool HasFailed { get; }
    }
}
