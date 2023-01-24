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
}
