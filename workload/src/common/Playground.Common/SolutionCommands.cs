using Playground.Common.Messaging.Types;

namespace Playground.Common;

public static class SolutionServices
{
    public const string SolutionOne = "service-one";
    public const string SolutionTwo = "service-two";
}

[MessageNamespace(
    SolutionServices.SolutionTwo,
    "commands.ping",
    DispatchStrategyType.Rpc)]
public record PingCommand(string ValueOne, string ValueTwo) : ICommandMessage<PingResponse>
{
    public PingResponse? Response { get; set; }
}

public record PingResponse(string PingValue);

[MessageNamespace(
    SolutionServices.SolutionTwo,
    "commands.device.status",
    DispatchStrategyType.Async)]
public record DeviceUpdate(string DeviceId) : ICommandMessage<DeviceStatus>
{
    public DeviceStatus? Response { get; set; }
}

public record DeviceStatus(bool IsEnabled);