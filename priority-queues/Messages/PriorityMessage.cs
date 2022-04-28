namespace Messages;

public interface IPriorityMessage
{
    byte Priority { get; }
}

public record PriorityMessage(string Id, byte Priority) : IPriorityMessage;