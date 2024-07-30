using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Protos;
using Grpc.Core;

namespace Fei.Is.Api.Features.DataPoints.Commands;

public class StoreDataPointsGrpc(TimeScaleDbContext timescaleDb, ILogger<StoreDataPointsGrpc> logger) : StoreDataService.StoreDataServiceBase
{
    public override Task<StoreDataResponse> StoreData(StoreDataRequest request, ServerCallContext context)
    {
        var datapoints = new List<Data.Models.DataPoint>();

        foreach (var dp in request.DataPoints)
        {
            try
            {
                var dataPoint = new Data.Models.DataPoint
                {
                    SensorTag = dp.SensorTag,
                    DeviceId = Guid.Parse(dp.DeviceId),
                    TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(dp.Timestamp),
                    Value = dp.Value,
                };

                datapoints.Add(dataPoint);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to parse data point");
            }
        }

        if (datapoints.Count > 0)
        {
            timescaleDb.BulkInsert(datapoints);
        }

        return Task.FromResult(new StoreDataResponse { Success = true });
    }
}
