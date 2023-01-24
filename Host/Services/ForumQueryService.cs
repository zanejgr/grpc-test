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
    public override Task<ForumPostList> ListForumChildren(ForumChildRequest request, ServerCallContext context)
    {
        return base.ListForumChildren(request, context);
    }
    public override Task<DirectMessageList> ListInbox(ListInboxRequest request, ServerCallContext context)
    {
        return base.ListInbox(request, context);
    }
    public override Task OpenStream(OpenStreamRequest request, IServerStreamWriter<Event> responseStream, ServerCallContext context)
    {
        return base.OpenStream(request, responseStream, context);
    }
}