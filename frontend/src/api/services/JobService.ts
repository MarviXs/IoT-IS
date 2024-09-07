import { client, customFetch, baseUrl, createServerEventSource } from '@/api/client';
import { ActiveJobsResponse, JobsQueryParams, StartJobRequest } from '../types/Job';
import { EventSourceMessage } from 'eventsource-client';

class JobService {
  async getActiveJobs(deviceId: string) {
    return await client.GET('/devices/{deviceId}/jobs/active', { params: { path: { deviceId } } });
  }

  getActiveJobsEventSource(deviceId: string) {
    return createServerEventSource(`devices/${deviceId}/jobs/active/sse`);
  }

  async getJob(jobId: string) {
    return await client.GET('/jobs/{jobId}', { params: { path: { jobId } } });
  }

  async getJobs(queryParams?: JobsQueryParams) {
    return await client.GET('/jobs', { params: { query: queryParams } });
  }

  async startJob(deviceId: string, body: StartJobRequest) {
    return await client.POST('/devices/{deviceId}/jobs', { params: { path: { deviceId } }, body });
  }

  async cancelJob(jobId: string) {
    return await client.PUT('/jobs/{jobId}/cancel', { params: { path: { jobId } } });
  }

  async pauseJob(jobId: string) {
    return await client.PUT('/jobs/{jobId}/pause', { params: { path: { jobId } } });
  }

  async resumeJob(jobId: string) {
    return await client.PUT('/jobs/{jobId}/resume', { params: { path: { jobId } } });
  }

  async skipStep(jobId: string) {
    return await client.PUT('/jobs/{jobId}/skip-step', { params: { path: { jobId } } });
  }

  async skipCycle(jobId: string) {
    return await client.PUT('/jobs/{jobId}/skip-cycle', { params: { path: { jobId } } });
  }
}

export default new JobService();
