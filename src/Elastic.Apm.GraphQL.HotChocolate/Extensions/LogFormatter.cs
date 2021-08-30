using System;

namespace Elastic.Apm.GraphQL.HotChocolate
{
    internal static class LogFormatter
    {
        internal static Func<string, Exception, string> Nop { get; } =
            (s, _) => s;
    }
}
