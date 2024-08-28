using Fei.Is.Api.Data.Enums;
using ProtoBuf;

namespace Fei.Is.Api.MqttClient.Proto;

[ProtoContract]
class JobProto
{
    [ProtoMember(1)]
    public required string JobId { get; set; }

    [ProtoMember(2)]
    public required List<CommandProto> Commands { get; set; } = [];

    [ProtoMember(3)]
    public required JobStatusEnum Status { get; set; }

    [ProtoMember(4)]
    public required string Name { get; set; }

    [ProtoMember(5)]
    public int CurrentStep { get; set; }

    [ProtoMember(6)]
    public int TotalSteps { get; set; }

    [ProtoMember(7)]
    public int CurrentCycle { get; set; }

    [ProtoMember(8)]
    public int TotalCycles { get; set; }

    [ProtoMember(9)]
    public bool Paused { get; set; }

    [ProtoMember(10)]
    public DateTime? StartedAt { get; set; }

    [ProtoMember(11)]
    public DateTime? FinishedAt { get; set; }
}
