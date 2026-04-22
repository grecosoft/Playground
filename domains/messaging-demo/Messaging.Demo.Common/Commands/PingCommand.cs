using System.Text.Json.Serialization;
using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("commands.ping")]
public record PingCommand(
    [property: JsonPropertyName("valueOne")] string ValueOne, 
    [property: JsonPropertyName("valueTwo")] string ValueTwo) : ICommandMessage<PingResponse>
{
    public PingResponse? Response { get; set; }
}

public record PingResponse(
    [property: JsonPropertyName("pingValue")] string PingValue);