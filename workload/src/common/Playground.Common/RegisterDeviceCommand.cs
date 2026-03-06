using Playground.Common.Messaging.Types;

namespace Playground.Common;

[CommandNamespace("datalab.commands.register")]
public record RegisterDeviceCommand(Guid DeviceId, string DeviceName);

[CommandNamespace("datalab.commands.register.response")]
public record RegistrationResponse(Guid DeviceId, string DeviceName, string DeviceState, string DeviceUrl);