public record User(string username)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
