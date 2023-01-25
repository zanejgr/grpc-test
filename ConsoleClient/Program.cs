using Grpc.Net.Client;
using GrpcTest.Forum.Service;

using var channel = GrpcChannel.ForAddress("http://localhost:5207");
var client = new CommandService.CommandServiceClient(channel);
var reply = client.Login(new LoginRequest());
Console.WriteLine(new Guid(reply.User.Id.ToByteArray()));
Console.WriteLine(reply.User.Username);

