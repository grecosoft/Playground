using Playground.Common.Messaging.Types;

namespace Playground.Common;

public static class SolutionServices
{
    public const string SolutionOne = "service-one";
    public const string SolutionTwo = "service-two";
}

[MessageNamespace(SolutionServices.SolutionOne, "commands.ping")]
public record PingCommand(string ExpectedValue) : ICommandMessage<PingResponse>
{
    
}

public record PingResponse(string ActualValue);