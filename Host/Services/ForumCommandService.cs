using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

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
        try
        {
            string username = request.UserRequest.KeyCase switch
            {
                UserRequest.KeyOneofCase.Id => _forumRepository.GetUser(
                    new Guid(request.UserRequest.Id.ToByteArray()))?.username
                    ?? throw new ArgumentException("no user with that ID"),
                UserRequest.KeyOneofCase.Username =>
                request.UserRequest.Username,
                _ => throw new NotImplementedException(),
            };
            var u = _forumRepository.Login(key: context.Peer,
             username: request.UserRequest.Username);
            Console.WriteLine(context.Peer);
            return Task.Run(() => new LoginResponse()
            {
                User = new GrpcTest.Forum.Messages.User
                {
                    Username = u.username,
                    Id = ByteString.CopyFrom(u.Id.ToByteArray())
                }
            });
        }
        catch (ArgumentException e)
        {
            return Task.Run(() => new LoginResponse
            {
                Error = new GrpcTest.Common.Error
                {
                    Code = GrpcTest.Common.ErrorCode.NotFound,
                    Message = e.ToString(),
                    Data = Any.Pack(request)
                }
            });
        }
        catch (NotImplementedException)
        {
            return Task.Run(() => new LoginResponse
            {
                Error = new GrpcTest.Common.Error
                {
                    Code = GrpcTest.Common.ErrorCode.UndefinedError,
                    Message = "Not implemented",
                    Data = Any.Pack(request)
                }
            });
        }
        catch (NullReferenceException)
        {
            return Task.Run(() => new LoginResponse
            {
                Error = new GrpcTest.Common.Error
                {
                    Code = GrpcTest.Common.ErrorCode.BadRequest,
                    Message = "A required field was missing",
                    Data = Any.Pack(request)
                }
            });
        }
    }
    public override Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        try
        {
            _forumRepository.Logout(context.Peer);
            return Task.Run(() => new LogoutResponse
            {
                Empty = new Empty()
            });
        }
        catch (ArgumentException e)
        {
            return Task.Run(() => new LogoutResponse
            {
                Error = new GrpcTest.Common.Error
                {
                    Code = GrpcTest.Common.ErrorCode.NotFound,
                    Message = e.ToString(),
                }
            });
        }

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