using Google.Protobuf;

using Grpc.Core;

using GrpcTest.Forum.Service;

using Host.Models;
namespace Host.Services;
/// <summary>
/// Service to handle login, logout, forum post and message requests 
/// </summary>
public class ForumCommandService : CommandService.CommandServiceBase
{
    private readonly ForumRepository _forumRepository;

    public ForumCommandService(ForumRepository forumRepository)
    {
        _forumRepository = forumRepository;
    }

    public override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var u = _forumRepository.GetUser("admin");
        return Task.Run(() => new LoginResponse()
        {
            User = new GrpcTest.Forum.Messages.User
            {
                Username = u.username,
                Id = ByteString.CopyFrom(u.Id.ToByteArray())
            }
        });
    }
    public override Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        return base.Logout(request, context);
    }
    public override Task<MakeForumPostResponse> MakeForumPost(MakeForumPostRequest request, ServerCallContext context)
    {
        return base.MakeForumPost(request, context);
    }
    public override Task<SendDmRequestResponse> SendDm(SendDmRequest request, ServerCallContext context)
    {
        return base.SendDm(request, context);
    }
}