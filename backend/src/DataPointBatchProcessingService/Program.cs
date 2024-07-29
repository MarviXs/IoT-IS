using Fei.Is.DataPointBatchProcessingService;
using Fei.Is.DataPointBatchProcessingService.Protos;
using Fei.Is.DataPointBatchProcessingService.Services;

var builder = Host.CreateApplicationBuilder(args);
var grpcAddress = builder.Configuration.GetSection("GrpcSettings:DataPointGrpcAddress").Value;
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddGrpcClient<StoreDataService.StoreDataServiceClient>(o =>
{
    o.Address = new Uri(grpcAddress);
});

// builder.Logging.ClearProviders();
// builder.Logging.AddConsole();

var host = builder.Build();
host.Run();
