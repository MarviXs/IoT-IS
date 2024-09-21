using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.SignalR.Dtos;

public record JobUpdateDto(
    Guid id,
    Guid deviceId,
    string name,
    int totalSteps,
    int totalCycles,
    int currentStep,
    int currentCycle,
    string currentCommand,
    bool paused,
    double progress,
    JobStatusEnum status
);
