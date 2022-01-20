using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Elastic.Apm.Api;
using Elastic.Apm.Logging;
using MassTransit;

namespace Elastic.Apm.Messaging.MassTransit
{
    internal class MassTransitDiagnosticListener : IObserver<KeyValuePair<string, object?>>
    {
        private readonly IApmAgent _apmAgent;
        private readonly IApmLogger _logger;
        private readonly MassTransitDiagnosticOptions _options;

        private readonly ConcurrentDictionary<ActivitySpanId, IExecutionSegment> _activities = new();
        private readonly ConcurrentDictionary<ActivitySpanId, IExecutionSegment[]> _multipleActivities = new();

        internal MassTransitDiagnosticListener(IApmAgent apmAgent, MassTransitDiagnosticOptions options)
        {
            _apmAgent = apmAgent;
            _options = options;
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
                case Constants.Events.ConsumeStart:
                    HandleConsumeStart(activity, value.Value);
                    return;
                case Constants.Events.ConsumeStop:
                    HandleStop(activity.SpanId, activity.Duration);
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
                if (context is SendContext sendContext)
                {
                    var hasTransaction = true;
                    var name = $"Send {_options.GetSendLabel(sendContext)}";

                    var isSendingResponse = sendContext.Headers.TryGetMessageResponse(out var messageResponse);
                    if (isSendingResponse)
                    {
                        name = $"Respond {messageResponse}";
                    }

                    IExecutionSegment? executionSegment = _apmAgent.Tracer.GetExecutionSegment();
                    if (executionSegment == null)
                    {
                        executionSegment = _apmAgent.Tracer
                            .StartTransaction(name, Constants.Apm.Type);
                        hasTransaction = false;
                    }

                    var subType = sendContext.GetSpanSubType();
                    ISpan span = executionSegment.StartSpan(
                        hasTransaction ? name : "Sending",
                        Constants.Apm.Type,
                        subType,
                        Constants.Apm.SendAction);

                    Uri? address = isSendingResponse ? sendContext.SourceAddress : sendContext.DestinationAddress;
                    span.Context.Destination = new Destination
                    {
                        Address = sendContext.DestinationAddress.AbsoluteUri,  
                        Service = new Destination.DestinationService
                        {
                            Resource = $"{subType}{address.AbsolutePath}"
                        }
                    };

                    span.Context.Message = new Message
                    {
                        Queue = new Queue { Name = address.GetAbsoluteName() }
                    };

                    sendContext.SetTracingData(span, isSendingResponse);

                    if (hasTransaction)
                    {
                        _activities.TryAdd(activity.SpanId, span);
                    }
                    else
                    {
                        _multipleActivities.TryAdd(activity.SpanId, new[] { span, executionSegment });
                    }
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
                    var transactionName = $"Receive {_options.GetReceiveLabel(receiveContext)}";

                    var isReceivingResponse = receiveContext.TryGetMessageResponse(out var messageResponse);
                    if (isReceivingResponse)
                    {
                        transactionName = $"Receive response {messageResponse}";
                    }

                    ITransaction? transaction;
                    var inline = _options.InlineReceiveTransaction || receiveContext.WaitForResponse();
                    if (inline)
                    {
                        DistributedTracingData? tracingData = receiveContext.GetTracingData();

                        transaction = _apmAgent.Tracer.StartTransaction(
                            transactionName,
                            Constants.Apm.Type,
                            tracingData);
                    }
                    else
                    {
                        transaction = _apmAgent.Tracer.StartTransaction(
                            transactionName,
                            Constants.Apm.Type);
                    }

                    var subType = receiveContext.GetSpanSubType();
                    ISpan span = transaction.StartSpan(
                        "Receiving",
                        Constants.Apm.Type,
                        subType,
                        Constants.Apm.SendAction);

                    span.Context.Destination = new Destination
                    {
                        Address = receiveContext.InputAddress.AbsoluteUri,
                        Service = new Destination.DestinationService
                        {
                            Resource = $"{subType}{receiveContext.InputAddress.AbsolutePath}"
                        }
                    };

                    span.Context.Message = new Message
                    {
                        Queue = new Queue { Name = receiveContext.GetInputAbsoluteName() }
                    };

                    _multipleActivities.TryAdd(activity.SpanId, new[] { (IExecutionSegment)span, transaction });
                }
            }
            catch (Exception ex)
            {
                var message = $"{Constants.Events.ReceiveStart} instrumentation failed.";
                _logger.Log(LogLevel.Error, message, ex, default);
            }
        }

        private void HandleConsumeStart(Activity activity, object? context)
        {
            try
            {
                if (context is ConsumeContext consumeContext)
                {
                    var consumerType = activity.Tags.FirstOrDefault(t => t.Key == "consumer-type").Value;
                    var name = string.IsNullOrEmpty(consumerType) ? "Consume" : $"Consume by {consumerType}";

                    IExecutionSegment? executionSegment = _apmAgent.Tracer.GetExecutionSegment();
                    if (executionSegment != null)
                    {
                        var subType = consumeContext.ReceiveContext.GetSpanSubType();
                        ISpan span = executionSegment.StartSpan(
                            name,
                            Constants.Apm.Type,
                            subType,
                            Constants.Apm.ConsumeAction);

                        span.Context.Destination = new Destination
                        {
                            Address = consumeContext.ReceiveContext.InputAddress.AbsoluteUri,
                            Service = new Destination.DestinationService
                            {
                                Resource = $"{subType}{consumeContext.ReceiveContext.InputAddress.AbsolutePath}"
                            }
                        };

                        span.Context.Message = new Message
                        {
                            Queue = new Queue { Name = consumeContext.ReceiveContext.GetInputAbsoluteName() }
                        };

                        _activities.TryAdd(activity.SpanId, span);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = $"{Constants.Events.ConsumeStart} instrumentation failed.";
                _logger.Log(LogLevel.Error, message, ex, default);
            }
        }

        private void HandleStop(ActivitySpanId? spanId, TimeSpan duration)
        {
            if (spanId.HasValue)
            {
                if (_activities.Any() &&
                    _activities.TryRemove(spanId.Value, out IExecutionSegment? executionSegment) &&
                    executionSegment != null)
                {
                    executionSegment.Duration = duration.TotalMilliseconds;
                    executionSegment.End();    
                }

                if (_multipleActivities.Any() &&
                    _multipleActivities.TryRemove(spanId.Value, out IExecutionSegment[]? executionSegments) &&
                    executionSegments != null)
                {
                    for (var i = 0; i < executionSegments.Length; i++)
                    {
                        executionSegments[i].Duration = duration.TotalMilliseconds;
                        executionSegments[i].End();
                    }
                }
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
