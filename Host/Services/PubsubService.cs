using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using GrpcTest;

using Host;

namespace Host.Services;

public class PubsubService : PubSub.PubSubBase
{
    private readonly ILogger<GreeterService> _logger;
    public PubsubService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }
}
