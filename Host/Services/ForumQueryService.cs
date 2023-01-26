using Google.Protobuf;

using Grpc.Core;

using GrpcTest.Forum.Service;

using Host.Models;
namespace Host.Services;
/// <summary>
/// Service to handle queries and subscriptions
/// </summary>
public class ForumQueryService : QueryService.QueryServiceBase
{
    private readonly ForumRepository _forumRepository;

    public ForumQueryService(ForumRepository forumRepository)
    {
        _forumRepository = forumRepository;
    }
    public override Task<ForumPostList> ListForumChildren(
        ForumChildRequest request, ServerCallContext context)
    {
        return base.ListForumChildren(request, context);
    }
    public override Task<DirectMessageList> ListInbox(
        ListInboxRequest request, ServerCallContext context) => Task.Run(() =>
    {
        var u = _forumRepository.GetLogin(context.Peer);
        if (u == null)
        {
            return new DirectMessageList();
        }
        var res = new DirectMessageList();
        res.Messages.AddRange(
            _forumRepository.ListDirectMessages(null, u.Id)
            .Select(m => new GrpcTest.Forum.Messages.DirectMessage
            {
                Message = new GrpcTest.Forum.Messages.Message
                {
                    Author = new GrpcTest.Forum.Messages.User
                    {
                        Id = ByteString.CopyFrom(m.Sender.ToByteArray()),
                        Username = _forumRepository.GetUser(m.Sender)!.username
                    },
                    Text = m.Text
                },
                Recipient = new GrpcTest.Forum.Messages.User
                {
                    Id = ByteString.CopyFrom(m.Recipient.ToByteArray()),
                    Username = _forumRepository.GetUser(m.Recipient)!.username
                }
            }));
        return res;
    });
    public override Task OpenStream(
        OpenStreamRequest request,
        IServerStreamWriter<Event> responseStream,
        ServerCallContext context)
    {
        return base.OpenStream(request, responseStream, context);
    }
}