using Grpc.Core;
using Host;
using GrpcTest;
using Google.Protobuf.WellKnownTypes;

namespace Host.Services;

public class PubsubService : PubSub.PubSubBase
{
    private readonly ILogger<GreeterService> _logger;
    public PubsubService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }
    public override Task<Event> GetAnEvent(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Event{Value = DateTime.Now.ToLongTimeString()});
    }
    public override async Task Subscribe(Subscription request, IServerStreamWriter<Event> responseStream, ServerCallContext context)
    {
        await responseStream.WriteAsync(new GrpcTest.Event{Value=$"It is now {DateTime.Now}."});
    }
    public override Task<Unsubscription> Unsubscribe(Subscription request, ServerCallContext context)
    {
        return base.Unsubscribe(request, context);
    }
}
