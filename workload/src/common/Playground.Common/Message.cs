namespace Playground.Common;

public class Message(string correlationId)
{
    public string CorrelationId { get; set; } = correlationId;
}

