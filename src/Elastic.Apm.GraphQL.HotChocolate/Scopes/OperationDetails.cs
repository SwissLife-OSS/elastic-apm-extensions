namespace Elastic.Apm.GraphQL.HotChocolate
{
    public class OperationDetails
    {
        internal OperationDetails(
            string? name,
            bool hasFailed)
        {
            Name = name;
            HasFailed = hasFailed;
        }

        public string? Name { get; }
        public bool HasFailed { get; }
    }
}
