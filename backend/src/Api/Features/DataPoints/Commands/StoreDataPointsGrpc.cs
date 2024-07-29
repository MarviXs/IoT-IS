using Fei.Is.Api.Protos;
using Grpc.Core;

namespace Fei.Is.Api.Features.DataPoints.Commands;

public class StoreDataPointsGrpc(ILogger<StoreDataPointsGrpc> logger) : StoreDataService.StoreDataServiceBase
{
    public override Task<StoreDataResponse> StoreData(StoreDataRequest request, ServerCallContext context)
    {
        logger.LogInformation(request.DataPoints.Count.ToString());
        logger.LogInformation("Storing data points");
        Console.WriteLine("Storing data points");

        return Task.FromResult(new StoreDataResponse { Success = true });
    }
}
