using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using FlatSharp;
using FluentResults;
using JobFlatBuffers;

namespace Fei.Is.Api.MqttClient.Publish;

public class PublishJobControl(MqttClientService mqttClient, AppDbContext context, ILogger<PublishJobControl> logger)
{
    public async Task<Result> Execute(string accessToken, Guid jobId, JobControl jobControlEnum)
    {
        var topic = $"devices/{accessToken}/job/control";

        var jobControl = new JobControlFbs { JobId = jobId.ToString(), Control = ConvertToJobControlEnumFbs(jobControlEnum) };

        int maxSize = JobControlFbs.Serializer.GetMaxSize(jobControl);
        byte[] buffer = new byte[maxSize];
        int bytesWritten = JobControlFbs.Serializer.Write(buffer, jobControl);

        var result = await mqttClient.PublishAsync(topic, buffer, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
        if (result.IsSuccess)
        {
            return Result.Ok();
        }

        return Result.Fail(new MqttError());
    }

    private static JobControlEnumFbs ConvertToJobControlEnumFbs(JobControl jobControl)
    {
        return jobControl switch
        {
            JobControl.JOB_PAUSE => JobControlEnumFbs.JOB_PAUSE,
            JobControl.JOB_RESUME => JobControlEnumFbs.JOB_RESUME,
            JobControl.JOB_SKIP_STEP => JobControlEnumFbs.JOB_SKIP_STEP,
            JobControl.JOB_SKIP_CYCLE => JobControlEnumFbs.JOB_SKIP_CYCLE,
            JobControl.JOB_CANCEL => JobControlEnumFbs.JOB_CANCEL,
            _ => throw new ArgumentOutOfRangeException(nameof(jobControl), jobControl, null)
        };
    }
}
