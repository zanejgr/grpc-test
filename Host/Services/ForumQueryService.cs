using Google.Protobuf;
using System.Diagnostics.CodeAnalysis;

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
    public override async Task OpenStream(
        OpenStreamRequest request,
        IServerStreamWriter<Event> responseStream,
        ServerCallContext context)
    {
        var currentUser = _forumRepository.GetLogin(context.Peer);
        if (currentUser == null)
        {
            await responseStream.WriteAsync(new GrpcTest.Forum.Service.Event
            {
                Action = GrpcTest.Common.Action.UndefinedAction
            });
            return;
        }
        var userIds = request.Users.Select(u => u.KeyCase switch
        {
            UserRequest.KeyOneofCase.Id => new Guid(u.Id.ToByteArray()),
            UserRequest.KeyOneofCase.Username =>
                _forumRepository.GetUser(u.Username)!.Id,
            _ => Guid.Empty
        });
        var forums = request.Forums;
        List<Action<object?, DirectMessage>> msgEvents = new();
        List<Action<object?, BoardPost>> postEvents = new();
        if (userIds.Count() == 0)
        {
            var dlg = handle_DirectMessage_delegate(
                currentUser.Id, null, responseStream);
            msgEvents.Add(dlg);
        }
        foreach (var u in userIds)
        {
            var dlg = handle_DirectMessage_delegate(
                currentUser.Id, u, responseStream);
            msgEvents.Add(dlg);
        }
        if (forums.Count == 0)
        {
            var dlg = handle_ForumPost_delegate(null, responseStream);
            postEvents.Add(dlg);
        }
        foreach (var f in request.Forums)
        {
            var dlg = handle_ForumPost_delegate(null, responseStream);
            postEvents.Add(dlg);
        }
        foreach (var dlg in msgEvents)
        {
            _forumRepository.DirectMessageHandler += dlg.Invoke;
        }
        foreach (var dlg in postEvents)
        {
            _forumRepository.BoardPostHandler += dlg.Invoke;
        }
        while (!context.CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100);
        }
        foreach (var dlg in msgEvents)
        {
            _forumRepository.DirectMessageHandler -= dlg.Invoke;
        }
        foreach (var dlg in postEvents)
        {
            _forumRepository.BoardPostHandler -= dlg.Invoke;
        }

    }
    private Action<object?, DirectMessage> handle_DirectMessage_delegate(
        Guid current,
        Guid? other,
        IServerStreamWriter<GrpcTest.Forum.Service.Event> stream)
    {
        var recipient = new GrpcTest.Forum.Messages.User
        {
            Id = ByteString.CopyFrom(current.ToByteArray()),
            Username = _forumRepository.GetUser(current)?.username ?? ""
        };
        return async (object? sender, DirectMessage dm) =>
        {
            if (current == dm.Recipient && (
                other == null || dm.Sender == other))
            {
                await stream.WriteAsync(new GrpcTest.Forum.Service.Event
                {
                    Action = GrpcTest.Common.Action.Created,
                    DirectMessage = new GrpcTest.Forum.Messages.DirectMessage
                    {
                        Message = new GrpcTest.Forum.Messages.Message
                        {
                            Author = new GrpcTest.Forum.Messages.User
                            {
                                Id = ByteString.CopyFrom(
                                    dm.Sender.ToByteArray()),
                                Username = _forumRepository.GetUser(
                                    dm.Sender)?.username ?? ""
                            },
                            Text = dm.Text
                        },
                        Recipient = recipient
                    }
                });
            }
        };
    }
    private Action<object?, BoardPost> handle_ForumPost_delegate(
        Guid? parent,
        IServerStreamWriter<GrpcTest.Forum.Service.Event> stream
    )
    {
        GrpcTest.Forum.Messages.BoardPost parentPost = new();
        if (parent is not null)
        {
            var repoPost = _forumRepository.GetBoardPost(
                parent.Value) ?? new(default, default, "");
            parentPost.Id = ByteString.CopyFrom(parent.Value.ToByteArray());
            parentPost.Message = new GrpcTest.Forum.Messages.Message
            {
                Author = new GrpcTest.Forum.Messages.User
                {
                    Id = ByteString.CopyFrom(repoPost.Author.ToByteArray()),
                    Username = _forumRepository.GetUser(
                            repoPost.Author)?.username ?? ""
                },
                Text = repoPost.Text
            };
        };

        return async (object? sender, BoardPost post) =>
        {
            if (parent == null || post.ParentId == parent)
            {
                await stream.WriteAsync(new GrpcTest.Forum.Service.Event
                {
                    Post = new GrpcTest.Forum.Messages.BoardPost
                    {
                        Parent = parentPost,
                        Id = ByteString.CopyFrom(post.Id.ToByteArray()),
                        Message = new GrpcTest.Forum.Messages.Message
                        {
                            Author = new GrpcTest.Forum.Messages.User
                            {
                                Id = ByteString.CopyFrom(post.Author.ToByteArray()),
                                Username = _forumRepository.GetUser(
                                post.Author)!.username
                            }
                        }
                    },
                    Action = GrpcTest.Common.Action.Created
                });
            }
        };
    }
}