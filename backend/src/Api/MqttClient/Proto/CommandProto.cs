using ProtoBuf;

namespace Fei.Is.Api.MqttClient.Proto;

[ProtoContract]
class CommandProto
{
    [ProtoMember(1)]
    public required string Name { get; set; }

    [ProtoMember(2)]
    public List<double> Params { get; set; } = [];
}
