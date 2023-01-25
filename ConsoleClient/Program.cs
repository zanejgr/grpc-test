using Grpc.Net.Client;
using GrpcTest.Forum.Service;

using var channel = GrpcChannel.ForAddress("http://localhost:5207");
var client = new CommandService.CommandServiceClient(channel);
var reply = client.Login(new LoginRequest{UserRequest=new UserRequest{Username="admin"}});
Console.WriteLine(reply.DataCase);
Console.WriteLine(new Guid(reply.User.Id.ToByteArray()));
Console.WriteLine(reply.User.Username);
var res = client.Logout(new LogoutRequest());
Console.WriteLine(res.DataCase);
