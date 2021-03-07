using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HotChocolate.Language;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal static class DefinitionNodeExtensions
    {
        internal static IReadOnlyList<FieldNode> GetFieldNodes(
            this IReadOnlyList<IDefinitionNode> definitions)
        {
            return definitions.OfType<OperationDefinitionNode>()
                .SelectMany(x => x.SelectionSet.Selections)
                .OfType<FieldNode>()
                .ToImmutableList();
        }

        internal static int FirstSelectionsCount(
            this IReadOnlyList<IDefinitionNode> definitions)
        {
            return definitions.OfType<OperationDefinitionNode>()
                .FirstOrDefault()?.SelectionSet.Selections.Count ?? 0;
        }
    }
}
