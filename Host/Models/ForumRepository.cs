namespace Host.Models;
public class ForumRepository
{
    private readonly List<User> _users;
    private readonly List<DirectMessage> _messages;
    private readonly List<BoardPost> _boardPosts;

    public ForumRepository()
    {
        _users = new();
        _messages = new();
        _boardPosts = new();
        var admin = new User("admin");
        _users.Add(admin);
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
}