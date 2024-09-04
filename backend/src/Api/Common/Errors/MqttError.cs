using FluentResults;

namespace Fei.Is.Api.Common.Errors;

public class MqttError : Error
{
    public MqttError()
        : base("MQTT error") { }
}
