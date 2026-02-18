using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using FluentResults;
using Google.FlatBuffers;
using JobFlatBuffers;

namespace Fei.Is.Api.MqttClient.Publish;

public class PublishJobControl(MqttClientService mqttClient, ILogger<PublishJobControl> logger)
{
    public async Task<Result> Execute(string accessToken, Guid jobId, JobControl jobControlEnum)
    {
        var topic = $"devices/{accessToken}/job/control";

        var builder = new FlatBufferBuilder(1);
        var jobIdOffset = builder.CreateString(jobId.ToString());
        var jobControlEnumFbs = ConvertToJobControlEnumFbs(jobControlEnum);

        JobControlFbs.StartJobControlFbs(builder);
        JobControlFbs.AddJobId(builder, jobIdOffset);
        JobControlFbs.AddControl(builder, jobControlEnumFbs);
        var jobControl = JobControlFbs.EndJobControlFbs(builder);
        builder.Finish(jobControl.Value);
        var buffer = builder.SizedByteArray();

        var isPublished = await mqttClient.TryPublishAsync(topic, buffer, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
        if (isPublished)
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
