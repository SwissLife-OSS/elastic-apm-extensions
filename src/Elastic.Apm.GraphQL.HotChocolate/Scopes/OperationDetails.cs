using System.Collections.Immutable;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal class OperationDetails
    {
        public static readonly OperationDetails Empty = new OperationDetails(
            "unnamed",
            "unknown",
            new[] { "unknown" }.ToImmutableHashSet());

        internal OperationDetails(
            string? name,
            string? rootSelection,
            IImmutableSet<string> selections)
        {
            Name = name;
            RootSelection = rootSelection;
            Selections = selections;
        }

        public string? Name { get; }
        public string? RootSelection { get; }
        public IImmutableSet<string> Selections { get; }
    }
}
