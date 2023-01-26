namespace Host.Models;
public sealed record BoardPost(
    Guid Author, Guid? ParentId, string Text) : Message(Text)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
