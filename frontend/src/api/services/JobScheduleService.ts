import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type JobSchedulesResponse =
  paths['/devices/{deviceId}/job-schedules']['get']['responses']['200']['content']['application/json'];
export type JobScheduleResponse =
  paths['/job-schedules/{scheduleId}']['get']['responses']['200']['content']['application/json'];
export type CreateJobScheduleRequest =
  paths['/devices/{deviceId}/job-schedules']['post']['requestBody']['content']['application/json'];
export type UpdateJobScheduleRequest =
  paths['/job-schedules/{scheduleId}']['put']['requestBody']['content']['application/json'];

class JobScheduleService {
  async getSchedules(deviceId: string) {
    return await client.GET('/devices/{deviceId}/job-schedules', {
      params: { path: { deviceId } },
    });
  }

  async createSchedule(deviceId: string, body: CreateJobScheduleRequest) {
    return await client.POST('/devices/{deviceId}/job-schedules', {
      params: { path: { deviceId } },
      body,
    });
  }

  async updateSchedule(scheduleId: string, body: UpdateJobScheduleRequest) {
    return await client.PUT('/job-schedules/{scheduleId}', {
      params: { path: { scheduleId } },
      body,
    });
  }

  async deleteSchedule(scheduleId: string) {
    return await client.DELETE('/job-schedules/{scheduleId}', {
      params: { path: { scheduleId } },
    });
  }

  async getSchedule(scheduleId: string) {
    return await client.GET('/job-schedules/{scheduleId}', {
      params: { path: { scheduleId } },
    });
  }
}

export default new JobScheduleService();
