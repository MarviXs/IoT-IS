import { client } from '@/api/client';
import { JobsQueryParams, StartJobRequest } from '../types/Job';

class JobService {
  async getActiveJob(deviceId: string) {
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

  async cancelJob(deviceId: string) {
    return await client.GET('/devices/{deviceId}/jobs/active', { params: { path: { deviceId } } });
  }

  async pauseJob(deviceId: string) {
    return await client.GET('/devices/{deviceId}/jobs/active', { params: { path: { deviceId } } });
  }

  async continueJob(deviceId: string) {
    return await client.GET('/devices/{deviceId}/jobs/active', { params: { path: { deviceId } } });
  }

  async skipStep(deviceId: string) {
    return await client.GET('/devices/{deviceId}/jobs/active', { params: { path: { deviceId } } });
  }

  async skipCycle(deviceId: string) {
    return await client.GET('/devices/{deviceId}/jobs/active', { params: { path: { deviceId } } });
  }
}

export default new JobService();
