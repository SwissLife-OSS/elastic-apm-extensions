using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Elastic.Apm.Api;
using Elastic.Apm.Logging;
using MassTransit;

namespace Elastic.Apm.Messaging.MassTransit
{
    internal class MassTransitDiagnosticListener : IObserver<KeyValuePair<string, object?>>
    {
        private readonly IApmAgent _apmAgent;
        private readonly IApmLogger _logger;

        private readonly ConcurrentDictionary<ActivitySpanId, IExecutionSegment> _activities = 
            new ConcurrentDictionary<ActivitySpanId, IExecutionSegment>();

        internal MassTransitDiagnosticListener(IApmAgent apmAgent)
        {
            _apmAgent = apmAgent;
            _logger = apmAgent.Logger;
        }

        public void OnNext(KeyValuePair<string, object?> value)
        {
            if (!HasActivity(value.Key, out Activity? activity))
            {
                return;
            }

            switch (value.Key)
            {
                case Constants.Events.SendStart:
                    HandleSendStart(activity, value.Value);
                    return;
                case Constants.Events.SendStop:
                    HandleStop(activity.SpanId, activity.Duration);
                    return;
                case Constants.Events.ReceiveStart:
                    HandleReceiveStart(activity, value.Value);
                    return;
                case Constants.Events.ReceiveStop:
                    HandleStop(activity.ParentSpanId, activity.Parent!.Duration);
                    return;
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private void HandleSendStart(Activity activity, object? context)
        {
            try
            {
                IExecutionSegment? executionSegment = _apmAgent.Tracer.GetExecutionSegment();
                if (executionSegment != null && context is SendContext sendContext)
                {
                    var spanName = context is PublishContext ? "Publish" : "Send";
                    spanName = $"{spanName} {sendContext.DestinationAddress.AbsolutePath}";
                    var subType = sendContext.DestinationAddress.Scheme;

                    ISpan span = executionSegment.StartSpan(
                        spanName,
                        Constants.Apm.Type,
                        subType,
                        Constants.Apm.SendAction);
                    
                    sendContext.SetTracingData(span);

                    _activities.TryAdd(activity.SpanId, span);
                }
            }
            catch (Exception ex)
            {
                var message = $"{Constants.Events.SendStart} instrumentation failed.";
                _logger.Log(LogLevel.Error, message, ex, default);
            }
        }

        private void HandleReceiveStart(Activity activity, object? context)
        {
            try
            {
                if (context is ReceiveContext receiveContext)
                {
                    DistributedTracingData? tracingData = receiveContext.GetTracingData();
                    var transactionName = $"Receive {receiveContext.InputAddress.AbsolutePath}";

                    ITransaction transaction = _apmAgent.Tracer.StartTransaction(
                        transactionName,
                        Constants.Apm.Type,
                        tracingData);

                    _activities.TryAdd(activity.SpanId, transaction);
                }
            }
            catch (Exception ex)
            {
                var message = $"{Constants.Events.ReceiveStart} instrumentation failed.";
                _logger.Log(LogLevel.Error, message, ex, default);
            }
        }

        private void HandleStop(ActivitySpanId? spanId, TimeSpan duration)
        {
            if (spanId.HasValue &&
                _activities.TryRemove(spanId.Value, out IExecutionSegment executionSegment))
            {
                executionSegment.Duration = duration.TotalMilliseconds;
                executionSegment.End();
            }
        }

        private bool HasActivity(string eventName, [NotNullWhen(true)] out Activity? activity)
        {
            activity = Activity.Current;
            if (activity == null)
            {
                var message = $"No activity was found for event: {eventName}";
                _logger.Log(LogLevel.Warning, message, default, default);
                return false;
            }

            return true;
        }
    }
}
