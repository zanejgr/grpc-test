namespace Host.Models;
public sealed record DirectMessage(
    Guid Sender, Guid Recipient, string Message) : Message(Message)
{ }
