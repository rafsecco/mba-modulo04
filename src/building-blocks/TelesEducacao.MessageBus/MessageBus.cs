using EasyNetQ;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

namespace TelesEducacao.MessageBus;

public class MessageBus : IMessageBus
{
    private IBus _bus;

    public MessageBus(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T message) where T : IntegrationEvent
    {
        await _bus.PubSub.PublishAsync(message);
    }

    public async Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage) where T : class
    {
        await _bus.PubSub.SubscribeAsync(subscriptionId, onMessage);
    }

    public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
    where TRequest : IntegrationEvent where TResponse : ResponseMessage
    {
        return await _bus.Rpc.RequestAsync<TRequest, TResponse>(request);
    }

    public async Task<IAsyncDisposable> RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
    where TRequest : IntegrationEvent where TResponse : ResponseMessage
    {
        return await _bus.Rpc.RespondAsync(responder);
    }
}

public interface IMessageBus
{
    Task PublishAsync<T>(T message) where T : IntegrationEvent;

    Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IntegrationEvent
        where TResponse : ResponseMessage;

    Task<IAsyncDisposable> RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
        where TRequest : IntegrationEvent
        where TResponse : ResponseMessage;

    Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage) where T : class;
}