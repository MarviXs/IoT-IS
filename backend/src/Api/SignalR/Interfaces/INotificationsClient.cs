using Fei.Is.Api.SignalR.Dtos;

namespace Fei.Is.Api.SignalR.Interfaces;

public interface INotificationsClient
{
    Task ReceiveJobUpdate(JobUpdateDto jobUpdate);
    Task ReceiveSensorLastDataPoint(SensorLastDataPointDto sensorLastDataPoint);
}
