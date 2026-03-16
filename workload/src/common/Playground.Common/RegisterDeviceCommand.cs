using Playground.Common.Messaging.Types;

namespace Playground.Common;

[MessageNamespace("solution-two", "datalab.commands.register")]
public record RegisterDeviceCommand(Guid DeviceId, string DeviceName);

