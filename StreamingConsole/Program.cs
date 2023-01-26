using Google.Protobuf;

using Grpc.Core;
using Grpc.Net.Client;

using GrpcTest.Forum.Service;

using var channel = GrpcChannel.ForAddress("http://localhost:5207");
var commandClient = new CommandService.CommandServiceClient(channel);
var queryClient = new QueryService.QueryServiceClient(channel);

var reply = commandClient.Login(
    new LoginRequest
    {
        UserRequest = new UserRequest
        {
            Username = "streamer"
        }
    });

Console.WriteLine(reply.ToString());
if (reply.DataCase == LoginResponse.DataOneofCase.User)
{
    Console.Write("User ID:");
    Console.WriteLine(new Guid(reply.User.Id.ToByteArray()));
}
Console.WriteLine("Subscribing. Press any key to end.");
var tokenSource = new CancellationTokenSource();
var token = tokenSource.Token;
Task.Factory.StartNew(async () =>
{
    using var stream = queryClient.OpenStream(new OpenStreamRequest(), cancellationToken: token);
    await foreach (var msg in stream.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(msg.ToString());
    }
}, token);
Console.ReadKey();
tokenSource.Cancel();