using System;
using MassTransit;

namespace Elastic.Apm.Messaging.MassTransit
{
    public class MassTransitDiagnosticOptions
    {
        private readonly Func<ReceiveContext, string> _defaultTransactionName =
            context => $"Receive {context.InputAddress.AbsolutePath}";

        internal MassTransitDiagnosticOptions()
        {
            TransactionName = _defaultTransactionName;
        }

        internal string GetTransactionName(ReceiveContext context)
        {
            var transactionName = TransactionName(context);
            if (string.IsNullOrEmpty(transactionName))
            {
                transactionName = _defaultTransactionName(context);
            }

            return transactionName;
        }

        public Func<ReceiveContext, string> TransactionName { get; set; }
    }
}
