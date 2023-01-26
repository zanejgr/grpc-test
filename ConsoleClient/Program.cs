using Google.Protobuf;
using Grpc.Net.Client;
using GrpcTest.Forum.Service;

using var channel = GrpcChannel.ForAddress("http://localhost:5207");
var client = new CommandService.CommandServiceClient(channel);
var reply = client.Login(new LoginRequest{UserRequest=new UserRequest{Username="admin"}});
Console.WriteLine(reply.DataCase);
Console.WriteLine(new Guid(reply.User.Id.ToByteArray()));
Console.WriteLine(reply.User.Username);

var dm = client.SendDm(new SendDmRequest{
		Recipient= new UserRequest{
		Username = "admin"
		},
		Text = "hi hello howdy"});

var post = client.MakeForumPost(new MakeForumPostRequest{ Text = "hsdjkhfksdkhfkshdkf"});
client.MakeForumPost(new MakeForumPostRequest{ Text = "replying to master post", Id = ByteString.CopyFrom(Guid.Parse("67c3f052-3f02-4e23-ae97-bcc02e658b55").ToByteArray())});
// doesn't work. Always Board Post Not Found because of reference equality.
client.MakeForumPost(new MakeForumPostRequest{ Text = "replying to post", Message = post.Message });
var res = client.Logout(new LogoutRequest());
Console.WriteLine(res.DataCase);
