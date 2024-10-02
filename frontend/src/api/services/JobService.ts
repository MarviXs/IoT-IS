import { client } from '@/api/client';
import type { components, paths } from '@/api/generated/schema.d.ts';

export type ActiveJobsResponse =
  paths['/devices/{deviceId}/jobs/active']['get']['responses']['200']['content']['application/json'];
export type StartJobRequest = paths['/devices/{deviceId}/jobs']['post']['requestBody']['content']['application/json'];
export type JobsQueryParams = paths['/jobs']['get']['parameters']['query'];
export type JobResponse = paths['/jobs/{jobId}']['get']['responses']['200']['content']['application/json'];
export type JobsResponse = paths['/jobs']['get']['responses']['200']['content']['application/json'];
export type JobStatus = components['schemas']['Fei.Is.Api.Data.Enums.JobStatusEnum'];

class JobService {
  async getActiveJobs(deviceId: string) {
    return await client.GET('/devices/{deviceId}/jobs/active', { params: { path: { deviceId } } });
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
