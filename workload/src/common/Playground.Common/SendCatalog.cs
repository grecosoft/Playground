namespace Playground.Common;

public class SendCatalog(
    string correlationId,
    string value1,
    string value2): Message(correlationId)
{
    public string Value1 { get; set; } = value1;
    public string Value2 { get; set; } = value2;
}