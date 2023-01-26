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
        ForumChildRequest request, ServerCallContext context) => Task.Run(async () =>
    {
        Guid? reqGuid;
        BoardPost[] posts;
        if (request.Id == ByteString.Empty)
        {
            reqGuid = null;
            posts = _forumRepository.ListBoardPosts(null, 1);
        }
        else
        {
            reqGuid = new Guid(request.Id.ToByteArray());
            posts = _forumRepository.ListBoardPosts(reqGuid, 1);
        }

        var res = new ForumPostList();
        res.Posts.AddRange(posts.Select(p =>
            new GrpcTest.Forum.Messages.BoardPost
            {
                Message = new GrpcTest.Forum.Messages.Message
                {
                    Author = new GrpcTest.Forum.Messages.User
                    {
                        Id = ByteString.CopyFrom(
                                    p.Author.ToByteArray()),
                        Username = _forumRepository.GetUser(
                                    p.Author)!.username

                    },
                    Text = p.Text,
                },
                Id = ByteString.CopyFrom(p.Id.ToByteArray())
            }
        ));
        var postBuffer = new List<GrpcTest.Forum.Messages.BoardPost>();
        if (request.Depth > 1)
        {
            foreach (var v in res.Posts)
            {
                var childRequest = new ForumChildRequest
                {
                    Depth = request.Depth - 1,
                    Id = v.Id
                };
                var children = await ListForumChildren(childRequest, context);
                foreach (var child in children.Posts)
                {
                    child.Parent = v;
                    postBuffer.Add(child);
                }
            }
            res.Posts.AddRange(postBuffer);
        }

        return res;
    });
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