namespace Host.Models;
public class ForumRepository
{
    private readonly List<User> _users;
    private readonly List<DirectMessage> _messages;
    private readonly List<BoardPost> _boardPosts;
    private readonly Dictionary<string, User> _logins;
    public event EventHandler<DirectMessage>? DirectMessageHandler;
    public event EventHandler<BoardPost>? BoardPostHandler;

    public ForumRepository()
    {
        _logins = new();
        _users = new();
        _messages = new();
        _boardPosts = new();
        var admin = new User("admin");
        var streamer = new User("streamer");
        _users.Add(admin);
        _users.Add(streamer);
        _boardPosts.Add(new BoardPost(admin.Id, null, "Master Post")
        {
            Id = Guid.Parse("67c3f052-3f02-4e23-ae97-bcc02e658b55")
        });
    }
    public User? GetLogin(string key)
    {
        if (_logins.ContainsKey(key))
        {
            return _logins[key];
        }
        else
        {
            return null;
        }
    }
    public User Login(string key, string username)
    {
        var u = GetUser(username);
        if (u == null)
        {
            throw new ArgumentException("User does not exist");
        }
        _logins[key] = u;
        return u;
    }
    public User Logout(string key)
    {
        if (_logins.ContainsKey(key))
        {
            var user = _logins[key];
            _logins.Remove(key);
            return user;
        }
        throw new ArgumentException("Session does not exist");
    }
    public User? GetUser(string username)
    {
        return _users.SingleOrDefault(u => u.username == username);
    }
    public User CreateUser(string username)
    {
        if (_users.Any(u => u.username == username))
        {
            throw new ArgumentException("Username already taken");
        }
        var u = new User(username);
        _users.Add(u);
        return u;
    }
    public void DeleteUser(string username)
    {
        var u = GetUser(username);
        if (u == null)
        {
            throw new ArgumentException("User does not exist");
        }
        else
        {
            _users.Remove(u);
        }
    }
    public User? GetUser(Guid id)
    {
        return _users.SingleOrDefault(u => u.Id == id);
    }

    public void DeleteUser(Guid id)
    {
        var u = GetUser(id);
        if (u == null)
        {
            throw new ArgumentException("User does not exist");
        }
        else
        {
            _users.Remove(u);
        }
    }

    public BoardPost? GetBoardPost(Guid id)
    {
        return _boardPosts.SingleOrDefault(p => p.Id == id);
    }
    public BoardPost CreateBoardPost(
        User sender, string content, BoardPost? parent = null)
    {
        if (!_users.Any(u => u == sender))
        {
            throw new ArgumentException("User not found");
        }
        if (parent == null)
        {
            var a = new BoardPost(sender.Id, null, content);
            _boardPosts.Add(a);
            if (BoardPostHandler != null)
            {
                BoardPostHandler(this, a);
            }
            return a;
        }
        if (!_boardPosts.Any(p => p == parent))
        {
            throw new ArgumentException("Board post not found");
        }
        var res = new BoardPost(sender.Id, parent.Id, content);
        _boardPosts.Add(res);
        if (BoardPostHandler != null)
        {
            BoardPostHandler(this, res);
        }
        return res;

    }
    public BoardPost[] ListBoardPosts(Guid? root, int depth = int.MaxValue)
    {
        List<BoardPost> posts = new();
        if (depth < 1)
        {
            return posts.ToArray();
        }
        posts.AddRange(_boardPosts.Where(p => p.ParentId == root));
        List<BoardPost> next = new();
        foreach (var p in posts)
        {
            next.AddRange(ListBoardPosts(p.Id, depth - 1));
        }
        posts.AddRange(next);
        return posts.ToArray();
    }
    public DirectMessage[] ListDirectMessages(Guid? author, Guid? recipient)
    {
        IEnumerable<DirectMessage> res = _messages;
        if (author != null)
        {
            res = res.Where(m => m.Sender == author);
        }
        if (recipient != null)
        {
            res = res.Where(m => m.Recipient == recipient);
        }
        return res.ToArray();
    }

    public void CreateDirectMessage(
        User sender, User recipient, string content)
    {
        if (_users.Any(u => u == sender) && _users.Any(u => u == recipient))
        {
            var m = new DirectMessage(sender.Id, recipient.Id, content);
            _messages.Add(m);
            if (DirectMessageHandler != null)
            {
                DirectMessageHandler(this, m);
            }
        }
        else
        {
            throw new ArgumentException("User does not exist");
        }
    }
}