using Google.Protobuf;

using Grpc.Net.Client;

using GrpcTest.Forum.Service;

using var channel = GrpcChannel.ForAddress("http://localhost:5207");
var commandClient = new CommandService.CommandServiceClient(channel);
var queryClient = new QueryService.QueryServiceClient(channel);

var inbox = queryClient.ListInbox(new());
Console.WriteLine(inbox.Messages.Count());
foreach (var v in inbox.Messages)
{
    Console.WriteLine(v.ToString());
}

var reply = commandClient.Login(new LoginRequest { UserRequest = new UserRequest { Username = "admin" } });
Console.WriteLine(reply.DataCase);
Console.WriteLine(new Guid(reply.User.Id.ToByteArray()));
Console.WriteLine(reply.User.Username);

inbox = queryClient.ListInbox(new());
Console.WriteLine(inbox.Messages.Count());
foreach (var v in inbox.Messages)
{
    Console.WriteLine(v.ToString());
}

var dm = commandClient.SendDm(new SendDmRequest
{
    Recipient = new UserRequest
    {
        Username = "admin"
    },
    Text = "hi hello howdy"
});

var post = commandClient.MakeForumPost(new MakeForumPostRequest { Text = "hsdjkhfksdkhfkshdkf" });
commandClient.MakeForumPost(new MakeForumPostRequest { Text = "replying to master post", Id = ByteString.CopyFrom(Guid.Parse("67c3f052-3f02-4e23-ae97-bcc02e658b55").ToByteArray()) });
commandClient.MakeForumPost(new MakeForumPostRequest { Text = "replying to post", Message = post.Message });

bool next = false;
do
{
    Console.WriteLine("P: Post\nR: Read\nEnter selection:");

    switch (Console.ReadKey().KeyChar)
    {
        case 'P':
        case 'p':
            next = true;
            {
                Console.WriteLine("Enter parent guid");
                Guid parentGuid = default;
                var parentStr = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(parentStr) && !Guid.TryParse(parentStr, out parentGuid))
                {
                    Console.WriteLine("Error parsing");
                    break;
                }
                Console.WriteLine("Enter message");
                string? message = Console.ReadLine();
                if (message == null)
                {
                    Console.WriteLine("no message");
                    break;
                }
                var req = new MakeForumPostRequest { Text = message };
                if (!string.IsNullOrWhiteSpace(parentStr))
                {
                    req.Id = ByteString.CopyFrom(parentGuid.ToByteArray());
                }
                var postRes = commandClient.MakeForumPost(req);
                Console.WriteLine(postRes.ToString());
            }
            break;
        case 'R':
        case 'r':
            next = true;
            {
                Console.WriteLine("Enter parent guid");
                Guid parentGuid = default;
                var parentStr = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(parentStr) && !Guid.TryParse(parentStr, out parentGuid))
                {
                    Console.WriteLine("Error parsing");
                    break;
                }
                Console.WriteLine("Enter depth (default 1)");
                string? depthStr = Console.ReadLine();
                int depthInt = default;
                if (string.IsNullOrWhiteSpace(depthStr))
                {
                    depthInt = 1;
                }
                else if (!int.TryParse(depthStr, out depthInt))
                {
                    Console.WriteLine("Error parsing");
                    break;
                }
                var req = new ForumChildRequest { Depth = depthInt };
                if (!string.IsNullOrWhiteSpace(parentStr))
                {
                    req.Id = ByteString.CopyFrom(parentGuid.ToByteArray());
                }
                var posts = queryClient.ListForumChildren(req).Posts;
                try
                {
                    Console.WriteLine(posts.ToString());
                    Console.WriteLine(posts.Count());
                    foreach (var m in posts)
                    {
                        Console.WriteLine(m.ToString());
                        Console.WriteLine($"ID: {new Guid(m.Id.ToByteArray())}");
                    }
                }
                catch
                {
                    Console.WriteLine(queryClient.ListForumChildren(req));
                }
            }
            break;
        default:
            next = false;
            break;
    }

} while (next);

var res = commandClient.Logout(new LogoutRequest());
Console.WriteLine(res.DataCase);
