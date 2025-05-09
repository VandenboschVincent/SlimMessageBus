﻿namespace SlimMessageBus.Host.Services;

internal interface IMessageHeaderService
{
    void AddMessageHeaders(IDictionary<string, object> messageHeaders, IDictionary<string, object> headers, object message, ProducerSettings producerSettings);
    void AddMessageTypeHeader(object message, IDictionary<string, object> headers);
}

internal partial class MessageHeaderService : IMessageHeaderService
{
    private readonly ILogger _logger;
    private readonly MessageBusSettings _settings;
    private readonly IMessageTypeResolver _messageTypeResolver;

    public MessageHeaderService(ILogger logger, MessageBusSettings settings, IMessageTypeResolver messageTypeResolver)
    {
        _logger = logger;
        _settings = settings;
        _messageTypeResolver = messageTypeResolver;
    }

    public void AddMessageHeaders(IDictionary<string, object> messageHeaders, IDictionary<string, object> headers, object message, ProducerSettings producerSettings)
    {
        if (headers != null)
        {
            // Add user specific headers
            foreach (var header in headers)
            {
                messageHeaders[header.Key] = header.Value;
            }
        }

        AddMessageTypeHeader(message, messageHeaders);

        if (producerSettings.HeaderModifier != null)
        {
            // Call header hook        
            LogExecutingHeaderModifier("producer");
            producerSettings.HeaderModifier(messageHeaders, message);
        }

        if (_settings.HeaderModifier != null)
        {
            // Call header hook
            LogExecutingHeaderModifier("bus");
            _settings.HeaderModifier(messageHeaders, message);
        }
    }

    public void AddMessageTypeHeader(object message, IDictionary<string, object> headers)
    {
        if (message != null)
        {
            headers.SetHeader(MessageHeaders.MessageType, _messageTypeResolver.ToName(message.GetType()));
        }
    }

    #region Logging

    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = $"Executing {{ConfigLevel}} {nameof(ProducerSettings.HeaderModifier)}")]
    private partial void LogExecutingHeaderModifier(string configLevel);

    #endregion
}

#if NETSTANDARD2_0

internal partial class MessageHeaderService
{
    private partial void LogExecutingHeaderModifier(string configLevel)
        => _logger.LogTrace($"Executing {{ConfigLevel}} {nameof(ProducerSettings.HeaderModifier)}", configLevel);
}

#endif